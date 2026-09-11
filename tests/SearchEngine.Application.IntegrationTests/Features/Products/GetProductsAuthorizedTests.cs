using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using SearchEngine.Application.IntegrationTests.Common;

namespace SearchEngine.Application.IntegrationTests.Features.Products;

/// <summary>
/// Integration tests untuk endpoint Products
/// yang membutuhkan JWT Authentication.
/// </summary>
public class GetProductsAuthorizedTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetProductsAuthorizedTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Memastikan endpoint GET /api/products
    /// mengembalikan HTTP 200 OK
    /// ketika request menggunakan access token yang valid.
    /// </summary>
    [Fact]
    public async Task GetProducts_Should_Return_200_When_Authorized()
    {
        // Arrange
        var auth =
            await AuthTestHelper
                .LoginAsync(_client);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                auth.Access);

        // Act
        var response =
            await _client.GetAsync(
                "/api/products");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Memastikan endpoint GET /api/products
    /// mengembalikan data response yang tidak kosong
    /// ketika user berhasil terautentikasi.
    /// </summary>
    [Fact]
    public async Task GetProducts_Should_Return_Data_When_Authorized()
    {
        // Arrange

        var auth =
            await AuthTestHelper
                .LoginAsync(_client);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                auth.Access);

        // Act
        var response =
            await _client.GetAsync(
                "/api/products");

        var content =
            await response.Content
                .ReadAsStringAsync();

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        content.Should()
            .NotBeNullOrWhiteSpace();
    }
}
