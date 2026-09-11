namespace SearchEngine.Application.Features.Modules.DTOs;

public class UpdateModuleRequest
{
    public string ModuleId { get; set; } = default!;
    public string ModuleName { get; set; } = default!;
    public string ModulePath { get; set; } = default!;
}
