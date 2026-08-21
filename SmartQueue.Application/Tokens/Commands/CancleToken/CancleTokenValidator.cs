using System.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace SmartQueue.Application.Tokens.Commands.CancleToken;

public class CancleTokenValidator : AbstractValidator<CancelTokenCommand>
{
    public CancleTokenValidator()
    {
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.TokenId).NotEmpty();
    }
}