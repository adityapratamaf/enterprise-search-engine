using SearchEngine.Infrastructure.Identity.Context;
using SearchEngine.Infrastructure.Identity.Entities;
using SearchEngine.Infrastructure.Identity.Seed;
using SearchEngine.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.WebAPI.Extensions;

public static class DatabaseExtensions
{
    public static async Task<WebApplication>
        InitializeDatabasesAsync(
            this WebApplication app)
    {
        using var scope =
            app.Services.CreateScope();

        var services =
            scope.ServiceProvider;

        var configuration =
            services.GetRequiredService<IConfiguration>();

        var logger =
            services.GetRequiredService<
                ILoggerFactory>()
            .CreateLogger("Database");

        try
        {
            //--------------------------------------------------
            // Business Database
            //--------------------------------------------------

            var businessDb =
                services.GetRequiredService<
                    ApplicationBusinessDbContext>();

            await businessDb.Database.MigrateAsync();

            //--------------------------------------------------
            // Identity Database
            //--------------------------------------------------

            var identityDb =
                services.GetRequiredService<
                    ApplicationIdentityDbContext>();

            await identityDb.Database.MigrateAsync();

            //--------------------------------------------------
            // Hangfire Database
            //--------------------------------------------------

            await EnsureHangfireDatabaseAsync(
                configuration);

            //--------------------------------------------------
            // Seeder
            //--------------------------------------------------

            var roleManager =
                services.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<
                    UserManager<ApplicationUser>>();

            await DefaultRolesSeeder.SeedAsync(
                roleManager,
                userManager);

            await ModuleSeeder.SeedAsync(
                identityDb);

            await PermissionActionSeeder.SeedAsync(
                identityDb);

            await PermissionCatalogSeeder.SeedAsync(
                identityDb);

            await RolePermissionSeeder.SeedAsync(
                identityDb,
                roleManager);

            logger.LogInformation(
                "Database initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Database initialization failed.");

            throw;
        }

        return app;
    }

    //------------------------------------------------------
    // Hangfire DB
    //------------------------------------------------------

    private static async Task
        EnsureHangfireDatabaseAsync(
            IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "HangfireConnection")
            ?? throw new Exception(
                "HangfireConnection not found.");

        var builder =
            new SqlConnectionStringBuilder(
                connectionString);

        var database =
            builder.InitialCatalog;

        builder.InitialCatalog =
            "master";

        // Nama database berasal dari konfigurasi (tepercaya) dan tidak bisa
        // dijadikan parameter pada perintah DDL. Escape identifier & literal
        // sebagai defense-in-depth agar aman dari SQL injection.
        var safeLiteral =
            database.Replace("'", "''");

        var safeIdentifier =
            database.Replace("]", "]]");

        await using var connection =
            new SqlConnection(
                builder.ConnectionString);

        await connection.OpenAsync();

        var command =
            connection.CreateCommand();

        command.CommandText =
        $"""
        IF DB_ID(N'{safeLiteral}') IS NULL
        BEGIN
            CREATE DATABASE [{safeIdentifier}]
        END
        """;

        await command.ExecuteNonQueryAsync();
    }
}
