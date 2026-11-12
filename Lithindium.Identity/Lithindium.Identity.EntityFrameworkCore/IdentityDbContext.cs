// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Lithindium.Identity.EntityFrameworkCore {

    /// <summary>
    /// Relational identity schema.
    /// </summary>
    public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options) {

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            string binary = Database.IsNpgsql() ? "C" : "BINARY";
            modelBuilder.Entity<UserRow>(e => {
                e.ToTable("identity_user", table => {
                    table.HasCheckConstraint("ck_identity_rotation", "\"Rotation\" BETWEEN 0 AND 65535");
                    table.HasCheckConstraint("ck_identity_status", "\"Status\" IN (0, 1)");
                });
                e.HasKey(x => x.Uuid);
                e.Property(x => x.Username).HasMaxLength(21);
                e.Property(x => x.UsernameKey).HasMaxLength(128).UseCollation(binary);
                e.Property(x => x.Password).HasMaxLength(255);
                e.Property(x => x.DisplayName).HasMaxLength(32);
                e.HasIndex(x => x.UsernameKey).IsUnique().HasDatabaseName("ux_identity_username");
            });

            modelBuilder.Entity<EmailRow>(e => {
                e.ToTable("identity_email", table => {
                    table.HasCheckConstraint("ck_identity_email_primary", "\"PrimaryOwner\" IS NULL OR \"PrimaryOwner\" = \"UserUuid\"");
                });
                e.HasKey(x => x.AddressKey);
                e.Property(x => x.Address).HasMaxLength(254);
                e.Property(x => x.AddressKey).HasMaxLength(1024).UseCollation(binary);
                e.HasIndex(x => x.PrimaryOwner).IsUnique();
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<PhoneRow>(e => {
                e.ToTable("identity_phone", table => {
                    table.HasCheckConstraint("ck_identity_phone_primary", "\"PrimaryOwner\" IS NULL OR \"PrimaryOwner\" = \"UserUuid\"");
                });
                e.HasKey(x => new { x.Number, x.Extension });
                e.Property(x => x.Number).HasMaxLength(16).UseCollation(binary);
                e.Property(x => x.Extension).HasMaxLength(10).UseCollation(binary);
                e.HasIndex(x => x.PrimaryOwner).IsUnique();
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<LockoutRow>(e => {
                e.ToTable("identity_lockout", table => {
                    table.HasCheckConstraint("ck_identity_failures", "\"Failures\" BETWEEN 0 AND 4294967295");
                });
                e.HasKey(x => x.UserUuid);
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<TotpRow>(e => {
                e.ToTable("identity_totp");
                e.HasKey(x => x.Uuid);
                e.Property(x => x.Name).HasMaxLength(32);
                e.Property(x => x.Secret).HasMaxLength(255);
                e.Property(x => x.LastStep).HasMaxLength(20);
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<RecoveryRow>(e => {
                e.ToTable("identity_recovery");
                e.HasKey(x => x.Uuid);
                e.Property(x => x.CodeHash).HasMaxLength(32);
                e.Property(x => x.HashKey).HasMaxLength(64).UseCollation(binary);
                e.HasIndex(x => x.HashKey).IsUnique().HasDatabaseName("ux_identity_recovery_hash");
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<ExternalRow>(e => {
                e.ToTable("identity_external");
                e.HasKey(x => x.Uuid);
                e.Property(x => x.Name).HasMaxLength(255);
                e.Property(x => x.Issuer).HasMaxLength(512).UseCollation(binary);
                e.Property(x => x.Subject).HasMaxLength(255).UseCollation(binary);
                e.Property(x => x.Email).HasMaxLength(254);
                e.HasIndex(x => new { x.Issuer, x.Subject }).IsUnique().HasDatabaseName("ux_identity_external_pair");
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<SessionRow>(e => {
                e.ToTable("identity_session", table => {
                    table.HasCheckConstraint("ck_identity_session_lifetime", "\"ExpiresAt\" > \"AuthenticatedAt\"");
                });
                e.HasKey(x => x.Uuid);
                e.Property(x => x.DeviceName).HasMaxLength(64);
                e.Property(x => x.IpAddress).HasMaxLength(64);
                e.Property(x => x.UserAgent).HasMaxLength(2048);
                e.Property(x => x.Acr).HasMaxLength(255);
                e.HasIndex(x => x.ExpiresAt);
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<ChallengeRow>(e => {
                e.ToTable("identity_challenge", table => {
                    table.HasCheckConstraint("ck_identity_challenge_lifetime", "\"ExpiresAt\" > \"CreatedAt\"");
                    table.HasCheckConstraint("ck_identity_challenge_attempts", "\"MaxAttempts\" BETWEEN 1 AND 4294967295 AND \"Attempts\" BETWEEN 0 AND \"MaxAttempts\"");
                });
                e.HasKey(x => x.Uuid);
                e.Property(x => x.SecretHash).HasMaxLength(32);
                e.HasIndex(x => x.ExpiresAt);
                e.HasIndex(x => new { x.UserUuid, x.Type });
                e.HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserUuid).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.UserUuid);
            });

            modelBuilder.Entity<AuditRow>(e => {
                e.ToTable("identity_audit");
                e.HasKey(x => x.Uuid);
                e.HasIndex(x => x.User);
                e.HasIndex(x => x.Actor);
                e.HasIndex(x => x.Entity);
                e.HasIndex(x => x.Date);
            });

            modelBuilder.Entity<IdentifierRow>(e => {
                e.ToTable("identity_identifier");
                e.HasKey(x => new { x.Kind, x.Uuid });
            });
        }

    }

}
