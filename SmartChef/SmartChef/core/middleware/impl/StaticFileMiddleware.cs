using SmartChef.core.server;

namespace SmartChef.core.middleware.impl;

public sealed class StaticFileMiddleware : IMiddleware
{
    private readonly string _root;
    public StaticFileMiddleware(string root)
    {
        _root = root;
    }

    public async Task InvokeAsync(HttpContextExtension ctx, Func<Task> next)
    {
        var request = ctx.Request;
        var response = ctx.Response;

        var path = ctx.VirtualPath ?? request.Url?.LocalPath ?? "/";
        
        var pathSegments = path
            .TrimStart('/')
            .Split("/", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        // проверяем "специальные" маршруты
        string fileName;
        if (pathSegments.Length == 1)
        {
            fileName = pathSegments[0].ToLowerInvariant() switch
            {
                "login" => "login_page.html",
                "signup" => "signup_page.html",
                "user_profile" => "user_profile.html",
                "meal_plan" => "meal_plan.html",
                _ => pathSegments[0]  // обычный файл
            };
        }
        else if (pathSegments.Length == 0)
        {
            fileName = "index.html"; // если путь пустой, отдаём index
        }
        else
        {
            fileName = Path.Combine(pathSegments); // вложенные пути, например css/style.css
        }

        // полный путь до файла
        var filePath = Path.Combine(_root, "mvc", "views", "public", fileName);

        if (File.Exists(filePath))
        {
            await ShowFile(filePath, ctx);
        }
        else
        {
            throw new FileNotFoundException($"File {filePath} not found");
                /*
            // 404 картинка
            var path404 = Path.Combine(_root,"mvc", "views", "public", "img", "404.jpg");
            if (File.Exists(path404))
            {
                await ShowFile(path404, ctx);
            }
            else
            {
                // можно просто вернуть 404
                Console.WriteLine(ctx.Request.Url);
                ctx.Response.StatusCode = 404;
                await ctx.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("404 Not Found"));
                ctx.Response.OutputStream.Close();
            }*/
        }
    }


   
    
    public static async Task ShowFile(string fileName, HttpContextExtension context, CancellationToken ctx = default)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "public", fileName);
        context.Response.StatusCode = 200;
        context.Response.ContentType = Path.GetExtension(path) switch
        {
            ".html" => "text/html; charset=utf-8",
            ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "text/plain; charset=utf-8"
        };
        var file = await File.ReadAllBytesAsync(path, ctx);
        await context.Response.OutputStream.WriteAsync(file, ctx);
        context.Response.OutputStream.Close();
    }
    
}