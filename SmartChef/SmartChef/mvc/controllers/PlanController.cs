using System.Text.Json;
using System.Text.Json.Serialization;
using SmartChef.core.exceptions;
using SmartChef.core.server;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.dto.request;
using SmartChef.mvc.models.repositories;
using SmartChef.services;

namespace SmartChef.mvc.controllers;

public class PlanController
{

    private readonly PlanRepository _planRepository;

    public PlanController(SpoonacularApiClient spoonacularApiClient, PlanRepository planRepository)
    {
        _spoonacularApiClient = spoonacularApiClient;
        _planRepository = planRepository;
    }

    private SpoonacularApiClient _spoonacularApiClient;
    
   
    public async Task CalculateProperCalories(HttpContextExtension ctx, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
        var body = await reader.ReadToEndAsync(cancellationToken);
        
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new HttpException(400, "Request body for calories is empty.");
        }
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        PlanRequestForCalories? request;
        try
        {
            request = JsonSerializer.Deserialize<PlanRequestForCalories>(body, options);
        }
        catch
        {
            throw new HttpException(400, "Invalid request for calories format.");
        }

        if (request == null)
        {
            throw new HttpException(400, "Invalid JSON structure for calories.");
        }

        var nutrientsPreferences = HandleProperCalories(request);
        Console.WriteLine(nutrientsPreferences);
        
        await ctx.WriteJsonAsync(nutrientsPreferences);
        
    }
    
    private NutrientsPreferences HandleProperCalories(PlanRequestForCalories requestForCalories)
    {
        switch (requestForCalories.TypeFlag)
        {
            case "without":
            {
                var nutrientsPreferences = NutritionCalculator.CalculateNutritionPlan(2200, Goal.Maintenance);;
                return nutrientsPreferences;
            }
            case "calories":
            {
                var calories = requestForCalories.Calories;
                var goal = requestForCalories.Goal;
                if (goal == null || calories == null || calories < 800 || calories > 6000)
                {
                    throw new HttpException(400, "Invalid calories format.");
                }
                
                var nutrientsPreferences = NutritionCalculator.CalculateNutritionPlan(calories.Value, goal.Value);
                return nutrientsPreferences;
            }
            case "profile":
            {
                var userBodyInformation = requestForCalories.UserBodyInformation;
               
                if (userBodyInformation == null)
                {
                    throw new HttpException(400, "Invalid profile information format.");
                }

                Console.WriteLine(JsonSerializer.Serialize(requestForCalories));

                Console.WriteLine(JsonSerializer.Serialize(userBodyInformation));
                var nutrientsPreferences = NutritionCalculator.CalculateNutritionPlan(userBodyInformation);
                return nutrientsPreferences;
            } 
            case "macronutrients":
            {
                var proteins = requestForCalories.Proteins;
                var carbs = requestForCalories.Carbs;
                var fats = requestForCalories.Fats;

                if (proteins == null || carbs == null || fats == null)
                {
                    throw new HttpException(400, "Invalid nutrients format.");
                }
                
                var nutrientsPreferences =  NutritionCalculator.CalculateNutritionPlan(proteins.Value, carbs.Value, fats.Value);
                if (nutrientsPreferences.Calories < 1200 || nutrientsPreferences.Calories > 4000)
                {
                    throw new HttpException(400, "Total calories from macros are out of range (1200–4000).");
                }
                return nutrientsPreferences;
            }
            default:
            {
                throw new HttpException(400, "Invalid request for calories type.");
            }
        }
    }
    

    public async Task GeneratePlanAsync(HttpContextExtension ctx, CancellationToken ct = default)
    {
        var planRequestForPlan = ctx.BoundModel as PlanRequestForPlan;
        if (planRequestForPlan is null)
        {
            throw new HttpException(400, "Invalid request for plan.");
        }
        
        if (!ctx.IsAuthenticated)
        {
            throw new HttpException(401, "Cannot generate plan for unauthorised user");
        }
        
        bool userHavePlanToday = await _planRepository.UserHavePlanForToday(ctx.AuthUser!.UserId);
        PlanOfUser? plan = await _planRepository.GetRecipePlan(ctx.AuthUser!.UserId);
        
        if (userHavePlanToday && plan != null)
        {
            // вывод данных 
            await ctx.WriteJsonAsync(
                new
                { plan = plan, 
                    isGeneratedToday = false });
            return;
        }

        (int maxCalories, int minCalories) = NutritionCalculator.CalculateCalories(planRequestForPlan);
        var query = _spoonacularApiClient.BuildQueryForRequest(maxCalories, minCalories, planRequestForPlan, 100);
        string response = await _spoonacularApiClient.GetAsync("/recipes/complexSearch?", query);

        //string response = GetResponseFromFile();
        
        List<RecipeDtoFromApi> recipes = _spoonacularApiClient.ParseRecipes(response);
        RecipeDtoFromApi[] generatedPlanRecipes = PlanGenerator.GeneratePlanSecondVersion(planRequestForPlan, recipes, plan);
        
        await _planRepository.SaveRecipesAsync(generatedPlanRecipes);
        
        // добавление плана рецептов по пользователю
        await _planRepository.SaveRecipePlanUser(ctx.AuthUser!.UserId, generatedPlanRecipes, planRequestForPlan);

        PlanOfUser generatedPlan = new()
        {
            Recipes = generatedPlanRecipes,
            CaloriesPlan = planRequestForPlan.Calories,
            ProteinsPlan = planRequestForPlan.Proteins,
            CarbsPlan = planRequestForPlan.Carbs,
            FatsPlan = planRequestForPlan.Fats
        };
        await ctx.WriteJsonAsync(
            new {plan = generatedPlan, isGeneratedToday = true}
            );
        
    }
}