using FluentValidation;
using ProjectManagement.DTOs.Jira;

namespace ProjectManagement.Validators;

public class JiraImportDtoValidator : AbstractValidator<JiraImportDto>
{
    public JiraImportDtoValidator()
    {
        RuleFor(x => x.JiraIssueKey)
            .NotEmpty().WithMessage("JiraIssueKey is required.")
            .MaximumLength(50).WithMessage("JiraIssueKey must not exceed 50 characters.")
            .Matches(@"^[A-Z][A-Z0-9_]+-\d+$")
            .WithMessage("JiraIssueKey must be in the format PROJECT-123.");

        RuleFor(x => x.ReleaseId)
            .GreaterThan(0).WithMessage("A valid ReleaseId is required.");

        RuleFor(x => x.AssignedToUserId)
            .GreaterThan(0).WithMessage("A valid AssignedToUserId is required.");

        RuleFor(x => x.AssignedToQAUserId)
            .GreaterThan(0).WithMessage("A valid AssignedToQAUserId is required.");
    }
}
