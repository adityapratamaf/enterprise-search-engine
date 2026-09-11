using SearchEngine.Application.Features.Auth.Commands.ChangePassword;
using SearchEngine.Application.Features.Auth.Commands.Login;
using SearchEngine.Application.Features.Auth.Commands.Logout;
using SearchEngine.Application.Features.Auth.Commands.RefreshToken;
using SearchEngine.Application.Features.Auth.Commands.UpdateProfile;
using SearchEngine.Application.Features.Auth.DTOs;
using SearchEngine.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    // =====================================================
    // LOGIN
    // =====================================================

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command)
    {
        var result =
            await _mediator.Send(command);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    // =====================================================
    // REFRESH TOKEN
    // =====================================================

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command)
    {
        var result =
            await _mediator.Send(command);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    // =====================================================
    // LOGOUT
    // =====================================================

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var result =
            await _mediator.Send(
                new LogoutCommand());

        return Ok(result);
    }

    // =====================================================
    // UPDATE PROFILE
    // =====================================================

    [HttpPut("profile")]
    [Authorize]
    [EndpointDescription(
        "Memperbarui profil milik user yang sedang login " +
        "(Username, Email, FirstName, LastName, dan Password opsional). " +
        "User hanya dapat mengubah profilnya sendiri. " +
        "Role, IsActive, dan IsSuperUser tidak dapat diubah melalui endpoint ini.")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request)
    {
        var result =
            await _mediator.Send(
                new UpdateProfileCommand(request));

        return Ok(result);
    }

    // =====================================================
    // CHANGE PASSWORD
    // =====================================================

    [HttpPut("change-password")]
    [Authorize]
    [EndpointDescription(
        "Mengubah password milik user yang sedang login. " +
        "Memerlukan current password yang benar; new password harus mengikuti " +
        "kebijakan password ASP.NET Identity dan dikonfirmasi lewat confirmPassword. " +
        "User hanya dapat mengubah passwordnya sendiri.")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var result =
            await _mediator.Send(
                new ChangePasswordCommand(request));

        return Ok(result);
    }

    // =====================================================
    // PROFILE
    // =====================================================

    [HttpGet("profile")]
    [Authorize]
    [EndpointDescription(
        "Mengambil data profile user yang sedang login berdasarkan JWT Access Token. " +
        "Endpoint tidak menerima parameter apa pun dan hanya mengembalikan profile milik user itu sendiri.")]
    public async Task<IActionResult> Profile()
    {
        var result =
            await _mediator.Send(
                new GetCurrentUserQuery());

        return Ok(result);
    }
}
