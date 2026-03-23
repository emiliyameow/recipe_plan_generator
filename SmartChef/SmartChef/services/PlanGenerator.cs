using SmartChef.core.exceptions;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.dto.request;

namespace SmartChef.services;

public static class PlanGenerator
{
    public static RecipeDtoFromApi[] GeneratePlanSecondVersion(
        PlanRequestForPlan planRequestForPlan,
        List<RecipeDtoFromApi> recipes,
        PlanOfUser? planOfUser)

    {
        long[]? previousRecipesId = null;
        
        if (planOfUser != null && planOfUser.Recipes.Length == 3)
        {
            previousRecipesId = new long[3];
            previousRecipesId[0] = planOfUser.Recipes[0].Id;
            previousRecipesId[1] = planOfUser.Recipes[1].Id;
            previousRecipesId[2] = planOfUser.Recipes[2].Id;
        }
        
        // Разделяем рецепты на завтраки и обед/ужин
        List<RecipeDtoFromApi> breakfastRecipes = new List<RecipeDtoFromApi>();
        List<RecipeDtoFromApi> lunchDinnerRecipes = new List<RecipeDtoFromApi>();

        bool checkTimeForBreakfast = planRequestForPlan.BreakfastTime > 0;
        bool checkTimeForLunchDinner = planRequestForPlan.LunchDinnerTime > 0;

        foreach (var recipe in recipes)
        {
            if (previousRecipesId != null && previousRecipesId.Length == 3)
            {
                if (previousRecipesId.Contains(recipe.Id))
                {
                    continue;
                }
            }
            
            if (recipe.DishTypes.Contains("breakfast") 
                || recipe.DishTypes.Contains("morning meal") 
                || recipe.DishTypes.Contains("brunch") 
                || recipe.DishTypes.Contains("salad"))
            {
                if (!checkTimeForBreakfast || (recipe.ReadyInMinutes.HasValue && recipe.ReadyInMinutes.Value <= planRequestForPlan.BreakfastTime + 15)
                   )
                    breakfastRecipes.Add(recipe);
            }
            else if (recipe.DishTypes.Contains("side dish")
                     || recipe.DishTypes.Contains("main course")
                     || recipe.DishTypes.Contains("main dish")
                     || recipe.DishTypes.Contains("lunch"))
            {
                if (!checkTimeForLunchDinner || (recipe.ReadyInMinutes.HasValue && recipe.ReadyInMinutes.Value <= planRequestForPlan.LunchDinnerTime + 15))
                    lunchDinnerRecipes.Add(recipe);
            }
            
        }

        RecipeDtoFromApi bestBreakfast = null;
        RecipeDtoFromApi bestLunch = null;
        RecipeDtoFromApi bestDinner = null;
        double bestScore = double.MaxValue;

        double targetProtein = planRequestForPlan.Proteins;
        double targetFat = planRequestForPlan.Fats;
        double targetCarbs = planRequestForPlan.Carbs;
        double targetCalories = planRequestForPlan.Calories;

        Console.WriteLine($"Количество завтраков: {breakfastRecipes.Count}, Количество обедов и ужинов: {lunchDinnerRecipes.Count}");

        foreach (var breakfast in breakfastRecipes)
        {
            // Проверка на существующий завтрак
            if (planRequestForPlan.RecipePlanExist && breakfast.Id == planRequestForPlan.BreakfastId)
                continue;

            foreach (var lunch in lunchDinnerRecipes)
            {
                // Проверка на существующий обед
                if (planRequestForPlan.RecipePlanExist && lunch.Id == planRequestForPlan.LunchId)
                    continue;

                foreach (var dinner in lunchDinnerRecipes)
                {
                    if (lunch.Id == dinner.Id)
                        continue; // не брать один и тот же рецепт на обед и ужин

                    // Проверка на существующий ужин
                    if (planRequestForPlan.RecipePlanExist && dinner.Id == planRequestForPlan.DinnerId)
                        continue;

                    /*
                    // Проверка времени приготовления (дополнительно)
                    if ((checkTimeForBreakfast && breakfast.ReadyInMinutes.HasValue && breakfast.ReadyInMinutes.Value > planRequestForPlan.BreakfastTime + 15)
                        || (checkTimeForLunchDinner && lunch.ReadyInMinutes.HasValue && lunch.ReadyInMinutes.Value > planRequestForPlan.LunchDinnerTime + 20)
                        || (checkTimeForLunchDinner && dinner.ReadyInMinutes.HasValue && dinner.ReadyInMinutes.Value > planRequestForPlan.LunchDinnerTime + 20))
                        continue;
                        */

                    // Считаем суммарные БЖУ и калории
                    double totalProtein = breakfast.Proteins + lunch.Proteins + dinner.Proteins;
                    double totalFat = breakfast.Fats + lunch.Fats + dinner.Fats;
                    double totalCarbs = breakfast.Carbs + lunch.Carbs + dinner.Carbs;
                    double totalCalories = breakfast.Calories + lunch.Calories + dinner.Calories;

                    // Считаем score как квадрат отклонений 
                    double score =
                        Math.Pow(totalProtein - targetProtein, 2) +
                        Math.Pow(totalFat - targetFat, 2) +
                        Math.Pow(totalCarbs - targetCarbs, 2);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBreakfast = breakfast;
                        bestLunch = lunch;
                        bestDinner = dinner;
                    }
                }
            }
        }

        
        if (bestBreakfast != null && bestLunch != null && bestDinner != null)
        {
            return new RecipeDtoFromApi[] { bestBreakfast, bestLunch, bestDinner };
        }
        
        throw new HttpException(501, "Suitable meal plan not found. Try to change your preferences.");
        
        var caloriesPlan = bestBreakfast.Calories + bestDinner.Calories + bestLunch.Calories;
        var proteins = bestBreakfast.Proteins + bestDinner.Proteins + bestLunch.Proteins;
        var carbohydrates = bestBreakfast.Carbs + bestDinner.Carbs + bestLunch.Carbs;
        var fats = bestBreakfast.Fats + bestDinner.Fats + bestLunch.Fats; 
    }
}