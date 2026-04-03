using FluentValidation;
using ProjectManagement.DTOs.Auth;

namespace ProjectManagement.Validators;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.");
    }
}
