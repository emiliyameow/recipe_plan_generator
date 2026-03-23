using FluentValidation;
using SmartChef.mvc.models.dto;

namespace SmartChef.mvc.models.validations;

public class UserValidationRules : AbstractValidator<UserRegisterModel>
{
    public UserValidationRules()
    {
        // --- Username ---
        RuleFor(user => user.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Matches(@"^[a-zA-Z0-9_]{5,20}$")
            .WithMessage("Username must be 5–20 characters and contain only letters, numbers, and underscores.");

        // --- Login ---
        RuleFor(user => user.Login)
            .NotEmpty().WithMessage("Login is required.")
            .Matches(@"^[a-zA-Z0-9_]{5,20}$")
            .WithMessage("Login must be 5–20 characters and contain only letters, numbers, and underscores.");

        // --- Email ---
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        
        // --- Password ---
        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one number.");
    }
}