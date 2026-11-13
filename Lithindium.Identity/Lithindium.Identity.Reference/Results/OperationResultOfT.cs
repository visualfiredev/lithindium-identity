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
    /// An operation result with a value on completion.
    /// </summary>
    public sealed class OperationResult<T> {

        private readonly T? _value;

        /// <summary>
        /// Gets the packed result.
        /// </summary>
        public OperationResult Result { get; }

        /// <summary>
        /// Gets whether a value is available.
        /// </summary>
        public bool HasValue => Result.IsCompleted;

        /// <summary>
        /// Gets the returned value.
        /// </summary>
        /// <exception cref="InvalidOperationException">The operation failed.</exception>
        public T Value => HasValue ? _value! : throw new InvalidOperationException("The operation failed.");

        /// <summary>
        /// Creates a completed result with a value.
        /// </summary>
        public OperationResult(OperationResult result, T value) {
            if (!result.IsCompleted) {
                throw new ArgumentException("A value requires a completed result.", nameof(result));
            }

            Result = result;
            _value = value;
        }

        /// <summary>
        /// Creates a failed result without a value.
        /// </summary>
        public OperationResult(OperationResult result) {
            if (!result.IsFailure) {
                throw new ArgumentException("A valueless result requires a failure.", nameof(result));
            }

            Result = result;
        }

    }

}
