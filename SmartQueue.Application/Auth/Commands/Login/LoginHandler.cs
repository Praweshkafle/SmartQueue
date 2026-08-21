using MediatR;
using SmartQueue.Application.Auth.Commands.Register;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public LoginHandler(IUnitOfWork uow, IJwtService jwt)
    {
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email.ToLower().Trim(), ct);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var token = _jwt.GenerateToken(user);
        return Result<AuthResponse>.Success(
            new AuthResponse(user.Id, user.Email, user.Role, token));
    }
}