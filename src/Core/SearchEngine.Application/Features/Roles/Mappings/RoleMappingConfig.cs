using SearchEngine.Application.Features.Roles.DTOs;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace SearchEngine.Application.Features.Roles.Mappings;

public class RoleMappingConfig
    : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<IdentityRole, ReadRoleResponse>();
    }
}
