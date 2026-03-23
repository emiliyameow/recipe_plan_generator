
using StackExchange.Redis;
using System.Text.Json;
using SmartChef.mvc.models.dto;

namespace SmartChef.mvc.models.repositories;

    /*
     *
     * | Действие          | Команда                     |
       | ----------------- | --------------------------- |
       | Запуск вручную    | `redis-server`              |
       | Запуск как сервис | `brew services start redis` |
       | Проверить работу  | `redis-cli ping`            |
       | Остановить        | `brew services stop redis`  |
       
     */
public class RedisSessionsRepository
{
    private static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect("localhost:6379");
    private readonly IDatabase _db;
    private readonly TimeSpan _sessionLifetime = TimeSpan.FromMinutes(300);

    public RedisSessionsRepository()
    {
        _db = _redis.GetDatabase();
    }

    // Создать сессию
    public async Task<Guid> AddSessionAsync(string username, long userId, string email,string role = "user")
    {
        var sessionId = Guid.NewGuid();
        var session = new SessionData
        {
            Username = username,
            UserId = userId,
            Email = email
        };

        string json = JsonSerializer.Serialize(session);
        
        //await _db.StringSetAsync(sessionId.ToString(), json, new Ra);

        await _db.StringSetAsync(sessionId.ToString(), json, _sessionLifetime);

        return sessionId;
    }

    // Получить сессию
    public async Task<SessionData?> GetSessionAsync(Guid sessionId)
    {
        var json = await _db.StringGetAsync(sessionId.ToString());
        if (json.IsNullOrEmpty)
        {
            return null;
        }
        return JsonSerializer.Deserialize<SessionData>(json!);
    }

    public async Task<String?> GetUsernameAsync(Guid sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        return session?.Username;
    }

    // Продлить TTL сессии
    public async Task RefreshSessionAsync(Guid sessionId)
    {
        await _db.KeyExpireAsync(sessionId.ToString(), _sessionLifetime);
    }

    // Удалить сессию
    public async Task DeleteSessionAsync(Guid sessionId)
    {
        await _db.KeyDeleteAsync(sessionId.ToString());
    }
}
