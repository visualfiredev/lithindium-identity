// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lithindium.Identity.EntityFrameworkCore {

    /// <summary>
    /// Design-time context for generating provider-specific migrations.
    /// </summary>
    public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext> {

        /// <inheritdoc />
        public IdentityDbContext CreateDbContext(string[] args) {
            var options = new DbContextOptionsBuilder<IdentityDbContext>();
            string provider = Environment.GetEnvironmentVariable("LITHINDIUM_DB_PROVIDER") ?? "sqlite";
            string connection = Environment.GetEnvironmentVariable("LITHINDIUM_DB_CONNECTION") ??
                (provider == "sqlite" ? "Data Source=lithindium.db;Foreign Keys=True" : throw new InvalidOperationException("Set LITHINDIUM_DB_CONNECTION."));
            if (provider == "sqlite") options.UseSqlite(connection);
            else if (provider == "postgres") options.UseNpgsql(connection);
            else throw new InvalidOperationException("Use sqlite or postgres.");
            return new IdentityDbContext(options.Options);
        }

    }

}
