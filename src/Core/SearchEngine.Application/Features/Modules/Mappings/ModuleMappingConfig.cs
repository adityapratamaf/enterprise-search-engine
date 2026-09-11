using SearchEngine.Application.Features.Modules.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;

namespace SearchEngine.Application.Features.Modules.Mappings;

public class ModuleMappingConfig
    : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Module, ReadModuleResponse>();

        config.NewConfig<CreateModuleRequest, Module>();

        config.NewConfig<UpdateModuleRequest, Module>();
    }
}
