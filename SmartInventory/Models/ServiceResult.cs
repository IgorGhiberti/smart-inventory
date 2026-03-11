namespace SmartInventory.Models;

public sealed class ServiceResult<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string? Error { get; init; }
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, int statusCode = StatusCodes.Status200OK) =>
        new() { Success = true, StatusCode = statusCode, Data = data };

    public static ServiceResult<T> Fail(string error, int statusCode) =>
        new() { Success = false, StatusCode = statusCode, Error = error };
}
