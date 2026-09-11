using System.Net.Http.Headers;
using System.Net.Http.Json;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;

namespace SearchEngine.Application.IntegrationTests.Features.Products;

/// <summary>
/// A-018: offset pagination must be deterministic. These tests seed products
/// that share the same Name (the worst case for a non-unique sort column) and
/// verify the stable Id tie-breaker prevents rows from being skipped or
/// duplicated across page boundaries.
/// </summary>
public class ProductPaginationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductPaginationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthedClientAsync()
    {
        var client = _factory.CreateClient();

        var auth = await AuthTestHelper.LoginAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Access);

        return client;
    }

    private static async Task<List<Guid>> SeedDuplicateNameProductsAsync(
        HttpClient client,
        string marker,
        int count)
    {
        var ids = new List<Guid>();

        for (var i = 0; i < count; i++)
        {
            var response = await client.PostAsJsonAsync(
                "/api/products",
                new
                {
                    name = marker,
                    code = $"{marker}_{i}",
                    price = 10m,
                    stock = 1
                });

            response.EnsureSuccessStatusCode();

            var created = await response.Content
                .ReadFromJsonAsync<ApiResponse<ReadProductResponse>>();

            ids.Add(created!.Data!.Id);
        }

        return ids;
    }

    private static async Task<PaginatedResult<ReadProductResponse>> GetPageAsync(
        HttpClient client,
        string search,
        int pageNumber,
        int pageSize)
    {
        var response = await client.GetAsync(
            $"/api/products?search={search}" +
            $"&pageNumber={pageNumber}&pageSize={pageSize}&sortBy=name");

        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<PaginatedResult<ReadProductResponse>>>();

        return body!.Data!;
    }

    [Fact]
    public async Task Paging_Duplicate_Names_Should_Not_Skip_Or_Duplicate()
    {
        var client = await CreateAuthedClientAsync();
        var marker = "PGN" + Guid.NewGuid().ToString("N");

        var created = await SeedDuplicateNameProductsAsync(
            client,
            marker,
            count: 5);

        // Walk every page with a small page size so boundaries are exercised.
        var seen = new List<Guid>();
        for (var page = 1; page <= 3; page++)
        {
            var result = await GetPageAsync(
                client,
                marker,
                pageNumber: page,
                pageSize: 2);

            seen.AddRange(result.Items.Select(x => x.Id));
        }

        seen.Should().HaveCount(5);
        seen.Should().OnlyHaveUniqueItems();          // no duplicates
        seen.Should().BeEquivalentTo(created);        // no skips
    }

    [Fact]
    public async Task Same_Request_Should_Return_Deterministic_Order()
    {
        var client = await CreateAuthedClientAsync();
        var marker = "PGN" + Guid.NewGuid().ToString("N");

        await SeedDuplicateNameProductsAsync(client, marker, count: 4);

        var first = await GetPageAsync(client, marker, 1, 10);
        var second = await GetPageAsync(client, marker, 1, 10);

        first.Items.Select(x => x.Id)
            .Should()
            .Equal(second.Items.Select(x => x.Id));
    }

    [Fact]
    public async Task Search_For_Unknown_Term_Should_Return_Empty()
    {
        var client = await CreateAuthedClientAsync();
        var unknown = "NOEXIST" + Guid.NewGuid().ToString("N");

        var result = await GetPageAsync(client, unknown, 1, 10);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
