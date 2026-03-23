using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SmartChef.core.middleware;
using SmartChef.core.middleware.impl;
using SmartChef.core.server;
using SmartChef.core.utils;
using SmartChef.mvc.controllers;
using SmartChef.mvc.models;
using SmartChef.mvc.models.dto;
using SmartChef.mvc.models.dto.request;
using SmartChef.mvc.models.repositories;
using SmartChef.mvc.models.validations;
using SmartChef.services;

namespace SmartChef;


class Program
{
    static async Task Main(string[] args)
    {
        string fileName = "appsettings.json";
        string jsonString = await File.ReadAllTextAsync(fileName);
        var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);
        string? connectionString = settings?.ConnectionString;

        var maxConcurrentRequests = settings?.MaxConcurrentRequests;
        var apiKey = settings?.ApiKey;

        if (settings == null || connectionString is null || apiKey is null || maxConcurrentRequests == null || maxConcurrentRequests <= 0)
        {
            Console.WriteLine("Please provide a connection string, API key and count of max concurrent requests to start application.");
            return;
        }
        // Создаём DataSource (Postgres)
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder.Build();

        // Создаём базу
        var schemaDbRepository = new SchemaDbRepository(dataSource);
        await schemaDbRepository.CreateFoodDatabaseSchema();
        
        // Создаём репозитории
        var usersRepository = new UsersRepository(dataSource);
        var redisSessionsRepository = new RedisSessionsRepository();
        var usersBodyInfoRepository = new UsersBodyInfoRepository(dataSource);
        var planRepository = new PlanRepository(dataSource);
        
        // Создаём контроллеры
        var usersController = new UsersController(usersRepository, redisSessionsRepository);
        var usersBodyInfoController = new UsersBodyInfoController(redisSessionsRepository, usersBodyInfoRepository);
        var planController = new PlanController(new SpoonacularApiClient(apiKey), planRepository);
        
        // Создаём таблицу маршрутов
        var routes = new RouteTable();

        // заполняем таблицу маршрутов
        routes.MapPost<UserRegisterModel>("/signup", ctx => usersController.Register(ctx));

        routes.MapPost("/login", ctx => usersController.Login(ctx));
        routes.MapGet("/user/me", ctx => usersController.GetCurrentUsername(ctx));
        routes.MapPost("/user/quit", ctx => usersController.Quit(ctx));

        
        routes.MapPost("/user/add-body-info", ctx => usersBodyInfoController.AddBodyInformation(ctx));
        routes.MapGet("/user/profile-body-info", ctx => usersBodyInfoController.GetProfileAsync(ctx));
        
        
        routes.MapPost("/api/calculate-calories", ctx => planController.CalculateProperCalories(ctx));
        routes.MapPost<PlanRequestForPlan>("/user/generate-plan", ctx => planController.GeneratePlanAsync(ctx));
        routes.MapPost("/save-plan", ctx => usersController.HandleSaveUserPlan(ctx));
        
        // Middleware
        var errorHandling = new ErrorHandlingMiddleware();
        var routing = new RoutingMiddleware(routes);
        var staticFiles = new StaticFileMiddleware(AppContext.BaseDirectory);
        var auth = new AuthMiddleware();
        var modelBinding = new ModelBindingMiddleware(routes);

        var validators = new Dictionary<Type, object>
        {
            { typeof(UserRegisterModel), new UserValidationRules() },
            { typeof(PlanRequestForPlan), new PlanRequestForPlanValidator() } 
        };

        var validation = new ValidationMiddleware(validators);

        // Собираем пайплайн вручную
        var pipeline = new MiddlewarePipeline()
            .Use(errorHandling)
            .Use(auth)
            .Use(modelBinding)
            .Use(validation)
            .Use(routing)
            .Use(staticFiles);

        // Запускаем сервер
        var server = new HttpServer("http://localhost:5000/", pipeline, (int)maxConcurrentRequests);

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await server.StartAsync(cts.Token);
        
    }
}