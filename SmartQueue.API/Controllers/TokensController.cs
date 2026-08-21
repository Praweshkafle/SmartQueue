using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartQueue.Application.Tokens.Commands.CancleToken;
using SmartQueue.Application.Tokens.Commands.IssueToken;
using SmartQueue.Application.Tokens.Queries.GetMyToken;

namespace SmartQueue.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TokensController : ControllerBase
{
    private readonly IMediator _mediator;
    public TokensController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [EnableRateLimiting("per-user")]
    public async Task<IActionResult> IssueToken(
        [FromBody] IssueTokenRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new IssueTokenCommand(
            request.ServiceId,
            userId,
            request.IdempotencyKey);

        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
    
    [HttpGet("my")]
    public async Task<IActionResult> GetMyToken(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetMyTokenQuery(userId), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelToken(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role)!;

        var result = await _mediator.Send(
            new CancelTokenCommand(id, userId, userRole), ct);

        return result.IsSuccess
            ? Ok(new { message = "Token cancelled." })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}


public record IssueTokenRequest(Guid ServiceId, string IdempotencyKey);
