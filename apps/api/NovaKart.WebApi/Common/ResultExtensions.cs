using NovaKart.Application.Common;

namespace NovaKart.WebApi.Common;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => Results.ValidationProblem(
                errors: error.ValidationErrors ?? new Dictionary<string, string[]>(),
                title: error.Message,
                detail: "One or more validation errors occurred",
                statusCode: StatusCodes.Status400BadRequest),

            ErrorType.NotFound => Results.Problem(
                title: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                detail: "The requested resource was not found",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"),

            ErrorType.Conflict => Results.Problem(
                title: error.Message,
                statusCode: StatusCodes.Status409Conflict,
                detail: "The request conflicts with the current state of the resource",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.8"),

            ErrorType.Unauthorized => Results.Problem(
                title: error.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                detail: "Authentication is required to access this resource",
                type: "https://tools.ietf.org/html/rfc7235#section-3.1"),

            ErrorType.Forbidden => Results.Problem(
                title: error.Message,
                statusCode: StatusCodes.Status403Forbidden,
                detail: "You do not have permission to access this resource",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.3"),

            _ => Results.Problem(
                title: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                detail: "The request could not be processed",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1")
        };
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.Match(
            onSuccess: value => Results.Ok(value),
            onFailure: error => error.ToProblemDetails());
    }
}