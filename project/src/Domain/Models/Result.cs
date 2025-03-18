namespace Domain.Models;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public string? ErrorCode { get; private set; }
    public List<string>? ValidationErrors { get; private set; }
    public T? Data { get; private set; }

    private Result(
        bool isSuccess,
        T? data = default,
        string? message = null,
        string? errorCode = null,
        List<string>? validationErrors = null
    )
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    // Success Methods
    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>(true, data, message);
    }

    public static Result<T> Success(string? message = null)
    {
        return new Result<T>(true, default, message);
    }

    // Failure Methods
    public static Result<T> Failure(string message)
    {
        return new Result<T>(false, default, message);
    }

    public static Result<T> Failure(string message, string errorCode)
    {
        return new Result<T>(false, default, message, errorCode);
    }

    public static Result<T> Failure(string message, List<string> validationErrors)
    {
        return new Result<T>(false, default, message, validationErrors: validationErrors);
    }

    public static Result<T> Failure(string message, string errorCode, List<string> validationErrors)
    {
        return new Result<T>(false, default, message, errorCode, validationErrors);
    }
}
