using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SearchEngine.Application.IntegrationTests.Common;
using SearchEngine.Infrastructure.Identity.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Application.IntegrationTests.Features.PermissionActions;

/// <summary>
/// Integration tests untuk PermissionAction Management (CRUD).
/// Seluruh skenario dijalankan dalam satu flow (satu login) untuk menghormati
/// rate limit login, dan membersihkan data uji lewat DbContext di akhir.
/// </summary>
public class PermissionActionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    private readonly HttpClient _client;

    private readonly string _suffix =
        Guid.NewGuid().ToString("N")[..8];

    public PermissionActionTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PermissionAction_Crud_Flow_Works()
    {
        var auth = await AuthTestHelper.LoginAsync(_client);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Access);

        Guid? actionAId = null;
        Guid? actionBId = null;

        var codeA = $"test_a_{_suffix}";
        var nameA = $"Test Action A {_suffix}";
        var codeB = $"test_b_{_suffix}";
        var nameB = $"Test Action B {_suffix}";

        try
        {
            // ===== CREATE (active -> becomes part of catalog) =====
            var createA = await PostAsync(
                "/api/permission-actions",
                new { code = codeA, name = nameA, description = "A", displayOrder = 100, isActive = true });

            createA.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            createA.Body!.Success.Should().BeTrue();
            createA.Body.Data!.Code.Should().Be(codeA);
            actionAId = createA.Body.Data.Id;

            // ===== CREATE (inactive -> NOT part of catalog, deletable) =====
            var createB = await PostAsync(
                "/api/permission-actions",
                new { code = codeB, name = nameB, description = "B", displayOrder = 101, isActive = false });

            createB.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            createB.Body!.Success.Should().BeTrue();
            actionBId = createB.Body.Data!.Id;

            // ===== DUPLICATE CODE -> validation 400 =====
            var dupCode = await PostAsync(
                "/api/permission-actions",
                new { code = codeA, name = $"Different {_suffix}", displayOrder = 1, isActive = true });

            dupCode.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // ===== DUPLICATE NAME (case-insensitive) -> validation 400 =====
            var dupName = await PostAsync(
                "/api/permission-actions",
                new { code = $"diff_{_suffix}", name = nameA.ToUpperInvariant(), displayOrder = 1, isActive = true });

            dupName.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // ===== UPDATE (metadata) -> success =====
            var update = await PutAsync(
                $"/api/permission-actions/{actionAId}",
                new { name = $"{nameA} Updated", description = "updated", displayOrder = 200, isActive = true });

            update.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            update.Body!.Success.Should().BeTrue();
            update.Body.Data!.Name.Should().Be($"{nameA} Updated");

            // ===== GET ALL -> contains A and B =====
            var getAll = await GetListAsync("/api/permission-actions");
            getAll.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            getAll.Body!.Data!.Should().Contain(x => x.Id == actionAId);
            getAll.Body.Data!.Should().Contain(x => x.Id == actionBId);

            // ===== GET BY ID -> A =====
            var getById = await GetAsync($"/api/permission-actions/{actionAId}");
            getById.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            getById.Body!.Data!.Id.Should().Be(actionAId!.Value);

            // ===== DELETE success (B is not part of catalog) =====
            var deleteB = await DeleteAsync($"/api/permission-actions/{actionBId}");
            deleteB.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            deleteB.Body!.Success.Should().BeTrue();
            actionBId = null;

            // ===== DELETE when referenced (A is part of catalog) -> business error =====
            var deleteA = await DeleteAsync($"/api/permission-actions/{actionAId}");
            deleteA.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            deleteA.Body!.Success.Should().BeFalse();
        }
        finally
        {
            await CleanupAsync(actionAId, actionBId);
        }
    }

    // ================= HTTP helpers =================

    private async Task<(HttpResponseMessage Response, ApiResponse<PermActionDto>? Body)>
        PostAsync(string url, object payload)
    {
        var response = await _client.PostAsync(url, JsonContent(payload));
        return (response, await ReadAsync<ApiResponse<PermActionDto>>(response));
    }

    private async Task<(HttpResponseMessage Response, ApiResponse<PermActionDto>? Body)>
        PutAsync(string url, object payload)
    {
        var response = await _client.PutAsync(url, JsonContent(payload));
        return (response, await ReadAsync<ApiResponse<PermActionDto>>(response));
    }

    private async Task<(HttpResponseMessage Response, ApiResponse<PermActionDto>? Body)>
        GetAsync(string url)
    {
        var response = await _client.GetAsync(url);
        return (response, await ReadAsync<ApiResponse<PermActionDto>>(response));
    }

    private async Task<(HttpResponseMessage Response, ApiResponse<List<PermActionDto>>? Body)>
        GetListAsync(string url)
    {
        var response = await _client.GetAsync(url);
        return (response, await ReadAsync<ApiResponse<List<PermActionDto>>>(response));
    }

    private async Task<(HttpResponseMessage Response, ApiResponse<bool>? Body)>
        DeleteAsync(string url)
    {
        var response = await _client.DeleteAsync(url);
        return (response, await ReadAsync<ApiResponse<bool>>(response));
    }

    private static StringContent JsonContent(object payload)
    {
        return new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");
    }

    private static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        return JsonSerializer.Deserialize<T>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // ================= cleanup =================

    private async Task CleanupAsync(Guid? aId, Guid? bId)
    {
        var ids = new[] { aId, bId }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        if (ids.Count == 0)
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationIdentityDbContext>();

        // Hapus Permission (katalog) yang dihasilkan action uji lebih dulu
        // (FK PermissionAction -> Permission bersifat Restrict).
        var perms = await db.Permissions
            .Where(p => ids.Contains(p.PermissionActionId))
            .ToListAsync();

        if (perms.Count > 0)
        {
            db.Permissions.RemoveRange(perms);
            await db.SaveChangesAsync();
        }

        var actions = await db.PermissionActions
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();

        if (actions.Count > 0)
        {
            db.PermissionActions.RemoveRange(actions);
            await db.SaveChangesAsync();
        }
    }

    private sealed class PermActionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
