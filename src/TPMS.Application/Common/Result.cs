namespace TPMS.Application.Common;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public Error? Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string code, string message, object? metadata = null) =>
        new(false, default, new Error(code, message, metadata));

    public static Result<T> NotFound(string message) => Failure("not_found", message);

    public static Result<T> Conflict(string message, object? metadata = null) => Failure("conflict", message, metadata);

    public static Result<T> Validation(string message, object? metadata = null) => Failure("validation", message, metadata);
}
