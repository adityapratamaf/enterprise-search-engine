namespace SearchEngine.Application.IntegrationTests.Common;

public class AuthResponse
{
    public string Access { get; set; } = string.Empty;
    public string Refresh { get; set; } = string.Empty;
    public long Expiry { get; set; }
}
