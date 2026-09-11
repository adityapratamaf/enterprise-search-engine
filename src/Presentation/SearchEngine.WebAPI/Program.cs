using Asp.Versioning;
using SearchEngine.Application;
using SearchEngine.Infrastructure.Identity;
using SearchEngine.Infrastructure.Shared;
using SearchEngine.Infrastructure.Persistence;
using SearchEngine.Infrastructure.Search;
using SearchEngine.WebAPI.Middleware;
using SearchEngine.WebAPI.Authorization;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.WebAPI.Extensions;
using SearchEngine.WebAPI.OpenApi;
using SearchEngine.WebAPI.Logging;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);


// Logging Serilog
builder.AddSearchEngineLogging();


// Refresh Token
builder.Services.AddHttpContextAccessor();


// Controllers
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory =
            context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value!.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray());

                var response =
                    Result<object>.Failure(
                        "Validation failed",
                        errors);

                return new BadRequestObjectResult(
                    response);
            };
    });


// Global exception handling (framework-native IExceptionHandler pipeline)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// OpenAPI + Scalar
builder.Services.AddEndpointsApiExplorer();


// builder.Services.AddOpenApi();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

    options.AddOperationTransformer<AuthorizeOperationTransformer>();
});


// Application Layer
builder.Services.AddApplication();


// Identity Layer
builder.Services.AddIdentityInfrastructure(
    builder.Configuration);


// Persistence Layer
builder.Services.AddPersistence(
    builder.Configuration);


// File Attachment
builder.Services.AddSharedInfrastructure(
    builder.Configuration);


// Elasticsearch
builder.Services.AddSearchInfrastructure(
    builder.Configuration);


// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion =
        new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified =
        true;

    options.ReportApiVersions = true;
});


// Output Cache
builder.Services.AddOutputCache();


// Otorisasi imperatif berbasis resource (file attachment per module pemilik)
builder.Services.AddScoped<
    IPermissionAuthorizationService,
    PermissionAuthorizationService>();


// Hangfire
builder.Services
    .AddSearchEngineHangfire(
        builder.Configuration);


// OpenTelemetry
builder.Services
    .AddSearchEngineOpenTelemetry(
        builder.Configuration);


// CORS
builder.Services
    .AddSearchEngineCors(
        builder.Configuration);


// Forwarded Headers (real client IP behind a trusted reverse proxy / LB)
builder.Services
    .AddSearchEngineForwardedHeaders(
        builder.Configuration);


// HSTS (Strict-Transport-Security) options — applied only outside Development.
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    // Preload intentionally left off: only enable after committing to HTTPS-only
    // and submitting the domain to the HSTS preload list.
});


// Rate Limiter
builder.Services
    .AddSearchEngineRateLimiter(
        builder.Configuration);


// Health Checks
builder.Services
    .AddSearchEngineHealthChecks(
        builder.Configuration);


var app = builder.Build();


// Database Migration & Seeding.
// - Development: runs automatically for local convenience.
// - Other environments: opt-in via "Database:MigrateOnStartup" = true, or by
//   running the app once with the "--migrate" argument as a dedicated
//   deployment/init job. This prevents multiple production replicas from
//   racing on MigrateAsync() during a rolling/blue-green deployment.
var runDatabaseInit =
    args.Contains("--migrate")
    || args.Contains("--seed-demo")
    || app.Environment.IsDevelopment()
    || app.Configuration.GetValue<bool>(
        "Database:MigrateOnStartup");

if (runDatabaseInit)
{
    await app.InitializeDatabasesAsync();
}

if (args.Contains("--migrate"))
{
    // Migration/seed job: apply and exit without starting the web host.
    return;
}

if (args.Contains("--seed-demo"))
{
    // Membangkitkan data SPBU tiruan lalu keluar tanpa menyalakan web host.
    // Jumlahnya dapat diatur: --seed-demo --count 25000
    var countIndex =
        Array.IndexOf(args, "--count");

    var demoCount =
        countIndex >= 0
        && countIndex + 1 < args.Length
        && int.TryParse(args[countIndex + 1], out var parsed)
            ? parsed
            : 10_000;

    await app.SeedDemoDataAsync(demoCount);

    return;
}


// Forwarded Headers — must run before anything that reads the client IP
// (request logging, rate limiter, auth) so they see the real client address.
app.UseSearchEngineForwardedHeaders();


// Security Headers
app.UseSearchEngineSecurityHeaders();


// Global Error Handler (IExceptionHandler pipeline)
app.UseExceptionHandler();


// Correlation ID
app.UseMiddleware<
    CorrelationIdMiddleware>();


// Request Logging
app.UseSerilogRequestLogging();


// CORS
app.UseSearchEngineCors();


// HSTS — production only (avoid locking browsers to HTTPS during local HTTP dev)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}


// HTTPS
app.UseHttpsRedirection();


// File
app.UseStaticFiles();


// Rate Limiter
app.UseSearchEngineRateLimiter();


// JWT Authentication
app.UseAuthentication();


// Authorization
app.UseAuthorization();


// Basic Auth For Dashboard Operational (Hangfire & Health Checks UI)
app.UseMiddleware<
    DashboardBasicAuthMiddleware>();


// Health Checks
app.UseSearchEngineHealthChecks();


// Output Cache
app.UseOutputCache();


// Hangfire Dashboard
app.UseSearchEngineHangfire();


// OpenAPI Endpoint
app.MapOpenApi();


// Scalar Documentation
app.MapScalarApiReference(
    "/scalar/docs",
    options =>
    {
        options.WithTitle(
            "SearchEngine API");

        options.WithTheme(
            ScalarTheme.BluePlanet);

        options.WithDefaultHttpClient(
            ScalarTarget.CSharp,
            ScalarClient.HttpClient);

        // JWT Authorization
        options.AddPreferredSecuritySchemes(
            "Bearer");
    });


// Controllers
app.MapControllers();


// Run App
app.Run();


public partial class Program
{
}
