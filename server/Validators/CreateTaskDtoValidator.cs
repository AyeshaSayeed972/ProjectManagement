using FluentValidation;
using ProjectManagement.DTOs.Task;

namespace ProjectManagement.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ReleaseId)
            .GreaterThan(0).WithMessage("A valid ReleaseId is required.");

        RuleFor(x => x.AssignedToUserId)
            .GreaterThan(0).WithMessage("A valid AssignedToUserId is required.");

        RuleFor(x => x.AssignedToQAUserId)
            .GreaterThan(0).WithMessage("A valid AssignedToQAUserId is required.");
    }
}
