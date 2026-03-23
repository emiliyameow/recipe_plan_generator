using System.ComponentModel.DataAnnotations;
using SmartChef.core.server;
using ValidationResult = FluentValidation.Results.ValidationResult;


namespace SmartChef.core.middleware.impl;

public sealed class ValidationMiddleware : IMiddleware
{
    private readonly Dictionary<Type, object> _validators;

    public ValidationMiddleware(Dictionary<Type, object> validators)
    {
        _validators = validators;
    }

    public async Task InvokeAsync(HttpContextExtension ctx, Func<Task> next)
    {
        var model = ctx.BoundModel;
        if (model is null)
        {
            await next();
            return;
        }

        var modelType = model.GetType();
        if (!_validators.TryGetValue(modelType, out var validatorObj))
        {
            await next();
            return;
        }

        var validatorType = typeof(FluentValidation.IValidator<>).MakeGenericType(modelType);
        var method = validatorType.GetMethod("ValidateAsync", new[] { modelType, typeof(CancellationToken) })
                     ?? validatorType.GetMethod("Validate", new[] { modelType });

        if (method is null)
        {
            await next();
            return;
        }

        object? validationResultObj;
        if (method.Name == "ValidateAsync")
        {
            validationResultObj = await (dynamic)method.Invoke(validatorObj, new object[] { model, CancellationToken.None })!;
        }
        else
        {
            validationResultObj = method.Invoke(validatorObj, new object[] { model });
        }

        if (validationResultObj is ValidationResult result && !result.IsValid)
        {
            ctx.Response.StatusCode = 400;
            await ctx.WriteJsonAsync(new
            {
                error = "Validation errors: " + string.Join(", ", result.Errors.Select(e => e.ErrorMessage))
            }, 400);
            //ctx.Response.OutputStream.Close();
            return;
        }

        await next();
    }
}
