using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Extensions;
using SearchEngine.Application.Features.Users.DTOs;
using SearchEngine.Infrastructure.Identity.Context;
using SearchEngine.Infrastructure.Identity.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Services;

public class UserService
{
    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly RoleService
        _roleService;

    private readonly ApplicationIdentityDbContext
        _context;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleService roleService,
        ApplicationIdentityDbContext context)
    {
        _userManager = userManager;
        _roleService = roleService;
        _context = context;
    }

    // ========== Create ==========
    public async Task<Result<ReadUserResponse>>
        CreateUserAsync(
            CreateUserRequest request)
    {
        var exists = await _userManager.Users
            .AnyAsync(x =>
                x.Email == request.Email);

        if (exists)
        {
            return Result<ReadUserResponse>
                .Failure("Email already exists");
        }

        var roleExists = await _roleService
            .RoleExistsAsync(
                request.Role!);

        if (!roleExists)
        {
            return Result<ReadUserResponse>
                .Failure("Role not found");
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            IsActive = true,
            IsSuperUser = request.IsSuperUser
        };

        var result = await _userManager
            .CreateAsync(
                user,
                request.Password!);

        if (!result.Succeeded)
        {
            return Result<ReadUserResponse>
                .Failure(
                "Failed create user",
                result.Errors);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                request.Role!);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return Result<ReadUserResponse>
                .Failure(
                    "Failed assign role",
                    roleResult.Errors);
        }

        var dto = user.Adapt<ReadUserResponse>();

        dto.Role = request.Role!;

        return Result<ReadUserResponse>
            .SuccessResult(
                dto,
                "User created successfully");
    }

    // ========== Get All ==========
    public async Task<
        Result<
            PaginatedResult<ReadUserResponse>>>
        GetAllUsersAsync(
            PaginationRequest request)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(
            request.Search))
        {
            query = query.Where(x =>
                x.UserName!.Contains(
                    request.Search) ||

                x.Email!.Contains(
                    request.Search) ||

                x.FirstName!.Contains(
                    request.Search) ||

                x.LastName!.Contains(
                    request.Search));
        }

        // SORT — always end with a stable Id tie-breaker so offset
        // pagination is deterministic and page boundaries never skip or
        // duplicate rows when the primary sort column has duplicate values.
        var ordered = request.SortBy?.ToLower() switch
        {
            "email" => request.IsDescending
                ? query.OrderByDescending(
                    x => x.Email)
                : query.OrderBy(
                    x => x.Email),

            "firstname" => request.IsDescending
                ? query.OrderByDescending(
                    x => x.FirstName)
                : query.OrderBy(
                    x => x.FirstName),

            _ => request.IsDescending
                ? query.OrderByDescending(
                    x => x.UserName)
                : query.OrderBy(
                    x => x.UserName)
        };

        query = ordered.ThenBy(x => x.Id);

        var result = await query
            .ToPaginatedResultAsync<
                ApplicationUser,
                ReadUserResponse>(request);

        var userIds = result.Items
            .Select(x => x.Id)
            .ToList();

        var roleByUser = await (
            from userRole in _context.UserRoles
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId)
            select new
            {
                userRole.UserId,
                RoleName = role.Name
            })
            .AsNoTracking()
            .ToListAsync();

        var roleLookup = roleByUser
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.First().RoleName);

        foreach (var item in result.Items)
        {
            item.Role =
                roleLookup.TryGetValue(
                    item.Id,
                    out var roleName)
                    ? roleName ?? "-"
                    : "-";
        }

        return Result<
            PaginatedResult<ReadUserResponse>>
            .SuccessResult(result);
    }

    // ========== Get By Id ==========
    public async Task<Result<ReadUserResponse>>
        GetUserByIdAsync(
            string id)
    {
        var user = await _userManager
            .FindByIdAsync(id);

        if (user is null)
        {
            return Result<ReadUserResponse>
                .Failure("User not found");
        }

        var roles = await _userManager
            .GetRolesAsync(user);

        var dto = user.Adapt<ReadUserResponse>();

        dto.Role =
            roles.FirstOrDefault()
            ?? "-";

        return Result<ReadUserResponse>
            .SuccessResult(dto);
    }

    // ========== Update ==========
    public async Task<Result<ReadUserResponse>>
        UpdateUserAsync(
            string id,
            UpdateUserRequest request)
    {
        var user = await _userManager
            .FindByIdAsync(id);

        if (user is null)
        {
            return Result<ReadUserResponse>
                .Failure("User not found");
        }

        var roleExists = await _roleService
            .RoleExistsAsync(
                request.Role!);

        if (!roleExists)
        {
            return Result<ReadUserResponse>
                .Failure("Role not found");
        }

        user.UserName = request.Username;
        user.Email = request.Email;
        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;
        user.IsActive = request.IsActive;
        user.IsSuperUser = request.IsSuperUser;

        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager
            .GetRolesAsync(user);

        await _userManager.RemoveFromRolesAsync(
            user,
            currentRoles);

        await _userManager.AddToRoleAsync(
            user,
            request.Role!);

        var dto = user.Adapt<ReadUserResponse>();

        dto.Role = request.Role!;

        return Result<ReadUserResponse>
            .SuccessResult(
                dto,
                "User updated successfully");
    }

    // ========== Delete ==========
    public async Task<Result<string>>
        DeleteUserAsync(
            string id)
    {
        var user = await _userManager
            .FindByIdAsync(id);

        if (user is null)
        {
            return Result<string>
                .Failure("User not found");
        }

        await _userManager.DeleteAsync(user);

        return Result<string>
            .SuccessResult(
                string.Empty,
                "User deleted successfully");
    }

    // ========== Toogle ==========
    public async Task<Result<string>>
        ToggleUserStatusAsync(
            string id)
    {
        var user = await _userManager
            .FindByIdAsync(id);

        if (user is null)
        {
            return Result<string>
                .Failure("User not found");
        }

        user.IsActive = !user.IsActive;

        await _userManager.UpdateAsync(user);

        return Result<string>
            .SuccessResult(
                string.Empty,
                $"User status changed to {user.IsActive}");
    }
}
