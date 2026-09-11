namespace SearchEngine.Application.Common.Models;

public class ModulePermissionResponse
{
    public string ModuleId { get; set; } = default!;

    public string ModuleName { get; set; } = default!;

    public string ModulePath { get; set; } = default!;

    public List<string> Actions { get; set; } = [];
}
