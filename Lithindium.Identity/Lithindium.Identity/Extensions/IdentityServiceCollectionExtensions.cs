// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Identity service registration.
    /// </summary>
    public static class IdentityServiceCollectionExtensions {

        /// <summary>
        /// Registers scoped managers and default policies. Register a scoped user store
        /// and a shared data protection provider separately.
        /// </summary>
        public static IServiceCollection AddLithindiumIdentity(this IServiceCollection services,
            UserManagerOptions? users = null, ChallengeManagerOptions? challenges = null, SignInManagerOptions? signIn = null) {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton(users ?? new UserManagerOptions());
            services.TryAddSingleton(challenges ?? new ChallengeManagerOptions());
            services.TryAddSingleton(signIn ?? new SignInManagerOptions());
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddScoped<IPasswordHasher<object>, PasswordHasher<object>>();
            services.TryAddScoped<UserManager>();
            services.TryAddScoped<ChallengeManager>();
            services.TryAddScoped<SignInManager>();
            return services;
        }

        /// <summary>
        /// Registers identity with a single scoped store implementation.
        /// </summary>
        public static IServiceCollection AddLithindiumIdentity<TStore>(this IServiceCollection services,
            UserManagerOptions? users = null, ChallengeManagerOptions? challenges = null, SignInManagerOptions? signIn = null)
            where TStore : class, IUserStore {
            services.AddLithindiumIdentity(users, challenges, signIn);
            services.TryAddScoped<TStore>();
            services.TryAddScoped<IUserStore>(provider => provider.GetRequiredService<TStore>());
            return services;
        }

    }

}
