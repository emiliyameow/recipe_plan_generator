using SmartChef.core.middleware;

namespace SmartChef.core.server;

public sealed class MiddlewarePipeline
{
    private readonly List<IMiddleware> _middlewares = new();

    public MiddlewarePipeline Use(IMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public async Task ExecuteAsync(HttpContextExtension ctx)
    {
        var index = -1;

        Task Next()
        {
            index++;
            if (index < _middlewares.Count)
            {
                return _middlewares[index].InvokeAsync(ctx, Next);
            }

            return Task.CompletedTask;
        }

        await Next();
    }
}
