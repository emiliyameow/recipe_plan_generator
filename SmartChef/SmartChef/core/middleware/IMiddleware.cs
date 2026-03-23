using SmartChef.core.server;

namespace SmartChef.core.middleware;

public interface IMiddleware
{
    Task InvokeAsync(HttpContextExtension ctx, Func<Task> next); 
    // функция которая будет вызываться для обработки нашего контекста в конкретном миддлваре
}