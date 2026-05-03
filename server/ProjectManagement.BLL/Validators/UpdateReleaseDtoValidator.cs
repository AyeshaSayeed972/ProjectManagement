using FluentValidation;
using ProjectManagement.DAL;

namespace ProjectManagement.BLL;

public class UpdateReleaseDtoValidator : AbstractValidator<UpdateReleaseDto>
{
    public UpdateReleaseDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}
