namespace Lingua.Api.Helpers;

/// <summary>
/// Helper class for validating URLs.
/// </summary>
public static class UrlValidator
{
    /// <summary>
    /// Validates a URL string.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return ValidationResult.Failure("URL parameter is required");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return ValidationResult.Failure("Invalid URL format");
        }

        // Validate that URL uses HTTP or HTTPS protocol
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return ValidationResult.Failure("URL must use HTTP or HTTPS protocol");
        }

        return ValidationResult.Success(url);
    }
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the validated value if validation was successful.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success(string value) => new()
    {
        IsValid = true,
        Value = value
    };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static ValidationResult Failure(string errorMessage) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };
}

