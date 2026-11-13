// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Operation status codes.
    /// </summary>
    /// <remarks>
    /// Unassigned values are reserved; their level remains defined by the upper two bits.
    /// </remarks>
    public enum StatusCode : byte {

        /// <summary>
        /// Operation succeeded.
        /// </summary>
        Success = 0x00,

        /// <summary>
        /// Operation succeeded without changes.
        /// </summary>
        NoOp = 0x01,

        /// <summary>
        /// Completed with an unexpected or undesirable result.
        /// </summary>
        Warning = 0x40,

        /// <summary>
        /// Partially completed; output may be incomplete.
        /// </summary>
        Partial = 0x41,

        /// <summary>
        /// Completed using deprecated behavior.
        /// </summary>
        Deprecated = 0x42,

        /// <summary>
        /// Recoverable failure.
        /// </summary>
        Error = 0x80,

        /// <summary>
        /// Invalid or out-of-range argument. Detail is its zero-based index.
        /// </summary>
        InvalidArgument = 0x81,

        /// <summary>
        /// Inconsistent, corrupted or invalid state.
        /// </summary>
        InvalidState = 0x82,

        /// <summary>
        /// Malformed or invalid input.
        /// </summary>
        MalformedInput = 0x83,

        /// <summary>
        /// Access was denied.
        /// </summary>
        AccessDenied = 0x84,

        /// <summary>
        /// Requested feature is not implemented.
        /// </summary>
        NotImplemented = 0x85,

        /// <summary>
        /// Unsupported system or configuration.
        /// </summary>
        NotSupported = 0x86,

        /// <summary>
        /// Required timeout elapsed.
        /// </summary>
        Timeout = 0x87,

        /// <summary>
        /// Required resource does not exist.
        /// </summary>
        NotFound = 0x88,

        /// <summary>
        /// External event interrupted the operation.
        /// </summary>
        Interrupted = 0x89,

        /// <summary>
        /// Dependency failed. Detail is its status code.
        /// </summary>
        DependencyFailure = 0x8A,

        /// <summary>
        /// Unique value or resource already exists.
        /// </summary>
        AlreadyExists = 0x8B,

        /// <summary>
        /// Concurrent change or serialization conflict.
        /// </summary>
        Conflict = 0x8C,

        /// <summary>
        /// Resource or credential expired.
        /// </summary>
        Expired = 0x8D,

        /// <summary>
        /// Single-use proof or time step already used.
        /// </summary>
        AlreadyUsed = 0x8E,

        /// <summary>
        /// Attempt limit exhausted.
        /// </summary>
        AttemptsExhausted = 0x8F,

        /// <summary>
        /// Credential verification failed.
        /// </summary>
        InvalidCredential = 0x92,

        /// <summary>
        /// Identity policy not satisfied.
        /// </summary>
        PolicyViolation = 0x93,

        /// <summary>
        /// Unrecoverable failure.
        /// </summary>
        Fatal = 0xC0,

        /// <summary>
        /// Fundamental invariant violated; logic error or corruption.
        /// </summary>
        InvariantViolation = 0xC1

    }

}
