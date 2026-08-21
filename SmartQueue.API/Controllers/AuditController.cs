using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQueue.Application.Audit.Queries.GetAuditLog;
using SmartQueue.Domain.Entities;

namespace SmartQueue.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuditController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{tokenId}")]
    public async Task<IActionResult> GetAuditLog(Guid tokenId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAuditLogQuery(tokenId), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}