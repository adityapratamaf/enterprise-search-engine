using FluentAssertions;
using System.Net;
using SearchEngine.Application.IntegrationTests.Common;

namespace SearchEngine.Application.IntegrationTests.Features.Products;

/// <summary>
/// Integration tests untuk memverifikasi
/// authorization pada endpoint Products.
/// </summary>
public class GetProductsUnauthorizedTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetProductsUnauthorizedTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Memastikan endpoint GET /api/products
    /// mengembalikan HTTP 401 Unauthorized
    /// ketika request tidak menyertakan JWT access token.
    /// </summary>
    [Fact]
    public async Task GetProducts_Should_Return_401_When_Unauthenticated()
    {
        // Act

        var response =
            await _client.GetAsync(
                "/api/products");

        // Assert

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}
