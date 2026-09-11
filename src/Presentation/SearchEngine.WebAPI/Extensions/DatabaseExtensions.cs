using SearchEngine.Infrastructure.Identity.Context;
using SearchEngine.Infrastructure.Identity.Entities;
using SearchEngine.Infrastructure.Identity.Seed;
using SearchEngine.Infrastructure.Persistence.Context;
using SearchEngine.Infrastructure.Persistence.Seed;
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

            //--------------------------------------------------
            // Reference data domain SPBU
            //
            // Aplikasi tidak dapat berfungsi tanpa data ini, sehingga
            // ditanam di setiap startup. Seluruhnya idempoten: hanya baris
            // yang belum ada yang ditambahkan. Data SPBU-nya sendiri TIDAK
            // termasuk di sini — lihat argumen "--seed-demo".
            //--------------------------------------------------

            await RegionalSeeder.SeedAsync(businessDb);

            await WilayahSeeder.SeedAsync(businessDb);

            await ProdukBbmSeeder.SeedAsync(businessDb);

            await FasilitasSeeder.SeedAsync(businessDb);

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
    // Data demo SPBU
    //
    // Dipanggil eksplisit lewat argumen "--seed-demo", tidak pernah ikut
    // startup biasa: puluhan ribu baris tiruan tidak boleh masuk database
    // hanya karena aplikasi dijalankan.
    //------------------------------------------------------

    public static async Task<WebApplication>
        SeedDemoDataAsync(
            this WebApplication app,
            int target = 10_000)
    {
        using var scope =
            app.Services.CreateScope();

        var logger =
            scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("DemoSeed");

        var businessDb =
            scope.ServiceProvider
                .GetRequiredService<ApplicationBusinessDbContext>();

        var mulai =
            DateTimeOffset.UtcNow;

        var summary =
            await SpbuDemoSeeder.SeedAsync(
                businessDb,
                target);

        if (summary.Dilewati > 0)
        {
            logger.LogInformation(
                "Data SPBU sudah ada; seeding demo dilewati.");

            return app;
        }

        logger.LogInformation(
            "Seeding demo selesai dalam {Durasi:0.0}s: "
            + "{Spbu} SPBU, {Produk} relasi produk, {Fasilitas} relasi fasilitas.",
            (DateTimeOffset.UtcNow - mulai).TotalSeconds,
            summary.Spbu,
            summary.Produk,
            summary.Fasilitas);

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
