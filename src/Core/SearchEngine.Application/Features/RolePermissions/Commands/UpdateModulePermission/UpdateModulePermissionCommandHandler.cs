using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace SearchEngine.Application.Features.RolePermissions.Commands.UpdateModulePermission;

public class UpdateModulePermissionCommandHandler
    : IRequestHandler<
        UpdateModulePermissionCommand,
        Result<bool>>
{
    private readonly IApplicationIdentityDbContext _context;

    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly IAuditActionService _auditActionService;

    public UpdateModulePermissionCommandHandler(
        IApplicationIdentityDbContext context,
        RoleManager<IdentityRole> roleManager,
        IAuditActionService auditActionService)
    {
        _context = context;
        _roleManager = roleManager;
        _auditActionService = auditActionService;
    }

    public async Task<Result<bool>> Handle(
        UpdateModulePermissionCommand request,
        CancellationToken cancellationToken)
    {
        return await RolePermissionSynchronizer.SyncAsync(
            _context,
            _roleManager,
            _auditActionService,
            request.Request,
            cancellationToken);
    }
}
