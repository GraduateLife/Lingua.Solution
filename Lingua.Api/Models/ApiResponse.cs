namespace Lingua.Api.Models;

/// <summary>
/// Generic API response wrapper.
/// </summary>
/// <typeparam name="T">The type of data in the response.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the response data.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets the error type if the operation failed.
    /// </summary>
    public string? ErrorType { get; init; }

    /// <summary>
    /// Gets an optional message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public static ApiResponse<T> Failure(string error, string? errorType = null) => new()
    {
        Success = false,
        Error = error,
        ErrorType = errorType
    };
}

