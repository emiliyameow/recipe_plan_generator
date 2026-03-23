namespace SmartChef.core.server;

public class RouteTable
{
    private readonly Dictionary<(string Path, string Method), RouteEntry> _routes = new();

    public void MapGet(string path, Func<HttpContextExtension, Task> action, Type? modelType = null)
    {
        _routes[(path, "GET")] = new RouteEntry(path, "GET", action, modelType);
    }

    public void MapPost(string path, Func<HttpContextExtension, Task> action, Type? modelType = null)
    {
        _routes[(path, "POST")] = new RouteEntry(path, "POST", action, modelType);
    }

    public void MapPost<TModel>(string path, Func<HttpContextExtension, Task> handler)
    {
        _routes[(path, "POST")] = new RouteEntry(path, "POST", handler, typeof(TModel));
    }
    
    public RouteEntry? Match(string path, string method)
    {
        return _routes.TryGetValue((path, method.ToUpperInvariant()), out var route)
            ? route
            : null;
    }
}

public class RouteEntry(string path, string method, Func<HttpContextExtension, Task> action, Type? modelType = null)
{
    public string Path { get; } = path;
    public string Method { get; } = method;
    public Func<HttpContextExtension, Task> Action { get; } = action;
    public Type? ModelType { get; } = modelType; // <- ожидаемый тип модели (если есть)
}