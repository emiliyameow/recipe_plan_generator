using Npgsql;
using SmartChef.mvc.models.dto.request;

namespace SmartChef.mvc.models.repositories;

public class UsersBodyInfoRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UsersBodyInfoRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }
    
    public async Task UpdateBodyInfoUnformationAsync(UserBodyInformation info, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);

        const string sql = @"
            INSERT INTO food_database.user_body_info 
                (user_id, height, weight, age, gender, activity_level, goal)
            VALUES 
                (@user_id, @height, @weight, @age, @gender, @activity_level, @goal)
            ON CONFLICT (user_id) DO UPDATE 
            SET height = EXCLUDED.height,
                weight = EXCLUDED.weight,
                age = EXCLUDED.age,
                gender = EXCLUDED.gender,
                activity_level = EXCLUDED.activity_level,
                goal = EXCLUDED.goal;
        ";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", info.UserId);
        cmd.Parameters.AddWithValue("@height", info.Height);
        cmd.Parameters.AddWithValue("@weight", info.Weight);
        cmd.Parameters.AddWithValue("@age", info.Age);
        cmd.Parameters.AddWithValue("@gender", info.Gender.ToString());
        cmd.Parameters.AddWithValue("@activity_level", info.ActivityLevel.ToString());
        cmd.Parameters.AddWithValue("@goal", info.Goal.ToString());

        await cmd.ExecuteNonQueryAsync(ct);
    }
    
    public async Task<UserBodyInformation?> GetBodyInfoByUserIdAsync(long userId, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);

        const string sql = @"
        SELECT user_id, height, weight, age, gender, activity_level, goal
        FROM food_database.user_body_info
        WHERE user_id = @user_id;
    ";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return new UserBodyInformation
            {
                UserId = reader.GetInt64(reader.GetOrdinal("user_id")),
                Height = reader.GetInt32(reader.GetOrdinal("height")),
                Weight = reader.GetDouble(reader.GetOrdinal("weight")),
                Age = reader.GetInt32(reader.GetOrdinal("age")),
                Gender = Enum.TryParse<Gender>(reader.GetString(reader.GetOrdinal("gender")), out var gender) ? gender : Gender.Male,
                ActivityLevel = Enum.TryParse<ActivityLevel>(reader.GetString(reader.GetOrdinal("activity_level")), out var activity) ? activity : ActivityLevel.Sedentary,
                Goal = Enum.TryParse<Goal>(reader.GetString(reader.GetOrdinal("goal")), out var goal) ? goal : Goal.Maintenance
            };
        }

        return null;
    }
    
    public async Task<(string Username, string Email, UserBodyInformation? BodyInfo)?> GetFullUserProfileAsync(long userId, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);

        const string sql = @"
            SELECT 
                u.username,
                u.email,
                b.user_id,
                b.height,
                b.weight,
                b.age,
                b.gender,
                b.activity_level,
                b.goal
            FROM food_database.users u
            LEFT JOIN food_database.user_body_info b ON u.user_id = b.user_id
            WHERE u.user_id = @user_id;
        ";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            var username = reader.GetString(reader.GetOrdinal("username"));
            var email = reader.GetString(reader.GetOrdinal("email"));

            // Если у пользователя нет записи в user_body_info
            if (reader.IsDBNull(reader.GetOrdinal("user_id")))
            {
                return (username, email, null);
            }

            var bodyInfo = new UserBodyInformation
            {
                UserId = reader.GetInt64(reader.GetOrdinal("user_id")),
                Height = reader.IsDBNull(reader.GetOrdinal("height")) ? 0 : reader.GetInt32(reader.GetOrdinal("height")),
                Weight = reader.IsDBNull(reader.GetOrdinal("weight")) ? 0 : reader.GetDouble(reader.GetOrdinal("weight")),
                Age = reader.IsDBNull(reader.GetOrdinal("age")) ? 0 : reader.GetInt32(reader.GetOrdinal("age")),
                Gender = Enum.TryParse<Gender>(reader.GetString(reader.GetOrdinal("gender")), out var gender) ? gender : Gender.Male,
                ActivityLevel = Enum.TryParse<ActivityLevel>(reader.GetString(reader.GetOrdinal("activity_level")), out var activity) ? activity : ActivityLevel.Sedentary,
                Goal = Enum.TryParse<Goal>(reader.GetString(reader.GetOrdinal("goal")), out var goal) ? goal : Goal.Maintenance
            };

            return (username, email, bodyInfo);
        }

        return null;
    }

    
}