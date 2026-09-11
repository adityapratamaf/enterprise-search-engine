using SearchEngine.Application.Features.PermissionActions.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;

namespace SearchEngine.Application.Features.PermissionActions.Mappings;

public class PermissionActionMappingConfig
    : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PermissionAction, PermissionActionResponse>();

        config.NewConfig<CreatePermissionActionRequest, PermissionAction>();

        config.NewConfig<UpdatePermissionActionRequest, PermissionAction>();
    }
}
