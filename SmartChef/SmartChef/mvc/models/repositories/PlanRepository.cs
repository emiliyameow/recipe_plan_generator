using Npgsql;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.dto.request;

namespace SmartChef.mvc.models.repositories;

public class PlanRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PlanRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }
    
    public async Task <bool> UserHavePlanForToday(long userId)
    {
        const string sql = @"
            SELECT 
                generate_date
            FROM food_database.user_plan
            WHERE user_id = @user_id;
        ";
            
        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("user_id", userId);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            DateTime generate_date = reader.GetDateTime(reader.GetOrdinal("generate_date"));
            if (generate_date.Month == DateTime.Now.Month && generate_date.Day == DateTime.Now.Day)
            {
                return true;
            }
            return false;
        }
        
        return false; //нет записей в таблице о юзере 
    }
    
    public async Task<RecipeDtoFromApi?> GetRecipeAsync(long recipeId)
    {
        const string sql = @"
        SELECT 
            recipe_id,
            image,
            title,
            ready_in_minutes,
            servings,
            source_url,
            calories,
            proteins,
            carbs,
            fats
        FROM food_database.recipes_from_plans
        WHERE recipe_id = @recipe_id;
    ";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("recipe_id", recipeId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null; // рецепт не найден

        var recipe = new RecipeDtoFromApi
        {
            Id = reader.GetInt32(reader.GetOrdinal("recipe_id")),
            Image = reader.IsDBNull(reader.GetOrdinal("image"))
                ? null : reader.GetString(reader.GetOrdinal("image")),
            Title = reader.IsDBNull(reader.GetOrdinal("title"))
                ? null 
                : reader.GetString(reader.GetOrdinal("title")),
            ReadyInMinutes = reader.IsDBNull(reader.GetOrdinal("ready_in_minutes"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("ready_in_minutes")),
            Servings = reader.IsDBNull(reader.GetOrdinal("servings"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("servings")),
            SourceUrl = reader.IsDBNull(reader.GetOrdinal("source_url"))
                ? null
                : reader.GetString(reader.GetOrdinal("source_url")),
            Calories = reader.GetDouble(reader.GetOrdinal("calories")),
            Proteins = reader.GetDouble(reader.GetOrdinal("proteins")),
            Carbs = reader.GetDouble(reader.GetOrdinal("carbs")),
            Fats = reader.GetDouble(reader.GetOrdinal("fats"))
        };

        return recipe;
    }
    
    public async Task<PlanOfUser?> GetRecipePlan(long userId)
    {
        const string sql1 = @"
            SELECT 
                recipe_breakfast_id,
                recipe_dinner_id,
                recipe_lunch_id,
                calories_plan,
                proteins_plan,
                carbs_plan,
                fats_plan
            FROM food_database.user_plan
            WHERE user_id = @user_id;
        ";
        
        await using var cmd = _dataSource.CreateCommand(sql1);
        cmd.Parameters.AddWithValue("user_id", userId);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            var breakfastRecipeId= reader.GetInt64(reader.GetOrdinal("recipe_breakfast_id"));
            var lunchRecipeId = reader.GetInt64(reader.GetOrdinal("recipe_lunch_id"));
            var dinnerRecipeId = reader.GetInt64(reader.GetOrdinal("recipe_dinner_id"));
            
            var caloriesPlan = reader.GetDouble(reader.GetOrdinal("calories_plan"));
            var proteinPlan = reader.GetDouble(reader.GetOrdinal("proteins_plan"));
            var carbsPlan = reader.GetDouble(reader.GetOrdinal("carbs_plan"));
            var fatsPlan = reader.GetDouble(reader.GetOrdinal("fats_plan"));
            
            var breakfastRecipe = await GetRecipeAsync(breakfastRecipeId);
            var lunchRecipe = await GetRecipeAsync(lunchRecipeId);
            var dinnerRecipe = await GetRecipeAsync(dinnerRecipeId);

            var plan = new PlanOfUser()
            {
                Recipes = [breakfastRecipe!, lunchRecipe!, dinnerRecipe!],
                CaloriesPlan = caloriesPlan,
                ProteinsPlan = proteinPlan,
                CarbsPlan = carbsPlan,
                FatsPlan = fatsPlan,
            };
            
            return plan;
        }

        return null;
    }
    
    
    public async Task SaveRecipePlanUser(long userId, RecipeDtoFromApi[] generatedPlanRecipes, PlanRequestForPlan planRequestForPlan, CancellationToken ct = default)
    {
        const string sql2 = @"
        INSERT INTO food_database.user_plan (
            user_id, generate_date, recipe_breakfast_id, recipe_lunch_id, recipe_dinner_id,
            calories_plan, proteins_plan, carbs_plan, fats_plan
        )
        VALUES (
            @user_id, CURRENT_DATE, @recipe_breakfast_id, @recipe_lunch_id, @recipe_dinner_id,
            @calories_plan, @proteins_plan, @carbs_plan, @fats_plan    
        )
        ON CONFLICT (user_id)
        DO UPDATE
            SET generate_date = EXCLUDED.generate_date,
                recipe_breakfast_id = EXCLUDED.recipe_breakfast_id,
                recipe_lunch_id = EXCLUDED.recipe_lunch_id,
                recipe_dinner_id = EXCLUDED.recipe_dinner_id,
                calories_plan = EXCLUDED.calories_plan,
                proteins_plan = EXCLUDED.proteins_plan,
                carbs_plan = EXCLUDED.carbs_plan,
                fats_plan = EXCLUDED.fats_plan
        WHERE user_plan.generate_date IS DISTINCT FROM CURRENT_DATE;
        ";

        await using var cmd2 = _dataSource.CreateCommand(sql2);
        cmd2.Parameters.AddWithValue("user_id", userId);
        cmd2.Parameters.AddWithValue("recipe_breakfast_id", generatedPlanRecipes[0].Id);
        cmd2.Parameters.AddWithValue("recipe_lunch_id", generatedPlanRecipes[1].Id);
        cmd2.Parameters.AddWithValue("recipe_dinner_id", generatedPlanRecipes[2].Id);
        
        cmd2.Parameters.AddWithValue("calories_plan", planRequestForPlan.Calories);
        cmd2.Parameters.AddWithValue("proteins_plan", planRequestForPlan.Proteins);
        cmd2.Parameters.AddWithValue("carbs_plan", planRequestForPlan.Carbs);
        cmd2.Parameters.AddWithValue("fats_plan", planRequestForPlan.Fats);
        
        int affected = await cmd2.ExecuteNonQueryAsync(ct);
        Console.WriteLine($"Вставлено записей: {affected}");
    }

    public async Task SaveRecipesAsync(RecipeDtoFromApi[] generatedPlanRecipes)
    {
        for (int i = 0; i < 3; i++)
        {
            var recipe = generatedPlanRecipes[i];
            
            const string sql1 = @"
                INSERT INTO food_database.recipes_from_plans (
                    recipe_id, image, title, ready_in_minutes, servings, source_url,
                    calories, proteins, carbs, fats
                )
                VALUES (
                    @recipe_id, @image, @title, @ready_in_minutes, @servings, @source_url,
                    @calories, @proteins, @carbs, @fats
                )
                ON CONFLICT DO NOTHING;";

            await using var cmd = _dataSource.CreateCommand(sql1);
            cmd.Parameters.AddWithValue("recipe_id", recipe.Id);
            cmd.Parameters.AddWithValue("image", recipe.Image ?? "");
            cmd.Parameters.AddWithValue("title", recipe.Title);
            cmd.Parameters.AddWithValue("ready_in_minutes", recipe.ReadyInMinutes ?? 0);
            cmd.Parameters.AddWithValue("servings", recipe.Servings);
            cmd.Parameters.AddWithValue("source_url", recipe.SourceUrl ?? "");
            cmd.Parameters.AddWithValue("calories", recipe.Calories);
            cmd.Parameters.AddWithValue("proteins", recipe.Proteins);
            cmd.Parameters.AddWithValue("carbs", recipe.Carbs);
            cmd.Parameters.AddWithValue("fats", recipe.Fats);

            int id = await cmd.ExecuteNonQueryAsync();
            
            Console.WriteLine("Id рецепта cохранилось в базу " + recipe.Id );
            
        }
    }
}