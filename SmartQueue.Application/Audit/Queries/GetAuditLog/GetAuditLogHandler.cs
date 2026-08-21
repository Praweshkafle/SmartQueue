using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Audit.Queries.GetAuditLog;

public class GetAuditLogHandler
    : IRequestHandler<GetAuditLogQuery, Result<IEnumerable<AuditLogResponse>>>
{
    private readonly IUnitOfWork _uow;
    public GetAuditLogHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<AuditLogResponse>>> Handle(
        GetAuditLogQuery request, CancellationToken ct)
    {
        var logs = await _uow.AuditLogs.GetByTokenIdAsync(request.TokenId, ct);
        var response = logs.Select(l => new AuditLogResponse(
            l.Id, l.Action, l.PerformedBy, l.PerformedAt, l.Notes));
        return Result<IEnumerable<AuditLogResponse>>.Success(response);
    }
}