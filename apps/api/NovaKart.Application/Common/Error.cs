namespace NovaKart.Application.Common;

public record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new("Validation.Failed", "One or more validation errors occurred.", ErrorType.Validation, errors);
}

