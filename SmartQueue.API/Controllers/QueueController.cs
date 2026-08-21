using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQueue.Application.Queue.Commands.AdvanceQueue;
using SmartQueue.Application.Queue.Queries.GetQueueStatus;
using SmartQueue.Application.Queue.Queries.GetQueueTokens;
using SmartQueue.Domain.Entities;

namespace SmartQueue.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class QueueController : ControllerBase
{
    private readonly IMediator _mediator;
    public QueueController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{serviceId}/tokens")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetQueueTokens(Guid serviceId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetQueueTokensQuery(serviceId), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
    
    [HttpGet("{serviceId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQueueStatus(Guid serviceId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetQueueStatusQuery(serviceId), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
    
    [HttpPatch("{serviceId}/advance")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> AdvanceQueue(Guid serviceId, CancellationToken ct)
    {
        var performedBy = User.FindFirstValue(ClaimTypes.Email)!;
        var result = await _mediator.Send(new AdvanceQueueCommand(serviceId, performedBy), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}