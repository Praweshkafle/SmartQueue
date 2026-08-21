using FluentValidation;

namespace SmartQueue.Application.Services.Commands.UpdateService;

public class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaxQueueSize).GreaterThan(0).LessThanOrEqualTo(500);
    }
}