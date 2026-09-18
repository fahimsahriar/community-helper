using CommunityHelper.Application.Features.Auth;
using CommunityHelper.Application.Features.Auth.Commands.GoogleLogin;
using CommunityHelper.Application.Features.Auth.Commands.Login;
using CommunityHelper.Application.Features.Auth.Commands.Logout;
using CommunityHelper.Application.Features.Auth.Commands.Refresh;
using CommunityHelper.Application.Features.Auth.Commands.RegisterUser;
using CommunityHelper.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CommunityHelper.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(ISender mediator) : ControllerBase
{
    public sealed record RegisterRequest(string Email, string Password, string Role);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshRequest(string RefreshToken);
    public sealed record GoogleRequest(string Code, string? Role);
    public sealed record LogoutRequest(string RefreshToken);

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResultDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.Email, request.Password, request.Role),
            cancellationToken);

        return CreatedAtAction(nameof(Me), result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RefreshCommand(request.RefreshToken),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Google(
        [FromBody] GoogleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ExchangeGoogleCodeCommand(request.Code, request.Role ?? "volunteer"),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(result);
    }
}
