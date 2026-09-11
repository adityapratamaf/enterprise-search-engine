using System.Text;
using System.Text.Json;

namespace SearchEngine.Application.IntegrationTests.Common;

public static class AuthTestHelper
{
    public static async Task<AuthResponse>
        LoginAsync(
            HttpClient client)
    {
        var payload = new
        {
            email = "admin@searchengine.local",
            password = "Admin123!"
        };

        var json =
            JsonSerializer.Serialize(
                payload);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await client.PostAsync(
                "/api/auth/login",
                content);

        response.EnsureSuccessStatusCode();

        var body =
            await response.Content
                .ReadAsStringAsync();

        var result =
            JsonSerializer.Deserialize<
                ApiResponse<AuthResponse>>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

        return result!.Data!;
    }
}
