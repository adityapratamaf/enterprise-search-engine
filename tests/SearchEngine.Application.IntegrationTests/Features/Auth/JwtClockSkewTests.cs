using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SearchEngine.Application.IntegrationTests.Features.Auth;

/// <summary>
/// A-015: the configured JwtBearer ClockSkew is tightened from the framework
/// default (5 minutes) to 30 seconds. Reading the actual resolved options is
/// deterministic and avoids brittle wall-clock timing tests.
/// </summary>
public class JwtClockSkewTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public JwtClockSkewTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void ClockSkew_Should_Be_30_Seconds()
    {
        using var scope =
            _factory.Services.CreateScope();

        var options = scope.ServiceProvider
            .GetRequiredService<
                IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        options.TokenValidationParameters.ClockSkew
            .Should()
            .Be(TimeSpan.FromSeconds(30));
    }
}
