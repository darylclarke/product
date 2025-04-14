using FluentValidation;

namespace ProductsApi.Filters;

public static class ValidationFilter
{
    public static RouteHandlerBuilder WithValidation<T>(
        this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            // Get the model from arguments
            var model = context.Arguments
                .OfType<T>()
                .FirstOrDefault();
            
            if (model == null)
                return await next(context);
            
            // Get the validator from DI
            var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
            
            if (validator == null)
                return await next(context);
            
            // Validate and handle results
            var validationResult = await validator.ValidateAsync(
                model, 
                context.HttpContext.RequestAborted);
            
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                
                return Results.ValidationProblem(errors);
            }
        
            return await next(context);
        });
    }
}
