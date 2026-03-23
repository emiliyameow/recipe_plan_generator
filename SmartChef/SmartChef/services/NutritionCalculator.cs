using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.dto.request;

namespace SmartChef.services;

public static class NutritionCalculator
{
    public static (int maxCaloriesForOneMeal, int minCaloriesForOneMeal) CalculateCalories(PlanRequestForPlan planRequestForPlan)
    {
        int maxCaloriesForOneMeal = (int)(planRequestForPlan.Calories * 0.3 + 0.15 * planRequestForPlan.Calories); 
        int minCaloriesForOneMeal = (int)(planRequestForPlan.Calories * 0.3 - 0.15 * planRequestForPlan.Calories); 

        return (maxCaloriesForOneMeal, minCaloriesForOneMeal);
    }
    
    // по информации от пользователя
    public static NutrientsPreferences CalculateNutritionPlan(UserBodyInformation userInfo)
    {
        // Считаем калории по формуле Миффлина — Сан Жеора с учётом активности и цели
        double calories = CalculateDailyCalories(userInfo);

        // Распределяем калории по БЖУ в зависимости от цели
        return CalculateMacronutrients(calories, userInfo.Goal);
    }

    //по информации с калориями и целью
    public static NutrientsPreferences CalculateNutritionPlan(double calories, Goal goal)
    {
        return CalculateMacronutrients(calories, goal);
    }
    
    //по конкретным бжу
    public static NutrientsPreferences CalculateNutritionPlan(double proteins, double fats, double carbs)
    {
        return new NutrientsPreferences
        {
            Calories = proteins * 4 + fats * 4 + carbs * 9,
            Fats = fats,
            Carbs = carbs,
            Proteins = proteins
        };
    }
    
    
    // --- Расчёт общей калорийности ---
    private static double CalculateDailyCalories(UserBodyInformation userInfo)
    {
        // Базовый обмен веществ (BMR)
        double bmr = userInfo.Gender == Gender.Male
            ? 10 * userInfo.Weight + 6.25 * userInfo.Height - 5 * userInfo.Age + 5
            : 10 * userInfo.Weight + 6.25 * userInfo.Height - 5 * userInfo.Age - 161;

        // Коэффициент активности
        double activityMultiplier = userInfo.ActivityLevel switch
        {
            ActivityLevel.Sedentary => 1.2,
            ActivityLevel.Light => 1.375,
            ActivityLevel.Moderate => 1.55,
            ActivityLevel.High => 1.725,
            ActivityLevel.Extreme => 1.9,
            _ => 1.375
        };

        double maintenanceCalories = bmr * activityMultiplier;

        // Коррекция по цели
        return userInfo.Goal switch
        {
            Goal.WeightLoss => maintenanceCalories * 0.85,       // -15%
            Goal.WeightGain => maintenanceCalories * 1.15,       // +15%
            Goal.Recomposition => maintenanceCalories * 0.95,  // лёгкий дефицит (-5%)
            _ => maintenanceCalories                             // поддержание
        };
    }

    // --- Расчёт БЖУ ---
    private static NutrientsPreferences CalculateMacronutrients(double calories, Goal goal)
    {
        var plan = new NutrientsPreferences
        {
            Calories = calories
        };

        // Получаем проценты БЖУ для данной цели
        var (proteinPct, fatPct, carbPct) = GetMacrosPercent(goal);

        // Расчёт макронутриентов
        plan.Proteins = (calories * proteinPct) / 4;
        plan.Fats = (calories * fatPct) / 9;
        plan.Carbs = (calories * carbPct) / 4;

        return plan;
    }

    // --- Процентное распределение БЖУ по целям ---
    private static (double proteinPct, double fatPct, double carbPct) GetMacrosPercent(Goal goal)
    {
        switch (goal)
        {
            // Похудение: больше белка, умеренно жиров, меньше углеводов
            case Goal.WeightLoss:
                return (0.28, 0.27, 0.45);
            
            // Поддержание: сбалансированные нормы ВОЗ / Роспотребнадзора
            case Goal.Maintenance:
            // Набор массы или поддержание - одни значения 
            case Goal.WeightGain:
                return (0.18, 0.27, 0.55);
            
            // Рекомпозиция тела: высокий белок, умеренные углеводы
            case Goal.Recomposition:
                return (0.30, 0.25, 0.45);
            default: //поддержание
                return (0.18, 0.27, 0.55);
        }
    }
}
