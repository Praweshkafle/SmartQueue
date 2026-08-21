using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public RegisterHandler(IUnitOfWork uow, IJwtService jwt)
    {
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponse>> Handle(
        RegisterCommand request, CancellationToken ct)
    {
        var exists = await _uow.Users.ExistsAsync(request.Email, ct);
        if (exists)
            return Result<AuthResponse>.Failure("Email already registered.", 409);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Roles.User,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        var token = _jwt.GenerateToken(user);
        return Result<AuthResponse>.Success(
            new AuthResponse(user.Id, user.Email, user.Role, token));
    }
}