using FluentValidation;
using ProjectManagement.DTOs.Jira;

namespace ProjectManagement.Validators;

public class UpsertJiraSettingsDtoValidator : AbstractValidator<UpsertJiraSettingsDto>
{
    public UpsertJiraSettingsDtoValidator()
    {
        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("BaseUrl is required.")
            .MaximumLength(500).WithMessage("BaseUrl must not exceed 500 characters.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("BaseUrl must be a valid absolute URL (e.g. https://yourorg.atlassian.net).");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.ApiToken)
            .NotEmpty().WithMessage("ApiToken is required.")
            .MaximumLength(500).WithMessage("ApiToken must not exceed 500 characters.");
    }
}
