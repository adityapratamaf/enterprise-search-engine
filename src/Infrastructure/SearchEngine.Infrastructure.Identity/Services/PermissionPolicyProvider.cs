using SearchEngine.Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SearchEngine.Infrastructure.Identity.Services;

public class PermissionPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?>
        GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy != null)
        {
            return policy;
        }

        var parts = policyName.Split('.');

        if (parts.Length != 2)
        {
            return null;
        }

        return new AuthorizationPolicyBuilder()
            .AddRequirements(
                new PermissionRequirement(
                    parts[0],
                    parts[1]))
            .Build();
    }
}
