using System.Text;
using FluentValidation;
using ServiceStack.Redis;
using SmartChef.core.exceptions;
using SmartChef.core.server;

namespace SmartChef.core.middleware.impl;

public class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContextExtension ctx, Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (HttpException ex)
        {
            
            await ctx.WriteJsonAsync(new { error = ex.Message }, ex.StatusCode);
            //await ctx.WriteJsonAsync(ex, ex.StatusCode);
        }
        
        catch (FileNotFoundException e)
        {
            await ctx.WriteJsonAsync(new { error = "File not found", details = e.Message }, 404);
        }
        catch (RedisException re)
        {
            await ctx.WriteJsonAsync(new { error = "Redis", details = re.Message }, 400);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ctx.Request.Url + "          " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            await ctx.WriteJsonAsync(new { error = "InternalServerError", trace = ex.Message }, 500);
        }
    }
}
