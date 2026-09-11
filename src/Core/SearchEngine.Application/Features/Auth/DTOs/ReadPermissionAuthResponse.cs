namespace SearchEngine.Application.Features.Auth.DTOs;

public class ReadPermissionAuthResponse
{
    public string ModuleId { get; set; } = default!;

    public string ModuleName { get; set; } = default!;

    public List<string> Actions { get; set; } = [];
}
