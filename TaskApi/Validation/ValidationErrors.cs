namespace TaskApi.Validation;

public record ErrorDetail(string Field, string Message);

public record ErrorBody(string Code, string Message, List<ErrorDetail>? Details = null);

public record ErrorResponse(ErrorBody Error);

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "TASK_NOT_FOUND";
    public const string InternalError = "INTERNAL_ERROR";
}