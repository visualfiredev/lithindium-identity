// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lithindium.Identity.EntityFrameworkCore {

    /// <summary>
    /// EF Core identity registration.
    /// </summary>
    public static class IdentityEntityFrameworkExtensions {

        /// <summary>
        /// Registers a dedicated scoped identity context and store.
        /// </summary>
        public static IServiceCollection AddLithindiumEntityFrameworkCore(this IServiceCollection services, Action<DbContextOptionsBuilder> configure) {
            ArgumentNullException.ThrowIfNull(configure);
            services.AddDbContext<IdentityDbContext>(configure);
            return services.AddLithindiumIdentity<EntityFrameworkUserStore>();
        }

    }

}
