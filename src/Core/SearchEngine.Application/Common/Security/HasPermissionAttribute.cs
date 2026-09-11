using Microsoft.AspNetCore.Authorization;

namespace SearchEngine.Application.Common.Security;

public class HasPermissionAttribute
    : AuthorizeAttribute
{
    public HasPermissionAttribute(
        string module,
        string action)
    {
        Policy = $"{module}.{action}";
    }
}
