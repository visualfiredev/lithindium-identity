// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// A 32-bit operation result: 0xSSCCOODD.
    /// </summary>
    /// <remarks>
    /// Inspired by CatalystUI's FDEFSPEC. Status, context, operation and detail
    /// occupy one byte each, from most to least significant.
    /// </remarks>
    public readonly record struct OperationResult {

        /// <summary>
        /// Gets the packed unsigned value.
        /// </summary>
        public uint Value { get; }

        /// <summary>
        /// Gets the status byte.
        /// </summary>
        public StatusCode Status => (StatusCode)(Value >> 24);

        /// <summary>
        /// Gets the context byte.
        /// </summary>
        public ContextCode Context => (ContextCode)((Value >> 16) & 0xFFu);

        /// <summary>
        /// Gets the operation byte.
        /// </summary>
        public OperationCode Operation => (OperationCode)((Value >> 8) & 0xFFu);

        /// <summary>
        /// Gets the raw detail byte.
        /// </summary>
        public byte Detail => (byte)(Value & 0xFFu);

        /// <summary>
        /// Gets the status level.
        /// </summary>
        public StatusCodeLevel Level => (StatusCodeLevel)(Value >> 30);

        /// <summary>
        /// Gets whether the result has success level.
        /// </summary>
        public bool IsSuccess => Level == StatusCodeLevel.Success;

        /// <summary>
        /// Gets whether the result has warning level.
        /// </summary>
        public bool IsWarning => Level == StatusCodeLevel.Warning;

        /// <summary>
        /// Gets whether the result has error level.
        /// </summary>
        public bool IsError => Level == StatusCodeLevel.Error;

        /// <summary>
        /// Gets whether the result has fatal level.
        /// </summary>
        public bool IsFatal => Level == StatusCodeLevel.Fatal;

        /// <summary>
        /// Gets whether the operation completed.
        /// </summary>
        public bool IsCompleted => IsSuccess || IsWarning;

        /// <summary>
        /// Gets whether the operation failed.
        /// </summary>
        public bool IsFailure => IsError || IsFatal;

        /// <summary>
        /// Creates a result from a packed value.
        /// </summary>
        /// <exception cref="ArgumentException">Success-level context is nonzero.</exception>
        public OperationResult(uint value) {
            if ((value >> 30) == 0 && (value & 0x00FF0000u) != 0) {
                throw new ArgumentException("Success-level context must be zero.", nameof(value));
            }

            Value = value;
        }

        /// <summary>
        /// Creates a result from its four bytes.
        /// </summary>
        public OperationResult(StatusCode status, ContextCode context = ContextCode.None, OperationCode operation = OperationCode.Unknown, byte detail = 0)
            : this(((uint)status << 24) | ((uint)context << 16) | ((uint)operation << 8) | detail) {
            // ...
        }

        /// <summary>
        /// Creates a result with a named detail code.
        /// </summary>
        public OperationResult(StatusCode status, ContextCode context, OperationCode operation, DetailCode detail)
            : this(status, context, operation, (byte)detail) {
            // ...
        }

        /// <summary>
        /// Returns the packed hexadecimal value.
        /// </summary>
        public override string ToString() => $"0x{Value:X8}";

    }

}
