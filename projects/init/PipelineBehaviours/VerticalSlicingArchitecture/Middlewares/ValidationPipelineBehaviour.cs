using FluentValidation;
using FluentValidation.Results;
using MediatR;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Middlewares
{
    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(r => !r.IsValid)
                    .SelectMany(r => r.Errors)
                    .ToList();

                if (failures.Any())
                {
                    var failureResponse = CreateFailureResponse<TResponse>(failures);
                    if (failureResponse is not null)
                    {
                        return failureResponse;
                    }

                    throw new ValidationException(failures);
                }
            }

            return await next();
        }

        private static TResponse? CreateFailureResponse<TResponse>(List<ValidationFailure> failures)
        {
            // Handle generic Result<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var errorName = typeof(TRequest).DeclaringType?.Name ?? typeof(TRequest).Name;
                var error = new Error(
                    GetErrorName<TRequest>() + ".Validation",
                    string.Join("; ", failures.Select(f => f.ErrorMessage)));

                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result<>)
                    .MakeGenericType(resultType)
                    .GetMethod(nameof(Result<object>.Failure));

                return (TResponse)failureMethod!.Invoke(null, new object[] { error })!;
            }

            // Handle Result without generic
            if (typeof(TResponse) == typeof(Result))
            {
                var error = new Error(
                    GetErrorName<TRequest>() + ".Validation",
                    string.Join("; ", failures.Select(f => f.ErrorMessage)));

                return (TResponse)(object)Result.Failure(error);
            }

            return default;
        }

        private static string GetErrorName<TRequest>()
        {
            return typeof(TRequest).DeclaringType?.Name ?? typeof(TRequest).Name;
        }
    }
}
