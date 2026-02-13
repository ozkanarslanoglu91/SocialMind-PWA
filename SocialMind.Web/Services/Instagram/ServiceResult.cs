namespace SocialMind.Web.Services.Instagram;

/// <summary>
/// Generic result wrapper for service operations
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
        ErrorMessage = null,
        ErrorCode = null
    };

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static ServiceResult<T> Fail(string errorMessage, string errorCode) => new()
    {
        Success = false,
        Data = default,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}
