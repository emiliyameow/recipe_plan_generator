using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartChef.mvc.models.dto;

namespace SmartChef.core.server;

public sealed class HttpContextExtension(HttpListenerContext raw)
{
    private HttpListenerContext Raw { get; } = raw;

    public HttpListenerRequest Request => Raw.Request;
    public HttpListenerResponse Response => Raw.Response;
    
    // public string? GuestToken { get; set; } для хранения данных неавторизованного пользователя
    public SessionData? AuthUser { get; set; }
    public bool IsAuthenticated { get; set; }
    
    // Для middleware, чтобы подменять путь
    public string? VirtualPath { get; set; }
    
    public object? BoundModel { get; set; } // привязанная модель
    public Dictionary<string, object> Items { get; } = new();

    public async Task WriteJsonAsync(object value, int status = 200)
    {
        Response.StatusCode = status;
        Response.ContentType = "application/json; charset=utf-8";
        await using var sw = new StreamWriter(Response.OutputStream);
        Console.WriteLine(JsonSerializer.Serialize(value));
        await sw.WriteAsync(JsonSerializer.Serialize(value));
    }
    
    public async Task WriteJsonEnumInCamelCaseAsync(object value, int status = 200)
    {
        Response.StatusCode = status;
        Response.ContentType = "application/json; charset=utf-8";
    
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
            Converters = { new JsonStringEnumConverter() }
        };

        await using var sw = new StreamWriter(Response.OutputStream);
        await sw.WriteAsync(JsonSerializer.Serialize(value, options));
    }
    
    public async Task WriteJsonEnumAsync(object value, int status = 200)
    {
        Response.StatusCode = status;
        Response.ContentType = "application/json; charset=utf-8";
    
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        await using var sw = new StreamWriter(Response.OutputStream);
        await sw.WriteAsync(JsonSerializer.Serialize(value, options));
    }

}