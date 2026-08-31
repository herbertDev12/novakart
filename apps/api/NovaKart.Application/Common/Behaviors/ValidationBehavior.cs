using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace NovaKart.Application.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private static readonly Func<Error, TResponse>? FailureFactory = TryBuildFailureFactory();

    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = await ValidateAsync(message, cancellationToken);

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        if (FailureFactory is null)
        {
            throw new InvalidOperationException(
                $"{typeof(TMessage).Name} failed validation but its response type " +
                $"{typeof(TResponse).Name} is neither Result nor Result<T>. Every handler must " +
                "return Result or Result<T> so validation failures can short-circuit the pipeline.");
        }

        var errorsByProperty = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        return FailureFactory(Error.Validation(errorsByProperty));
    }

    private async ValueTask<List<ValidationFailure>> ValidateAsync(
        TMessage message,
        CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();

        if (!_validators.Any())
        {
            return failures;
        }

        var context = new ValidationContext<TMessage>(message);

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);

            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        return failures;
    }

    private static Func<Error, TResponse>? TryBuildFailureFactory()
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return static error => (TResponse)(object)Result.Failure(error);
        }

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return null;
        }

        var failureMethod = responseType.GetMethod(
            nameof(Result.Failure),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
            binder: null,
            types: [typeof(Error)],
            modifiers: null);

        return failureMethod is null
            ? null
            : failureMethod.CreateDelegate<Func<Error, TResponse>>();
    }
}
