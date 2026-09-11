namespace SearchEngine.WebAPI.Options;

public class RateLimitingOptions
{
    public RateLimitPolicyOptions Login { get; set; } = new();

    public RateLimitPolicyOptions Refresh { get; set; } = new();

    public RateLimitPolicyOptions Public { get; set; } = new();

    public RateLimitPolicyOptions Api { get; set; } = new();
}

public class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; }

    public int WindowMinutes { get; set; }
}
