// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

namespace Lithindium.Identity.EntityFrameworkCore {

    /// <summary>
    /// Scoped, serializable relational identity store for SQLite and PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Do not share its context or run concurrent operations. Disable provider-level
    /// retries; retry the entire identity operation after a known serialization abort.
    /// </remarks>
    public sealed class EntityFrameworkUserStore : IUserEmailStore, IUserPhoneStore, IUserLockoutStore, IUserTotpStore,
        IUserRecoveryCodeStore, IUserExternalIdentityStore, IUserSessionStore, IUserChallengeStore, IUserAuditStore {

        private readonly IdentityDbContext _db;
        private StoreTransaction? _transaction;

        /// <summary>
        /// Creates a store over a dedicated identity context.
        /// </summary>
        public EntityFrameworkUserStore(IdentityDbContext db) {
            ArgumentNullException.ThrowIfNull(db);
            if (db.Database.ProviderName is not ("Microsoft.EntityFrameworkCore.Sqlite" or "Npgsql.EntityFrameworkCore.PostgreSQL"))
                throw new NotSupportedException("Use the SQLite or PostgreSQL identity provider.");
            if (db.Database.CreateExecutionStrategy().RetriesOnFailure)
                throw new InvalidOperationException("Disable provider-level retries for identity transactions.");
            _db = db;
        }

        /// <inheritdoc />
        public async Task<IUserStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) {
            if (_transaction is not null || _db.Database.CurrentTransaction is not null)
                throw new InvalidOperationException("Nested identity transactions are not supported.");
            IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken).ConfigureAwait(false);
            return _transaction = new StoreTransaction(this, transaction);
        }

        private IQueryable<T> Query<T>() where T : class {
            if (_transaction is { Completed: true }) throw new InvalidOperationException("Identity transaction completed.");
            return _db.Set<T>().AsNoTracking();
        }

        private async Task<bool> ExistsAsync(User user, CancellationToken ct) =>
            await Query<UserRow>().AnyAsync(x => x.Uuid == user.Uuid, ct).ConfigureAwait(false);

        private async Task<bool> RememberAsync(int kind, Guid uuid, CancellationToken ct) {
            if (uuid == Guid.Empty) throw new ArgumentException("UUID cannot be empty.");
            if (await Query<IdentifierRow>().AnyAsync(x => x.Kind == kind && x.Uuid == uuid, ct).ConfigureAwait(false)) return false;
            _db.Add(new IdentifierRow { Kind = kind, Uuid = uuid });
            return true;
        }

        private Task<int> SaveAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

        private async Task<OperationResult> WriteAsync(OperationCode op, Func<Task<OperationResult>> action, CancellationToken ct) {
            StoreTransaction? owned = null;
            try {
                if (_transaction is null) owned = (StoreTransaction)await BeginTransactionAsync(ct).ConfigureAwait(false);
                if (_transaction!.Completed) throw new InvalidOperationException("Identity transaction completed.");
                OperationResult result = await action().ConfigureAwait(false);
                if (result.IsFailure) return result;
                await SaveAsync(ct).ConfigureAwait(false);
                if (owned is not null) {
                    OperationResult commit = await owned.CommitAsync(ct).ConfigureAwait(false);
                    if (commit.IsFailure) return new OperationResult(commit.Status, commit.Context, op, commit.Detail);
                }
                return result;
            } catch (Exception exception) when (IsConflict(exception) || IsUnique(exception)) {
                if (_transaction is { Completed: false } transaction) await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                // A uniqueness race must be retried from fresh state before classifying it.
                return Fail(StatusCode.Conflict, ContextCode.None, op);
            } catch {
                if (_transaction is { Completed: false } transaction) await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            } finally {
                _db.ChangeTracker.Clear();
                if (owned is not null) await owned.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static bool IsConflict(Exception exception) =>
            exception is DbUpdateConcurrencyException ||
            exception is SqliteException { SqliteErrorCode: 5 or 6 } ||
            exception is PostgresException { SqlState: "40001" or "40P01" } ||
            (exception.InnerException is { } inner && IsConflict(inner));

        private static bool IsUnique(Exception exception) =>
            exception is SqliteException { SqliteExtendedErrorCode: 1555 or 2067 } ||
            exception is PostgresException { SqlState: "23505" } ||
            (exception.InnerException is { } inner && IsUnique(inner));

        private static OperationResult Ok(OperationCode op) => new(StatusCode.Success, operation: op);
        private static OperationResult Fail(StatusCode status, ContextCode context, OperationCode op) => new(status, context, op);
        private static OperationResult MissingUser(OperationCode op) => Fail(StatusCode.NotFound, ContextCode.User, op);
        private static OperationResult MissingEntity(OperationCode op) => Fail(StatusCode.NotFound, ContextCode.Entity, op);
        private static OperationResult Duplicate(ContextCode context, OperationCode op) => Fail(StatusCode.AlreadyExists, context, op);

        private sealed class StoreTransaction(EntityFrameworkUserStore owner, IDbContextTransaction transaction) : IUserStoreTransaction {

            internal bool Completed { get; private set; }
            internal HashSet<Guid> Reserved { get; } = [];

            public async Task<OperationResult> CommitAsync(CancellationToken cancellationToken = default) {
                if (Completed) throw new InvalidOperationException("Transaction completed.");
                try {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    Completed = true;
                    return Ok(OperationCode.StoreCommit);
                } catch (Exception exception) when (IsConflict(exception) || IsUnique(exception)) {
                    await RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                    return Fail(StatusCode.Conflict, ContextCode.None, OperationCode.StoreCommit);
                } catch {
                    // The outcome is unknown. Never retry a possibly committed transaction.
                    Completed = true;
                    throw;
                }
            }

            public async Task RollbackAsync(CancellationToken cancellationToken = default) {
                if (Completed) return;
                try {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                } finally {
                    Completed = true;
                    owner._db.ChangeTracker.Clear();
                }
            }

            public async ValueTask DisposeAsync() {
                try {
                    if (!Completed) await RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                } finally {
                    try {
                        await transaction.DisposeAsync().ConfigureAwait(false);
                    } finally {
                        owner._transaction = null;
                        owner._db.ChangeTracker.Clear();
                    }
                }
            }

        }

        private static string Key(string value) => value.Normalize(NormalizationForm.FormC).ToUpperInvariant();
        private static long Ticks(DateTime value) {
            if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("Expected UTC time.");
            return value.Ticks;
        }
        private static DateTime Utc(long ticks) => new(ticks, DateTimeKind.Utc);
        private static User ToRecord(UserRow row) => new(row.Uuid, row.Username, row.Password, row.DisplayName, checked((ushort)row.Rotation), row.Status);
        private static UserEmail ToRecord(EmailRow row) => new(row.Address, row.PrimaryOwner.HasValue);
        private static UserPhone ToRecord(PhoneRow row) => new(row.Number, row.Extension.Length == 0 ? null : row.Extension, row.PrimaryOwner.HasValue);
        private static UserLockout ToRecord(LockoutRow row) => new(row.Enabled, checked((uint)row.Failures), row.ReleaseAt is { } release ? Utc(release) : null);
        private static UserTotp ToRecord(TotpRow row) => new(row.Uuid, row.Name, ImmutableArray.CreateRange(row.Secret), row.Algorithm, row.Digits, row.Period, row.LastStep is { } step ? ulong.Parse(step, CultureInfo.InvariantCulture) : null);
        private static UserRecoveryCode ToRecord(RecoveryRow row) => new(row.Uuid, ImmutableArray.CreateRange(row.CodeHash));
        private static UserExternalIdentity ToRecord(ExternalRow row) => new(row.Uuid, row.Name, row.Issuer, row.Subject, row.Email);
        private static UserSession ToRecord(SessionRow row) => new(row.Uuid, row.DeviceName, row.IpAddress, row.UserAgent, ImmutableArray.CreateRange(JsonSerializer.Deserialize<string[]>(row.Amr) ?? throw new InvalidOperationException("Invalid AMR.")), row.Acr, Utc(row.AuthenticatedAt), Utc(row.ExpiresAt));
        private static UserChallenge ToRecord(ChallengeRow row) => new(row.Uuid, row.Type, row.SecretHash is { } hash ? ImmutableArray.CreateRange(hash) : null, row.Payload is { } payload ? ImmutableArray.CreateRange(payload) : null, checked((uint)row.Attempts), checked((uint)row.MaxAttempts), Utc(row.CreatedAt), Utc(row.ExpiresAt));
        private static UserAudit ToRecord(AuditRow row) => new(row.Uuid, row.User, row.Actor, row.Entity, row.Type, row.Action, row.Info, Utc(row.Date));

        private static void Apply(UserRow row, User user) {
            row.Username = user.Username;
            row.UsernameKey = Key(user.Username);
            row.Password = user.Password;
            row.DisplayName = user.DisplayName;
            row.Rotation = user.Rotation;
            row.Status = user.Status;
        }

        private static string Step(ulong step) => step.ToString("D20", CultureInfo.InvariantCulture);

        private async IAsyncEnumerable<TRecord> ReadMany<T, TRecord>(IQueryable<T> query, Func<T, TRecord> convert,
            [EnumeratorCancellation] CancellationToken ct) where T : class {
            List<T> rows = await query.ToListAsync(ct).ConfigureAwait(false);
            foreach (T row in rows) {
                ct.ThrowIfCancellationRequested();
                yield return convert(row);
            }
        }

        /// <inheritdoc />
        public async Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default) {
            UserRow? row = await Query<UserRow>().SingleOrDefaultAsync(x => x.Uuid == uuid, cancellationToken).ConfigureAwait(false);
            return row is null ? null : ToRecord(row);
        }

        /// <inheritdoc />
        public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) {
            string key = Key(username);
            UserRow? row = await Query<UserRow>().SingleOrDefaultAsync(x => x.UsernameKey == key, cancellationToken).ConfigureAwait(false);
            return row is null ? null : ToRecord(row);
        }

        /// <inheritdoc />
        public Task<OperationResult> CreateAsync(User user, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreCreateUser;
            return WriteAsync(op, async () => {
                string key = Key(user.Username);
                if (await Query<UserRow>().AnyAsync(x => x.UsernameKey == key, cancellationToken).ConfigureAwait(false))
                    return Duplicate(ContextCode.Username, op);
                if (!await RememberAsync(1, user.Uuid, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Identifier, op);
                var row = new UserRow { Uuid = user.Uuid };
                Apply(row, user);
                _db.Add(row);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default) {
            if (expected.Uuid != user.Uuid) throw new ArgumentException("User UUID cannot change.");
            const OperationCode op = OperationCode.StoreUpdateUser;
            return WriteAsync(op, async () => {
                UserRow? row = await Query<UserRow>().SingleOrDefaultAsync(x => x.Uuid == user.Uuid, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingUser(op);
                if (ToRecord(row) != expected) return Fail(StatusCode.Conflict, ContextCode.None, op);
                string key = Key(user.Username);
                if (await Query<UserRow>().AnyAsync(x => x.Uuid != user.Uuid && x.UsernameKey == key, cancellationToken).ConfigureAwait(false))
                    return Duplicate(ContextCode.Username, op);
                _db.Attach(row);
                Apply(row, user);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> DeleteAsync(Guid uuid, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreDeleteUser;
            return WriteAsync(op, async () => {
                UserRow? row = await Query<UserRow>().SingleOrDefaultAsync(x => x.Uuid == uuid, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingUser(op);
                _db.Remove(row);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<UserEmail> GetEmailsAsync(User user, CancellationToken cancellationToken = default) =>
            ReadMany(Query<EmailRow>().Where(x => x.UserUuid == user.Uuid), ToRecord, cancellationToken);

        /// <inheritdoc />
        public async Task<User?> FindByEmailAsync(string address, CancellationToken cancellationToken = default) {
            string key = Key(address);
            EmailRow? row = await Query<EmailRow>().SingleOrDefaultAsync(x => x.AddressKey == key, cancellationToken).ConfigureAwait(false);
            return row is null ? null : await FindByUuidAsync(row.UserUuid, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<OperationResult> AddEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreAddEmail;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                string key = Key(email.Address);
                if (await Query<EmailRow>().AnyAsync(x => x.AddressKey == key, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Email, op);
                if (email.Primary) await ClearPrimaryEmailAsync(user, cancellationToken).ConfigureAwait(false);
                _db.Add(new EmailRow { Address = email.Address, AddressKey = key, UserUuid = user.Uuid, PrimaryOwner = email.Primary ? user.Uuid : null });
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> RemoveEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreRemoveEmail;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                string key = Key(email.Address);
                EmailRow? row = await Query<EmailRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.AddressKey == key, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                _db.Remove(row);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> SetPrimaryEmailAsync(User user, UserEmail? email, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreSetPrimaryEmail;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                string? key = email is { } value ? Key(value.Address) : null;
                EmailRow? row = email is null ? null : await Query<EmailRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.AddressKey == key, cancellationToken).ConfigureAwait(false);
                if (email is not null && row is null) return MissingEntity(op);
                if (row?.PrimaryOwner == user.Uuid) return new OperationResult(StatusCode.NoOp, operation: op);
                await ClearPrimaryEmailAsync(user, cancellationToken).ConfigureAwait(false);
                if (row is not null) {
                    _db.Attach(row);
                    row.PrimaryOwner = user.Uuid;
                }
                return Ok(op);
            }, cancellationToken);
        }

        private async Task ClearPrimaryEmailAsync(User user, CancellationToken ct) {
            await Query<EmailRow>().Where(x => x.UserUuid == user.Uuid && x.PrimaryOwner != null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.PrimaryOwner, (Guid?)null), ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<UserPhone> GetPhonesAsync(User user, CancellationToken cancellationToken = default) =>
            ReadMany(Query<PhoneRow>().Where(x => x.UserUuid == user.Uuid), ToRecord, cancellationToken);

        /// <inheritdoc />
        public async Task<User?> FindByPhoneAsync(string number, string? extension = null, CancellationToken cancellationToken = default) {
            string key = extension ?? string.Empty;
            PhoneRow? row = await Query<PhoneRow>().SingleOrDefaultAsync(x => x.Number == number && x.Extension == key, cancellationToken).ConfigureAwait(false);
            return row is null ? null : await FindByUuidAsync(row.UserUuid, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<OperationResult> AddPhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreAddPhone;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                string extension = phone.Extension ?? string.Empty;
                if (await Query<PhoneRow>().AnyAsync(x => x.Number == phone.Number && x.Extension == extension, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Phone, op);
                if (phone.Primary) await ClearPrimaryPhoneAsync(user, cancellationToken).ConfigureAwait(false);
                _db.Add(new PhoneRow { Number = phone.Number, Extension = extension, UserUuid = user.Uuid, PrimaryOwner = phone.Primary ? user.Uuid : null });
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> RemovePhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreRemovePhone;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                string extension = phone.Extension ?? string.Empty;
                PhoneRow? row = await Query<PhoneRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Number == phone.Number && x.Extension == extension, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                _db.Remove(row);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> SetPrimaryPhoneAsync(User user, UserPhone? phone, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreSetPrimaryPhone;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                string? number = phone?.Number;
                string extension = phone?.Extension ?? string.Empty;
                PhoneRow? row = phone is null ? null : await Query<PhoneRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Number == number && x.Extension == extension, cancellationToken).ConfigureAwait(false);
                if (phone is not null && row is null) return MissingEntity(op);
                if (row?.PrimaryOwner == user.Uuid) return new OperationResult(StatusCode.NoOp, operation: op);
                await ClearPrimaryPhoneAsync(user, cancellationToken).ConfigureAwait(false);
                if (row is not null) {
                    _db.Attach(row);
                    row.PrimaryOwner = user.Uuid;
                }
                return Ok(op);
            }, cancellationToken);
        }

        private async Task ClearPrimaryPhoneAsync(User user, CancellationToken ct) {
            await Query<PhoneRow>().Where(x => x.UserUuid == user.Uuid && x.PrimaryOwner != null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.PrimaryOwner, (Guid?)null), ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<UserLockout?> GetLockoutAsync(User user, CancellationToken cancellationToken = default) {
            LockoutRow? row = await Query<LockoutRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid, cancellationToken).ConfigureAwait(false);
            return row is null ? null : ToRecord(row);
        }

        /// <inheritdoc />
        public Task<OperationResult> TryUpdateLockoutAsync(User user, UserLockout? expected, UserLockout lockout, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreUpdateLockout;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                LockoutRow? row = await Query<LockoutRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid, cancellationToken).ConfigureAwait(false);
                UserLockout? actual = row is null ? null : ToRecord(row);
                if (actual != expected) return Fail(StatusCode.Conflict, ContextCode.None, op);
                if (row is null) {
                    row = new LockoutRow { UserUuid = user.Uuid };
                    _db.Add(row);
                } else _db.Attach(row);
                row.Enabled = lockout.Enabled;
                row.Failures = lockout.Failures;
                row.ReleaseAt = lockout.ReleaseAt is { } release ? Ticks(release) : null;
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<UserTotp> GetTotpsAsync(User user, CancellationToken cancellationToken = default) =>
            ReadMany(Query<TotpRow>().Where(x => x.UserUuid == user.Uuid), ToRecord, cancellationToken);

        /// <inheritdoc />
        public async Task<UserTotp?> GetTotpAsync(User user, Guid uuid, CancellationToken cancellationToken = default) {
            TotpRow? row = await Query<TotpRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == uuid, cancellationToken).ConfigureAwait(false);
            return row is null ? null : ToRecord(row);
        }

        /// <inheritdoc />
        public Task<OperationResult> AddTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreAddTotp;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                if (await Query<TotpRow>().AnyAsync(x => x.Uuid == totp.Uuid, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Identifier, op);
                _db.Add(new TotpRow { Uuid = totp.Uuid, UserUuid = user.Uuid, Name = totp.Name, Secret = totp.Secret.AsSpan().ToArray(),
                    Algorithm = totp.Algorithm, Digits = totp.Digits, Period = totp.Period, LastStep = totp.LastStep is { } step ? Step(step) : null });
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> RemoveTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default) =>
            RemoveOwnedAsync<TotpRow>(user, Query<TotpRow>().Where(x => x.UserUuid == user.Uuid && x.Uuid == totp.Uuid), OperationCode.StoreRemoveTotp, cancellationToken);

        /// <inheritdoc />
        public Task<OperationResult> TryRenameTotpAsync(User user, UserTotp totp, string? name, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreRenameTotp;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                TotpRow? row = await Query<TotpRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == totp.Uuid, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                if (row.Name != totp.Name) return Fail(StatusCode.Conflict, ContextCode.None, op);
                _db.Attach(row);
                row.Name = name;
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> TryAdvanceTotpStepAsync(User user, UserTotp totp, ulong step, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreAdvanceTotpStep;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                TotpRow? row = await Query<TotpRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == totp.Uuid, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                if (ToRecord(row).LastStep is { } previous && step <= previous) return Fail(StatusCode.AlreadyUsed, ContextCode.Totp, op);
                _db.Attach(row);
                row.LastStep = Step(step);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<UserRecoveryCode> GetRecoveryCodesAsync(User user, CancellationToken cancellationToken = default) =>
            ReadMany(Query<RecoveryRow>().Where(x => x.UserUuid == user.Uuid), ToRecord, cancellationToken);

        /// <inheritdoc />
        public Task<int> CountRecoveryCodesAsync(User user, CancellationToken cancellationToken = default) =>
            Query<RecoveryRow>().CountAsync(x => x.UserUuid == user.Uuid, cancellationToken);

        /// <inheritdoc />
        public Task<OperationResult> ReplaceRecoveryCodesAsync(User user, IReadOnlyCollection<UserRecoveryCode> codes, CancellationToken cancellationToken = default) {
            ArgumentNullException.ThrowIfNull(codes);
            const OperationCode op = OperationCode.StoreReplaceRecoveryCodes;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                var identifiers = new HashSet<Guid>();
                var hashes = new HashSet<string>(StringComparer.Ordinal);
                var rows = new List<RecoveryRow>();
                foreach (UserRecoveryCode code in codes) {
                    if (code.Uuid == Guid.Empty || code.CodeHash.Length != 32) throw new ArgumentException("Invalid recovery code.", nameof(codes));
                    string key = Convert.ToHexString(code.CodeHash.AsSpan());
                    if (!identifiers.Add(code.Uuid) || await Query<RecoveryRow>().AnyAsync(x => x.UserUuid != user.Uuid && x.Uuid == code.Uuid, cancellationToken).ConfigureAwait(false))
                        return Duplicate(ContextCode.Identifier, op);
                    if (!hashes.Add(key) || await Query<RecoveryRow>().AnyAsync(x => x.UserUuid != user.Uuid && x.HashKey == key, cancellationToken).ConfigureAwait(false))
                        return Duplicate(ContextCode.RecoveryCode, op);
                    rows.Add(new RecoveryRow { Uuid = code.Uuid, UserUuid = user.Uuid, CodeHash = code.CodeHash.AsSpan().ToArray(), HashKey = key });
                }
                await Query<RecoveryRow>().Where(x => x.UserUuid == user.Uuid).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
                _db.AddRange(rows);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> TryConsumeRecoveryCodeAsync(User user, ImmutableArray<byte> codeHash, CancellationToken cancellationToken = default) {
            string key = Convert.ToHexString(codeHash.AsSpan());
            return RemoveOwnedAsync<RecoveryRow>(user, Query<RecoveryRow>().Where(x => x.UserUuid == user.Uuid && x.HashKey == key), OperationCode.StoreConsumeRecoveryCode, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<UserExternalIdentity> GetExternalIdentitiesAsync(User user, CancellationToken cancellationToken = default) =>
            ReadMany(Query<ExternalRow>().Where(x => x.UserUuid == user.Uuid), ToRecord, cancellationToken);

        /// <inheritdoc />
        public async Task<UserExternalIdentity?> GetExternalIdentityAsync(User user, Guid uuid, CancellationToken cancellationToken = default) {
            ExternalRow? row = await Query<ExternalRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == uuid, cancellationToken).ConfigureAwait(false);
            return row is null ? null : ToRecord(row);
        }

        /// <inheritdoc />
        public async Task<User?> FindByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default) {
            ExternalRow? row = await Query<ExternalRow>().SingleOrDefaultAsync(x => x.Issuer == issuer && x.Subject == subject, cancellationToken).ConfigureAwait(false);
            return row is null ? null : await FindByUuidAsync(row.UserUuid, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<OperationResult> AddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreAddExternalIdentity;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                if (await Query<ExternalRow>().AnyAsync(x => x.Uuid == identity.Uuid, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Identifier, op);
                if (await Query<ExternalRow>().AnyAsync(x => x.Issuer == identity.Issuer && x.Subject == identity.Subject, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.ExternalIdentity, op);
                _db.Add(new ExternalRow { Uuid = identity.Uuid, UserUuid = user.Uuid, Name = identity.Name, Issuer = identity.Issuer, Subject = identity.Subject, Email = identity.Email });
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default) {
            if (expected.Uuid != identity.Uuid || expected.Issuer != identity.Issuer || expected.Subject != identity.Subject)
                throw new ArgumentException("External identity keys cannot change.");
            const OperationCode op = OperationCode.StoreUpdateExternalIdentity;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                ExternalRow? row = await Query<ExternalRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == identity.Uuid, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                if (ToRecord(row) != expected) return Fail(StatusCode.Conflict, ContextCode.None, op);
                _db.Attach(row);
                row.Name = identity.Name;
                row.Email = identity.Email;
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> RemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default) =>
            RemoveOwnedAsync<ExternalRow>(user, Query<ExternalRow>().Where(x => x.UserUuid == user.Uuid && x.Uuid == identity.Uuid), OperationCode.StoreRemoveExternalIdentity, cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<UserSession> GetSessionsAsync(User user, CancellationToken cancellationToken = default) =>
            ReadMany(Query<SessionRow>().Where(x => x.UserUuid == user.Uuid), ToRecord, cancellationToken);

        /// <inheritdoc />
        public async Task<UserSession?> GetSessionAsync(User user, Guid uuid, CancellationToken cancellationToken = default) {
            SessionRow? row = await Query<SessionRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == uuid, cancellationToken).ConfigureAwait(false);
            return row is null ? null : ToRecord(row);
        }

        /// <inheritdoc />
        public Task<OperationResult> AddSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default) {
            RequireTransaction();
            if (session.ExpiresAt <= session.AuthenticatedAt) throw new ArgumentException("Invalid session lifetime.");
            const OperationCode op = OperationCode.StoreAddSession;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                if (!await RememberAsync(2, session.Uuid, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Identifier, op);
                _db.Add(new SessionRow { Uuid = session.Uuid, UserUuid = user.Uuid, DeviceName = session.DeviceName, IpAddress = session.IpAddress, UserAgent = session.UserAgent,
                    Amr = JsonSerializer.Serialize(session.Amr.AsSpan().ToArray()), Acr = session.Acr, AuthenticatedAt = Ticks(session.AuthenticatedAt), ExpiresAt = Ticks(session.ExpiresAt) });
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> RemoveSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default) =>
            RemoveOwnedAsync<SessionRow>(user, Query<SessionRow>().Where(x => x.UserUuid == user.Uuid && x.Uuid == session.Uuid), OperationCode.StoreRemoveSession, cancellationToken);

        /// <inheritdoc />
        public Task<OperationResult> RemoveSessionsAsync(User user, Guid? exceptUuid = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreRemoveSessions;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                await Query<SessionRow>().Where(x => x.UserUuid == user.Uuid && (exceptUuid == null || x.Uuid != exceptUuid)).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> RemoveExpiredSessionsAsync(DateTime expiresAt, CancellationToken cancellationToken = default) {
            long cutoff = Ticks(expiresAt);
            return Query<SessionRow>().Where(x => x.ExpiresAt <= cutoff).ExecuteDeleteAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<UserChallenge?> GetChallengeAsync(User user, Guid uuid, CancellationToken cancellationToken = default) {
            ChallengeRow? row = await Query<ChallengeRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == uuid, cancellationToken).ConfigureAwait(false);
            return row is null ? null : ToRecord(row);
        }

        /// <inheritdoc />
        public Task<OperationResult> AddChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default) {
            RequireTransaction();
            if (challenge.Attempts != 0 || challenge.MaxAttempts == 0 || challenge.ExpiresAt <= challenge.CreatedAt) throw new ArgumentException("Invalid challenge.");
            const OperationCode op = OperationCode.StoreAddChallenge;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                if (!await RememberAsync(3, challenge.Uuid, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Identifier, op);
                _db.Add(new ChallengeRow { Uuid = challenge.Uuid, UserUuid = user.Uuid, Type = challenge.Type,
                    SecretHash = challenge.SecretHash is { } hash ? hash.AsSpan().ToArray() : null,
                    Payload = challenge.Payload is { } payload ? payload.AsSpan().ToArray() : null,
                    Attempts = challenge.Attempts, MaxAttempts = challenge.MaxAttempts, CreatedAt = Ticks(challenge.CreatedAt), ExpiresAt = Ticks(challenge.ExpiresAt) });
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> TryReserveChallengeAttemptAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default) {
            StoreTransaction transaction = RequireTransaction();
            if (transaction.Reserved.Contains(challenge.Uuid)) throw new InvalidOperationException("Attempt already reserved.");
            long time = Ticks(now);
            const OperationCode op = OperationCode.StoreReserveChallengeAttempt;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                ChallengeRow? row = await Query<ChallengeRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == challenge.Uuid, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                if (row.ExpiresAt <= time) return Fail(StatusCode.Expired, ContextCode.Challenge, op);
                if (row.Attempts >= row.MaxAttempts) return Fail(StatusCode.AttemptsExhausted, ContextCode.Challenge, op);
                _db.Attach(row);
                row.Attempts++;
                transaction.Reserved.Add(challenge.Uuid);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> TryConsumeChallengeAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default) {
            StoreTransaction transaction = RequireTransaction();
            if (!transaction.Reserved.Contains(challenge.Uuid)) throw new InvalidOperationException("No reserved attempt.");
            long time = Ticks(now);
            const OperationCode op = OperationCode.StoreConsumeChallenge;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                ChallengeRow? row = await Query<ChallengeRow>().SingleOrDefaultAsync(x => x.UserUuid == user.Uuid && x.Uuid == challenge.Uuid, cancellationToken).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                if (row.ExpiresAt <= time) return Fail(StatusCode.Expired, ContextCode.Challenge, op);
                _db.Remove(row);
                transaction.Reserved.Remove(challenge.Uuid);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OperationResult> RemoveChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default) =>
            RemoveOwnedAsync<ChallengeRow>(user, Query<ChallengeRow>().Where(x => x.UserUuid == user.Uuid && x.Uuid == challenge.Uuid), OperationCode.StoreRemoveChallenge, cancellationToken);

        /// <inheritdoc />
        public Task<OperationResult> RemoveChallengesAsync(User user, UserChallengeType? type = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.StoreRemoveChallenges;
            return WriteAsync(op, async () => {
                if (!await ExistsAsync(user, cancellationToken).ConfigureAwait(false)) return MissingUser(op);
                await Query<ChallengeRow>().Where(x => x.UserUuid == user.Uuid && (type == null || x.Type == type)).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
                return Ok(op);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> RemoveExpiredChallengesAsync(DateTime expiresAt, CancellationToken cancellationToken = default) {
            long cutoff = Ticks(expiresAt);
            return Query<ChallengeRow>().Where(x => x.ExpiresAt <= cutoff).ExecuteDeleteAsync(cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<UserAudit> GetAuditsAsync(Guid user, CancellationToken cancellationToken = default) =>
            ReadMany(Query<AuditRow>().Where(x => x.User == user), ToRecord, cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<UserAudit> GetAuditsByActorAsync(Guid actor, CancellationToken cancellationToken = default) =>
            ReadMany(Query<AuditRow>().Where(x => x.Actor == actor), ToRecord, cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<UserAudit> GetAuditsByEntityAsync(Guid entity, CancellationToken cancellationToken = default) =>
            ReadMany(Query<AuditRow>().Where(x => x.Entity == entity), ToRecord, cancellationToken);

        /// <inheritdoc />
        public Task<OperationResult> AddAuditAsync(UserAudit audit, CancellationToken cancellationToken = default) {
            if (Encoding.UTF8.GetByteCount(audit.Info) > 4096) throw new ArgumentException("Audit metadata exceeds 4096 bytes.");
            using JsonDocument document = JsonDocument.Parse(audit.Info);
            if (document.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Audit metadata must be an object.");
            const OperationCode op = OperationCode.StoreAddAudit;
            return WriteAsync(op, async () => {
                if (await Query<AuditRow>().AnyAsync(x => x.Uuid == audit.Uuid, cancellationToken).ConfigureAwait(false)) return Duplicate(ContextCode.Identifier, op);
                _db.Add(new AuditRow { Uuid = audit.Uuid, User = audit.User, Actor = audit.Actor, Entity = audit.Entity, Type = audit.Type,
                    Action = audit.Action, Info = audit.Info, Date = Ticks(audit.Date) });
                return Ok(op);
            }, cancellationToken);
        }

        private StoreTransaction RequireTransaction() =>
            _transaction is { Completed: false } transaction ? transaction : throw new InvalidOperationException("An active identity transaction is required.");

        private Task<OperationResult> RemoveOwnedAsync<T>(User user, IQueryable<T> query, OperationCode op, CancellationToken ct) where T : class =>
            WriteAsync(op, async () => {
                if (!await ExistsAsync(user, ct).ConfigureAwait(false)) return MissingUser(op);
                T? row = await query.SingleOrDefaultAsync(ct).ConfigureAwait(false);
                if (row is null) return MissingEntity(op);
                _db.Remove(row);
                return Ok(op);
            }, ct);

    }

}
