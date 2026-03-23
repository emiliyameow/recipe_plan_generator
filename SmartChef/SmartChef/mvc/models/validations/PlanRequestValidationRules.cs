using FluentValidation;

using SmartChef.mvc.models.dto.request;

namespace SmartChef.mvc.models.validations;

public class PlanRequestForPlanValidator : AbstractValidator<PlanRequestForPlan>
{
    public PlanRequestForPlanValidator()
    {
        // --- Calories ---
        RuleFor(plan => plan.Calories)
            .GreaterThan(0).WithMessage("Calories must be greater than 0.")
            .InclusiveBetween(1200, 4000)
            .WithMessage("Calories must be between 1200 and 4000.");
        
        // --- Cuisine ---
        RuleFor(plan => plan.Cuisine)
            .Must(cuisines => cuisines == null || cuisines.Count == 0 || cuisines.Count >= 4)
            .WithMessage("Cuisine list must contain at least 4 items or be empty.");

        // --- Intolerances ---
        RuleFor(plan => plan.Intolerances)
            .Must(intols => intols == null || intols.Count == 0 || intols.Count < 5)
            .WithMessage("Intolerances list must contain no more than 4 items.");
        
        
        // --- BreakfastTime ---
        RuleFor(plan => plan.BreakfastTime)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Breakfast time must be greater than 0.");

        // --- LunchDinnerTime ---
        RuleFor(plan => plan.LunchDinnerTime)
            .GreaterThanOrEqualTo(0).WithMessage("Lunch time must be greater than 0.");
    }
}
