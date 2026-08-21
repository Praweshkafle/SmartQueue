using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;

public record AuthResponse(
    Guid UserId,
    string Email,
    string Role,
    string Token
);