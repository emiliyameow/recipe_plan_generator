using Npgsql;
using ServiceStack.Data;
using SmartChef.mvc.models.repositories.sql;

namespace SmartChef.mvc.models.repositories;

public class SchemaDbRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public SchemaDbRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task CreateFoodDatabaseSchema(CancellationToken cancellationToken = default)
    {
        const string sqlQuery = @"
             -- схема
            CREATE SCHEMA IF NOT EXISTS food_database2;

            -- таблица пользователей
            CREATE TABLE IF NOT EXISTS food_database2.users (
                user_id BIGSERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL,
                email VARCHAR(20) NOT NULL,
                login VARCHAR(50) NOT NULL,
                password VARCHAR(20) NOT NULL,
                user_role VARCHAR(20) NOT NULL,
            
                CONSTRAINT uq_users_email UNIQUE (email),
                CONSTRAINT uq_users_login UNIQUE (login)
            );

            -- таблица с рецептами
            CREATE TABLE IF NOT EXISTS food_database2.recipes_from_plans (
                recipe_id BIGINT PRIMARY KEY,
                image TEXT,
                title TEXT,
                ready_in_minutes INT,
                servings INT,
                source_url TEXT,
                calories DOUBLE PRECISION NOT NULL,
                proteins DOUBLE PRECISION NOT NULL,
                carbs DOUBLE PRECISION NOT NULL,
                fats DOUBLE PRECISION NOT NULL
            );

            -- параметры тела пользователя
            CREATE TABLE IF NOT EXISTS food_database2.user_body_info (
                user_id BIGINT PRIMARY KEY,
                height INT CHECK (height BETWEEN 50 AND 250),
                weight NUMERIC(5,2) CHECK (weight BETWEEN 20 AND 300),
                age INT CHECK (age BETWEEN 10 AND 120),
                gender VARCHAR(10) NOT NULL CHECK (gender IN ('Male', 'Female')),
                activity_level VARCHAR(20) NOT NULL CHECK (
                   activity_level IN ('Sedentary', 'Light', 'Moderate', 'High', 'Extreme')
                ),
                goal VARCHAR(20) NOT NULL CHECK (
                   goal IN ('WeightLoss', 'Maintenance', 'WeightGain', 'BodyRecomposition')
                ),
                -- Внешний ключ к таблице пользователей
                CONSTRAINT fk_user_body_info_user
                    FOREIGN KEY (user_id)
                    REFERENCES food_database2.users(user_id)
                    ON DELETE CASCADE
            );

            -- план питания
            CREATE TABLE IF NOT EXISTS food_database2.user_plan (
                user_id BIGINT PRIMARY KEY,
                generate_date DATE,
                recipe_breakfast_id BIGINT,
                recipe_lunch_id BIGINT,
                recipe_dinner_id BIGINT,
                
                -- Столбцы с пищевой ценностью всего плана
                calories_plan DOUBLE PRECISION NOT NULL,
                proteins_plan DOUBLE PRECISION NOT NULL,
                carbs_plan DOUBLE PRECISION NOT NULL,
                fats_plan DOUBLE PRECISION NOT NULL,
                
                -- Внешний ключ к таблице пользователей
                CONSTRAINT fk_user_plan
                    FOREIGN KEY (user_id)
                    REFERENCES food_database.users(user_id)
                    ON DELETE CASCADE,
                
                -- Внешние ключи к таблице рецептов
                CONSTRAINT fk_user_plan_breakfast
                    FOREIGN KEY (recipe_breakfast_id)
                    REFERENCES food_database2.recipes_from_plans(recipe_id)
                    ON DELETE CASCADE,
                CONSTRAINT fk_user_plan_lunch
                    FOREIGN KEY (recipe_lunch_id)
                    REFERENCES food_database2.recipes_from_plans(recipe_id)
                    ON DELETE CASCADE,
                CONSTRAINT fk_user_plan_dinner
                    FOREIGN KEY (recipe_dinner_id)
                    REFERENCES food_database2.recipes_from_plans(recipe_id)
                    ON DELETE CASCADE
            );


        ";

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        
        await using var cmd = new NpgsqlCommand(sqlQuery, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}