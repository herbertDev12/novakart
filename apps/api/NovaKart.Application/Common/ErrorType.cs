namespace NovaKart.Application.Common;

public enum ErrorType
{
    Failure = 500,
    Validation = 400,
    NotFound = 404,
    Conflict = 409,
    Unauthorized = 401,
    Forbidden = 403,
}