using FluentValidation;

namespace SmartQueue.Application.Services.Commands.CreateService;

public class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.MaxQueueSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(500);
    }
}