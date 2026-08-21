using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Tokens.Commands.CancleToken;

public record CancelTokenCommand(
    Guid TokenId,
    Guid RequestingUserId,
    string RequestingUserRole
) : IRequest<Result<bool>>;