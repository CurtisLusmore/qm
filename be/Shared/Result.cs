using System.Diagnostics.CodeAnalysis;

namespace be.Shared;

public class Result<T>
{
    private Result(bool isSuccess, int statusCode, T? value = default, string? error = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Value = value;
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value, int statusCode = 200) => new(true, statusCode, value);
    public static Result<T> Failure(string error, int statusCode = 500) => new(false, statusCode, default, error);
}
