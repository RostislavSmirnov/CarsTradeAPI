using MediatR;
using FluentValidation;


namespace CarsTradeAPI.Infrastructure.ValidationBehavior
{
    /// <summary>
    /// Поведение конвейера для валидации запросов с использованием FluentValidation
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

            FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            List<FluentValidation.Results.ValidationFailure> failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                List<ErrorDetail> errors = failures
                    .Select(f => ErrorDetail.Validation(f.PropertyName, f.ErrorMessage))
                    .ToList();

                // Если ответ — MbResult<T>, собираем его сразу
                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(MbResult<>))
                {
                    // достаём MbResult<SomeType>.Failure через рефлексию один раз
                    System.Reflection.MethodInfo? failureMethod = typeof(TResponse)
                        .GetMethod(nameof(MbResult<object>.Failure),
                                   new[] { typeof(IEnumerable<ErrorDetail>) });

                    if (failureMethod != null)
                    {
                        return (TResponse)failureMethod.Invoke(null, new object[] { errors })!;
                    }
                }

                // fallback — выбрасываем ValidationException
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
