using System.Net;
using System.Net.Http.Headers;
using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;

namespace SearchEngine.Application.IntegrationTests.Features.Products;

/// <summary>
/// A-007: a genuinely missing resource returns 404 Not Found (not 200 with
/// success=false).
/// </summary>
public class NotFoundApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NotFoundApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProductById_Should_Return_404_When_Missing()
    {
        var auth =
            await AuthTestHelper.LoginAsync(_client);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Access);

        var response =
            await _client.GetAsync(
                $"/api/products/{Guid.NewGuid()}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }
}
