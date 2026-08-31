namespace NovaKart.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("A success result cannot have an error.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("A failure result must have an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result Failure(ErrorType errorType, string code, string message) =>
        Failure(new Error(code, message, errorType));

    public T Match<T>(Func<T> onSuccess, Func<Error, T> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error);
    }

    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess();
        }
        else
        {
            onFailure(Error);
        }
    }

    public static implicit operator Result(Error error) => Failure(error);

    public override string ToString()
    {
        return IsSuccess ? "Success" : $"Failure: {Error.Code} - {Error.Message}";
    }
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access the value of a failed result. Error: {Error.Code} - {Error.Message}");

    protected Result(bool isSuccess, T? value, Error error) : base(isSuccess, error)
    {
        if (isSuccess && value is null)
        {
            throw new ArgumentException("A success result must have a value.", nameof(value));
        }

        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, Error.None);

    public new static Result<T> Failure(Error error) => new(false, default, error);

    public new static Result<T> Failure(ErrorType errorType, string code, string message) =>
        Failure(new Error(code, message, errorType));

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    public void Match(Action<T> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value);
        }
        else
        {
            onFailure(Error);
        }
    }

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);

    public static Result<T> FromResult(Result result)
    {
        return result.IsSuccess
            ? throw new InvalidOperationException("Cannot convert a successful non-generic Result to Result<T> without a value.")
            : Failure(result.Error);
    }

    public override string ToString()
    {
        return IsSuccess ? $"Success: {Value}" : $"Failure: {Error.Code} - {Error.Message}";
    }
}