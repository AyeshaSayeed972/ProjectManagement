using FluentValidation;
using ProjectManagement.DAL;

namespace ProjectManagement.BLL;

public class LinkJiraIssueDtoValidator : AbstractValidator<LinkJiraIssueDto>
{
    public LinkJiraIssueDtoValidator()
    {
        RuleFor(x => x.JiraIssueKey)
            .NotEmpty().WithMessage("JiraIssueKey is required.")
            .MaximumLength(50).WithMessage("JiraIssueKey must not exceed 50 characters.")
            .Matches(@"^[A-Z][A-Z0-9_]+-\d+$")
            .WithMessage("JiraIssueKey must be in the format PROJECT-123.");
    }
}
