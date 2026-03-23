using System.Text.Json;
using System.Text.Json.Serialization;
using SmartChef.core.exceptions;
using SmartChef.core.server;
using SmartChef.mvc.models.dto.request;
using SmartChef.mvc.models.repositories;

namespace SmartChef.mvc.controllers;

public class UsersBodyInfoController
{
    private readonly RedisSessionsRepository _redisSessionsRepository;
    private readonly UsersBodyInfoRepository _usersBodyInfoRepository;
    
    public UsersBodyInfoController(RedisSessionsRepository redisSessionsRepository, UsersBodyInfoRepository usersBodyInfoRepository )
    {
        _redisSessionsRepository = redisSessionsRepository;
        _usersBodyInfoRepository = usersBodyInfoRepository;
    }
    
    public async Task AddBodyInformation(HttpContextExtension ctx, CancellationToken ct = default)
    {
        try
        {
            using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
            var body = await reader.ReadToEndAsync(ct);

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new HttpException(400, "Request body is empty.");
            }

            //  десериализация с поддержкой enum по строкам
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            var info = JsonSerializer.Deserialize<UserBodyInformation>(body, options);

            if (info == null)
            {
                throw new HttpException(400, "Request body is empty.");
            }
            
            /* TODO: Валидация через класс
            // --- Валидация ---
            var errors = new List<string>();
            if (info.Height < 50 || info.Height > 250) errors.Add("Height must be between 50 and 250 cm.");
            if (info.Weight < 20 || info.Weight > 300) errors.Add("Weight must be between 20 and 300 kg.");
            if (info.Age < 10 || info.Age > 120) errors.Add("Age must be between 10 and 120 years.");

            if (errors.Count > 0)
            {
                await ctx.WriteJsonAsync(new { errors }, 400);
                return;
            }

            // --- Привязываем к пользователю ---
            if (ctx.AuthUser == null)
            {
                await ctx.WriteJsonAsync(new { error = "User not authenticated" }, 401);
                return;
            }
            */
            
            //TODO: проверка на то что юзер есть? в других классах?

            info.UserId = ctx.AuthUser!.UserId;

            // --- Сохраняем в БД ---
            await _usersBodyInfoRepository.UpdateBodyInfoUnformationAsync(info, ct);

            await ctx.WriteJsonAsync(new { success = true, message = "Body information saved" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] AddBodyInformation: {ex}");
            throw new HttpException(500, "Internal server error"); 
        }
    }
    
    public async Task GetProfileAsync(HttpContextExtension ctx, CancellationToken ct = default)
    {
        try
        {
            if (ctx.AuthUser == null)
            {
                await ctx.WriteJsonAsync(new { error = "User not authenticated" }, 401);
                return;
            }

            var userId = ctx.AuthUser.UserId;
            var profile = await _usersBodyInfoRepository.GetFullUserProfileAsync(userId, ct);

            if (profile == null)
            {
                throw new HttpException(401, "User not found");
            }

            await ctx.WriteJsonEnumAsync(new
            {
                username = profile.Value.Username,
                email = profile.Value.Email,
                bodyInfo = profile.Value.BodyInfo
            });
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetProfileAsync: {ex}");
            await ctx.WriteJsonAsync(new { error = "Internal server error" }, 500);
        }
    }


}