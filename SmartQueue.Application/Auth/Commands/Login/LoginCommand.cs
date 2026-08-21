using MediatR;
using SmartQueue.Application.Auth.Commands.Register;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;