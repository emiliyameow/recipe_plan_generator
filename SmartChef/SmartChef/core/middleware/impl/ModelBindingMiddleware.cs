using System.Text.Json;
using SmartChef.core.server;

namespace SmartChef.core.middleware.impl;

public sealed class ModelBindingMiddleware : IMiddleware
{
    private readonly RouteTable _routes;

    public ModelBindingMiddleware(RouteTable routes)
    {
        _routes = routes;
    }

    public async Task InvokeAsync(HttpContextExtension ctx, Func<Task> next)
    {
        // Получаем маршрут
        var route = _routes.Match(ctx.Request.Url!.AbsolutePath, ctx.Request.HttpMethod);
        if (route is null)
        {
            await next();
            return;
        }

        // Получаем тип модели, если контроллер объявил её (через метаданные)
        var modelType = route.ModelType; //  свойство в RouteEntry

        if (modelType is not null &&
            ctx.Request.HasEntityBody &&
            ctx.Request.ContentType?.Contains("application/json") == true)
        {
            using var reader = new StreamReader(ctx.Request.InputStream);
            var body = await reader.ReadToEndAsync();

            var model = JsonSerializer.Deserialize(body, modelType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ctx.BoundModel = model; // сохраняем для валидации
        }

        await next();
    }
}
