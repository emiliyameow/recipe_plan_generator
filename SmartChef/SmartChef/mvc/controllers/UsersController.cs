using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentValidation;
using SmartChef.core.exceptions;
using SmartChef.core.server;
using SmartChef.core.utils;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.dto.request;
using SmartChef.mvc.models.repositories;

namespace SmartChef.mvc.controllers;

public class UsersController
{
    private readonly UsersRepository _users;
    private readonly RedisSessionsRepository _sessions;

    public UsersController(UsersRepository users, RedisSessionsRepository sessions)
    {
        _users = users;
        _sessions = sessions;
    }
    
    
    public async Task Register(HttpContextExtension ctx, CancellationToken ct = default)
    {
        var user = (UserRegisterModel)ctx.BoundModel!;
        
        if (await _users.UserExistsAsync(user, ct))
        {
            throw new HttpException(400, "User with this email/login or username already exists.");
        }

        var id = await _users.CreateUserAsync(user, ct);
        var sessionId = await _sessions.AddSessionAsync(user.Username, id, user.Email);

        ctx.Response.Cookies.Add(new Cookie("session-id", sessionId.ToString()));
        
        await ctx.WriteJsonAsync(new
        {
            message = "User registered successfully",
            userId = id,
            username = user.Username
        }, 201);
    }

    public async Task Login(HttpContextExtension ctx, CancellationToken ct = default)
    {
        var loginModel = await ctx.Request.ReadJsonAsync<UserLoginModel>
        ( 
            ct,
            new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true }
        );

        if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Login) || string.IsNullOrWhiteSpace(loginModel.Password))
        {
            throw new ValidationException("Invalid login data");
        }
        
        var user = await _users.GetUserByLoginAsync(loginModel, ct);
        
        var sessionId = await _sessions.AddSessionAsync(user.Username, user.UserId, user.Email, user.Role);

        
        ctx.Response.Cookies.Add(new Cookie("session-id", sessionId.ToString()));
        
        await ctx.WriteJsonAsync(new
        {
            message = "Login successful",
            username = user.Username,
            role = user.Role
        });
    }

    public async Task GetCurrentUsername(HttpContextExtension ctx, CancellationToken ct = default)
    {
        var cookie = ctx.Request.Cookies["session-id"];
        if (cookie == null)
        {
            throw new HttpException(401, "User is unauthorized");
        }

        var sessionId = Guid.Parse(cookie.Value);
        var username = await _sessions.GetUsernameAsync(sessionId);

        if (username == null)
        {
            throw new HttpException(401, "User is unauthorized");
        }

        await ctx.Response.WriteJsonAsync(username);
    }

    public async Task Quit(HttpContextExtension ctx, CancellationToken ct = default)
    {
        var cookie = ctx.Request.Cookies["session-id"];
        if (cookie == null)
        {
            await ctx.WriteJsonAsync(new { message = "no_active_session" });
            return;
        }

        var sessionId = Guid.Parse(cookie.Value);
        await _sessions.DeleteSessionAsync(sessionId);

        var expired = new Cookie("session-id", "")
        {
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/"
        };

        
        ctx.Response.Cookies.Add(expired);
        
        ctx.AuthUser = null;
        ctx.IsAuthenticated = false;
        await ctx.WriteJsonAsync(new { message = "logout_success" });
    }
    
    // количество генераций
    // username 
    
    
    
    
    public async Task HandleSaveUserPlan(HttpContextExtension ctx, CancellationToken ct = default)
    {
        
        using var reader = new StreamReader(ctx.Request.InputStream);
        var body = await reader.ReadToEndAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var jsonDoc = JsonDocument.Parse(body);
        var tuple = (
            breakfastId: jsonDoc.RootElement.GetProperty("BreakfastId").GetInt64(),
            lunchId: jsonDoc.RootElement.GetProperty("LunchId").GetInt64(),
            dinnerId: jsonDoc.RootElement.GetProperty("DinnerId").GetInt64()
        );
        
        var idUser = await _users.SaveUserPlanByUserIdAsync(ctx.AuthUser!.UserId, tuple.breakfastId, tuple.lunchId, tuple.dinnerId);
        
        await ctx.WriteJsonAsync(idUser);
    }
    
}

