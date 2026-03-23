using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartChef.core.utils;

public static class RequestExtensions
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static async Task<T?> ReadJsonAsync<T>(this HttpListenerRequest req, CancellationToken ct, JsonSerializerOptions? jsonOpts = null)
    {
        jsonOpts ??= JsonOpts;
        using var sr = new StreamReader(req.InputStream, req.ContentEncoding);
        var body = await sr.ReadToEndAsync(ct);
        return string.IsNullOrWhiteSpace(body) 
            ? default 
            : JsonSerializer.Deserialize<T>(body, JsonOpts);
    }
    
    

    public static async Task WriteJsonAsync(this HttpListenerResponse res, object obj, int status = 200, CancellationToken? ct = null)
    {
        var json = JsonSerializer.Serialize(obj, JsonOpts);
        var buf = Encoding.UTF8.GetBytes(json);
        res.StatusCode = status;
        res.ContentType = "application/json; charset=utf-8";
        await res.OutputStream.WriteAsync(buf, ct ?? CancellationToken.None);
        res.Close();
    }
    
}