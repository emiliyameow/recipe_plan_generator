using System.Net;
using SmartChef.core.middleware;

namespace SmartChef.core.server;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly MiddlewarePipeline _middlewarePipeline;


    public HttpServer(string prefix, MiddlewarePipeline pipeline, int maxConcurrentRequests = 100)
    {
        _listener.Prefixes.Add(prefix);
        _middlewarePipeline = pipeline;
        _semaphore = new SemaphoreSlim(maxConcurrentRequests);
    }

    public async Task StartAsync(CancellationToken token)
    {
        _listener.Start();
        Console.WriteLine($"Server started at {string.Join(", ", _listener.Prefixes)}meal_plan");

        while (_listener.IsListening && !token.IsCancellationRequested)
        {
            try
            {
                var ctx = await _listener.GetContextAsync();
                
                _ = Task.Run(async () =>
                {
                    await _semaphore.WaitAsync(token);
                    try
                    {
                        await _middlewarePipeline.ExecuteAsync(new HttpContextExtension(ctx));
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, token);
            }
            catch (HttpListenerException)
            {
                // бывает при Stop — игнорируем
                break;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"[SERVER ERROR] {ex}");
            }
        }

        _listener.Stop();
        _listener.Close();
    }
}