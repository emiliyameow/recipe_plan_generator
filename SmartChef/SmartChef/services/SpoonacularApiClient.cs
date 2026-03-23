using System.Net;
using System.Text.Json;
using SmartChef.core.exceptions;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.dto.request;

namespace SmartChef.services;

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class SpoonacularApiClient
{
    public SpoonacularApiClient(String apiKey)
    {
        _apiKey = apiKey;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }
    
    private readonly HttpClient _client;

    private readonly string _apiKey;

    private const string BaseUrl = "https://api.spoonacular.com";
    private static readonly Random Random = new Random();

    public string BuildQueryForRequest(
        int? maxCalories,
        int? minCalories,
        PlanRequestForPlan planRequestForPlan,
        int number)
    {
        var query = new List<string>();

        // Кухни (cuisine=italian,mexican)
        if (planRequestForPlan.Cuisine is { Count: >= 4 })
        {
            query.Add($"cuisine={string.Join(",", planRequestForPlan.Cuisine.Select(Uri.EscapeDataString))}");
        }

        // Диета (diet=vegan)
        if (!string.IsNullOrWhiteSpace(planRequestForPlan.Diet))
        {
            query.Add($"diet={Uri.EscapeDataString(planRequestForPlan.Diet)}");
        }

        // Непереносимости (intolerances=gluten,soy)
        if (planRequestForPlan.Intolerances is { Count: < 5 })
        {
            query.Add($"intolerances={string.Join(",", planRequestForPlan.Intolerances.Select(Uri.EscapeDataString))}");
        }

        // Добавляем параметры для фильтрации по калориям
        if (minCalories.HasValue)
        {
            query.Add($"minCalories={minCalories.Value}");
        }

        if (maxCalories.HasValue)
        {
            query.Add($"maxCalories={maxCalories.Value}");
        }

        // Дополнительные параметры
        query.Add("addRecipeInformation=true");
        query.Add("addRecipeNutrition=true");
        query.Add("sort=healthiness");
        query.Add("sortDirection=desc");
        query.Add($"number={number}");

        return string.Join("&", query);
    }


    public async Task<string> GetAsync(string endpoint, string queryParams = "")
    {
        // проверки делаем тут
        string url = $"{BaseUrl}{endpoint}?{queryParams}&apiKey={_apiKey}";

        try
        {
            HttpResponseMessage response = await _client.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.PaymentRequired)
                {
                    throw new HttpException((int)response.StatusCode, "Sorry for inconvenience, there was a problem with limited requests to Spoonacular API. Please try tomorrow.");
                }
                else
                {
                    throw new HttpException((int)response.StatusCode, "There was an error in getting recipes from Spoonacular. Please try later.");
                }
                
                //throw new Exception("Problem with recipes");
                /*throw new HttpException((int)response.StatusCode,
                    $"GET request failed: {(int)response.StatusCode} {response.ReasonPhrase}\nResponse: {responseBody}");*/
                throw new HttpException((int)response.StatusCode, "There was an error connecting to Spoonacular API with recipes. Please try tomorrow.");
            }
            // Проверяем наличие квотных заголовков
            if (response.Headers.TryGetValues("X-API-Quota-Request", out var requestQuota))
                Console.WriteLine($"X-API-Quota-Request: {string.Join(", ", requestQuota)}");
            if (response.Headers.TryGetValues("X-API-Quota-Used", out var usedQuota))
                Console.WriteLine($"X-API-Quota-Used: {string.Join(", ", usedQuota)}");
            if (response.Headers.TryGetValues("X-API-Quota-Left", out var leftQuota))
                Console.WriteLine($"X-API-Quota-Left: {string.Join(", ", leftQuota)}");

            return responseBody;
        }
        catch (HttpException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpException(500, $"HTTP error during GET request: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            throw new HttpException(500, "GET request timed out.");
        }
        catch (Exception ex)
        {
            throw new HttpException(500, $"Unknown error during GET request: {ex.Message}");
        }
    }
    
    public List<RecipeDtoFromApi> ParseRecipes(string jsonArray)
    {
        var recipes = new List<RecipeDtoFromApi>();

        using (JsonDocument doc = JsonDocument.Parse(jsonArray))
        {
            var results = doc.RootElement.GetProperty("results");

            foreach (var element in results.EnumerateArray())
            {
                
                var nutrients = element.GetProperty("nutrition").GetProperty("nutrients");

                var r = new RecipeDtoFromApi
                {
                    Id = element.GetProperty("id").GetInt32(),
                    Image = element.GetProperty("image").GetString(),
                    Title = element.GetProperty("title").GetString(),
                    ReadyInMinutes = element.GetProperty("readyInMinutes").GetInt32(),
                    Servings = element.GetProperty("servings").GetInt32(),
                    SourceUrl = element.GetProperty("sourceUrl").GetString(),
                    Calories = nutrients.EnumerateArray().First(n => n.GetProperty("name").GetString() == "Calories")
                        .GetProperty("amount").GetDouble(),
                    Proteins = nutrients.EnumerateArray().First(n => n.GetProperty("name").GetString() == "Protein")
                        .GetProperty("amount").GetDouble(),
                    Carbs = nutrients.EnumerateArray().First(n => n.GetProperty("name").GetString() == "Carbohydrates")
                        .GetProperty("amount").GetDouble(),
                    Fats = nutrients.EnumerateArray().First(n => n.GetProperty("name").GetString() == "Fat")
                        .GetProperty("amount").GetDouble(),
                    DishTypes = element.GetProperty("dishTypes")
                        .EnumerateArray()
                        .Select(x => x.GetString())
                        .ToList()
                };
                
                recipes.Add(r);
                //Console.WriteLine($"{r.Title} ({r.Id}): {r.Calories} kcal, P:{r.Proteins} C:{r.Carbs} F:{r.Fats}");
            }
        }

        return recipes;
    }
    
}
