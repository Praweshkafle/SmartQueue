using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Audit.Queries.GetAuditLog;

public record GetAuditLogQuery(Guid TokenId)
    : IRequest<Result<IEnumerable<AuditLogResponse>>>;

public record AuditLogResponse(
    Guid Id,
    string Action,
    string PerformedBy,
    DateTime PerformedAt,
    string Notes
);