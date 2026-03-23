using SmartChef.core.server;

namespace SmartChef.core.middleware.impl;

public class RoutingMiddleware : IMiddleware
{
    private readonly RouteTable _routes;

    public RoutingMiddleware(RouteTable routes)
    {
        _routes = routes;
    }

    public async Task InvokeAsync(HttpContextExtension ctx, Func<Task> next)
    {
        var route = _routes.Match(ctx.Request.Url!.AbsolutePath, ctx.Request.HttpMethod);
        if (route is null)
        {
            await next();
            return;
        }

        await route.Action(ctx);
    }
}
