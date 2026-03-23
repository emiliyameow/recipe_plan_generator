using System.ComponentModel.DataAnnotations;
using Npgsql;
using SmartChef.core.exceptions;
using SmartChef.core.utils;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.repositories.sql;


namespace SmartChef.mvc.models.repositories;

public class UsersRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UsersRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <summary>
    /// Создает нового пользователя и возвращает его ID.
    /// </summary>
    
    public async Task<long> CreateUserAsync(UserRegisterModel user, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        
        await using var cmd = new NpgsqlCommand(SqlUsers.InsertUserWithUserRole, conn);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@password", MyPasswordHasher.Hash(user.Password));

        var userIdObj = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(userIdObj);
    }

    public async Task<bool> UserExistsAsync(UserRegisterModel user, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        // проверка, существует ли пользователь
        await using (var checkCmd = new NpgsqlCommand(SqlUsers.GetUserByLoginOrMail, conn))
        {
            checkCmd.Parameters.AddWithValue("@login", user.Login);
            checkCmd.Parameters.AddWithValue("@email", user.Email);
            checkCmd.Parameters.AddWithValue("@username",user.Username);
            var existsObj = await checkCmd.ExecuteScalarAsync(cancellationToken);
            if (existsObj != null)
            {
                return true;
            }
        }
        return false;
    }

    public async Task<UserDto> GetUserByLoginAsync(UserLoginModel loginModel, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            //await conn.OpenAsync(cancellationToken);

        const string sql = SqlUsers.GetUserByLogin; // метод, который получает юзера по логину 
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@login", loginModel.Login);
        string codedPassword = MyPasswordHasher.Hash(loginModel.Password);
        cmd.Parameters.AddWithValue("@password", codedPassword);

        if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Login) || string.IsNullOrWhiteSpace(loginModel.Password))
        {
            throw new ValidationException("Invalid login data");
        }
        
        await using var dbReader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (dbReader.HasRows && await dbReader.ReadAsync(cancellationToken))
        {
            long userId = dbReader.GetInt64(0);
            var username = dbReader.GetString(1);
            var email = dbReader.GetString(2);
            var login = dbReader.GetString(3);
            var password = dbReader.GetString(4);
            var role = dbReader.GetString(5);
            
            if (MyPasswordHasher.Validate(password, loginModel.Password))
            {
                var userDto = new  UserDto
                {
                    UserId = userId,
                    Username = username,
                    Email = email,
                    Login = login,
                    Role = role,
                    Password = password
                };

                return userDto;
            }
            throw new HttpException(401, "Invalid login or password");
        }
        
        throw new HttpException(401, "Invalid login or password");
    }
    
    public async Task<(DateTime? recordDate, long? breakfastId, long? lunchId, long? dinnerId)>
        GetUserPlanByUserIdAsync(long userId,
            CancellationToken cancellationToken = default)
    {
        
        
        const string query = @"
            SELECT generate_date, breakfast_id, lunch_id, dinner_id
            FROM food_database.users
            WHERE user_id = @userId;
        ";
        
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        
        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            DateTime? recordDate = reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0);
            long? breakfastId = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1);
            long? lunchId = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2);
            long? dinnerId = reader.IsDBNull(3) ? (long?)null : reader.GetInt64(3);
            
            return (recordDate, breakfastId, lunchId, dinnerId);
        }

        // если пользователя нет — вернуть все null
        return (null, null, null, null);
    }
    
    public async Task<long>
        SaveUserPlanByUserIdAsync(long userId,
            long breakfastId, long lunchId, long dinnerId,
            CancellationToken cancellationToken = default)
    {
        /*
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(SqlUsers.InsertUserWithUserRole, conn);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@password", MyPasswordHasher.Hash(user.Password));

        var userIdObj = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(userIdObj);*/
        
        
        DateTime recordDate = DateTime.Now;
       
        string query = @"
                INSERT INTO food_database.users
                    (user_id, record_date, breakfast_id, lunch_id, dinner_id)
                VALUES
                    (@user_id, @record_date, @breakfast_id, @lunch_id, @dinner_id)
                ON CONFLICT (user_id)
                DO UPDATE SET
                    record_date = EXCLUDED.record_date,
                    breakfast_id = EXCLUDED.breakfast_id
                    lunch_id = EXCLUDED.lunch_id,
                    dinner_id = EXCLUDED.dinner_id;
            ";

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("record_date", recordDate);
        cmd.Parameters.AddWithValue("breakfast_id", breakfastId);
        cmd.Parameters.AddWithValue("lunch_id", lunchId);
        cmd.Parameters.AddWithValue("dinner_id", dinnerId);

        var userIdObj = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(userIdObj);
    }

}

/*
             * DateTime recordDate = reader.GetDateTime(2);  // дата из PostgreSQL
               DateTime today = DateTime.Today;              // сегодняшняя дата (время = 00:00:00)

               if (recordDate == today)
               {
                   Console.WriteLine("Дата совпадает с сегодняшней!");
               }
               else if (recordDate < today)
               {
                   Console.WriteLine("Эта дата уже прошла.");
               }
               else
               {
                   Console.WriteLine("Эта дата в будущем.");
               }
             */