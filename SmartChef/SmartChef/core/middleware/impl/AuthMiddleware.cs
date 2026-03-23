using SmartChef.core.server;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.repositories;


namespace SmartChef.core.middleware.impl;

public sealed class AuthMiddleware : IMiddleware
{
    private readonly RedisSessionsRepository _sessions = new();

    public async Task InvokeAsync(HttpContextExtension ctx, Func<Task> next)
    {
        // Получаем sessionId из cookie
        var sessionIdStr = ctx.Request.Cookies?["session-id"]?.Value;

        SessionData? userSession = null;

        if (!string.IsNullOrEmpty(sessionIdStr) && Guid.TryParse(sessionIdStr, out var sessionId))
        {
            userSession = await _sessions.GetSessionAsync(sessionId);

            if (userSession != null)
            {
                // Продлеваем TTL сессии
                await _sessions.RefreshSessionAsync(sessionId);
            }
        }

        // Сохраняем в HttpContext
        ctx.AuthUser = userSession;  
        ctx.IsAuthenticated = userSession != null;
        
        await next();
    }
}
