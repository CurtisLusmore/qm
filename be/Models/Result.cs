using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace be.Models;

public class Result
{
    internal Result(
        bool isSuccess,
        HttpStatusCode statusCode,
        string? error = null)
    {
        IsSuccess = isSuccess;
        StatusCode = (int)statusCode;
        Error = error;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public string? Error { get; }

    public static Result Success(HttpStatusCode statusCode = HttpStatusCode.OK) => new(true, statusCode);
    public static Result<T> Success<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK) => new(true, statusCode, value);
    public static Result Failure(string error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) => new(false, statusCode, error);
    public static Result<T> Failure<T>(string error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) => new(false, statusCode, default, error);
}

public class Result<T> : Result
{
    internal Result(
        bool isSuccess,
        HttpStatusCode statusCode, T?
        value = default,
        string? error = null)
        : base(isSuccess, statusCode, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value, HttpStatusCode statusCode = HttpStatusCode.OK) => new(true, statusCode, value);
    public static new Result<T> Failure(string error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) => new(false, statusCode, default, error);
}
