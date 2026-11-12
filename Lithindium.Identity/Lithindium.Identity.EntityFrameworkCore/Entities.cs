// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;

namespace Lithindium.Identity.EntityFrameworkCore {

    internal sealed class UserRow {
        public Guid Uuid { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UsernameKey { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public int Rotation { get; set; }
        public UserStatus Status { get; set; }
    }

    internal sealed class EmailRow {
        public string AddressKey { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Guid UserUuid { get; set; }
        public Guid? PrimaryOwner { get; set; }
    }

    internal sealed class PhoneRow {
        public string Number { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public Guid UserUuid { get; set; }
        public Guid? PrimaryOwner { get; set; }
    }

    internal sealed class LockoutRow {
        public Guid UserUuid { get; set; }
        public bool Enabled { get; set; }
        public long Failures { get; set; }
        public long? ReleaseAt { get; set; }
    }

    internal sealed class TotpRow {
        public Guid Uuid { get; set; }
        public Guid UserUuid { get; set; }
        public string? Name { get; set; }
        public byte[] Secret { get; set; } = [];
        public UserTotpAlgorithm Algorithm { get; set; }
        public byte Digits { get; set; }
        public byte Period { get; set; }
        public string? LastStep { get; set; }
    }

    internal sealed class RecoveryRow {
        public Guid Uuid { get; set; }
        public Guid UserUuid { get; set; }
        public byte[] CodeHash { get; set; } = [];
        public string HashKey { get; set; } = string.Empty;
    }

    internal sealed class ExternalRow {
        public Guid Uuid { get; set; }
        public Guid UserUuid { get; set; }
        public string? Name { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    internal sealed class SessionRow {
        public Guid Uuid { get; set; }
        public Guid UserUuid { get; set; }
        public string? DeviceName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string Amr { get; set; } = string.Empty;
        public string? Acr { get; set; }
        public long AuthenticatedAt { get; set; }
        public long ExpiresAt { get; set; }
    }

    internal sealed class ChallengeRow {
        public Guid Uuid { get; set; }
        public Guid UserUuid { get; set; }
        public UserChallengeType Type { get; set; }
        public byte[]? SecretHash { get; set; }
        public byte[]? Payload { get; set; }
        public long Attempts { get; set; }
        public long MaxAttempts { get; set; }
        public long CreatedAt { get; set; }
        public long ExpiresAt { get; set; }
    }

    internal sealed class AuditRow {
        public Guid Uuid { get; set; }
        public Guid User { get; set; }
        public Guid? Actor { get; set; }
        public Guid? Entity { get; set; }
        public UserAuditType Type { get; set; }
        public UserAuditAction Action { get; set; }
        public string Info { get; set; } = string.Empty;
        public long Date { get; set; }
    }

    internal sealed class IdentifierRow {
        public int Kind { get; set; }
        public Guid Uuid { get; set; }
    }

}
