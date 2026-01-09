using System;

namespace Common;

public static class ValidationUtilities
{
    /// <summary>
    /// Validates that a string is not null or whitespace
    /// </summary>
    public static void RequireNotNullOrWhiteSpace(string value, string parameterName, string customMessage = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            var message = customMessage ?? $"{parameterName} is required";
            throw new ArgumentException(message, parameterName);
        }
    }

    /// <summary>
    /// Validates that a value is greater than zero
    /// </summary>
    public static void RequireGreaterThanZero(long value, string parameterName, string customMessage = null)
    {
        if (value <= 0)
        {
            var message = customMessage ?? $"{parameterName} must be greater than 0";
            throw new ArgumentOutOfRangeException(parameterName, message);
        }
    }

    /// <summary>
    /// Validates that a value is within the specified range
    /// </summary>
    public static void RequireInRange(int value, int min, int max, string parameterName, string customMessage = null)
    {
        if (value < min || value > max)
        {
            var message = customMessage ?? $"{parameterName} must be between {min} and {max}";
            throw new ArgumentOutOfRangeException(parameterName, message);
        }
    }

    /// <summary>
    /// Validates that an object is not null
    /// </summary>
    public static void RequireNotNull(object value, string parameterName, string customMessage = null)
    {
        if (value == null)
        {
            var message = customMessage ?? $"{parameterName} is required";
            throw new ArgumentNullException(parameterName, message);
        }
    }

    /// <summary>
    /// Validates that a condition is true
    /// </summary>
    public static void RequireTrue(bool condition, string parameterName, string errorMessage)
    {
        if (!condition)
        {
            throw new ArgumentException(errorMessage, parameterName);
        }
    }

    /// <summary>
    /// Validates that a condition is false
    /// </summary>
    public static void RequireFalse(bool condition, string parameterName, string errorMessage)
    {
        if (condition)
        {
            throw new ArgumentException(errorMessage, parameterName);
        }
    }

    /// <summary>
    /// Validates that a string length is within bounds
    /// </summary>
    public static void RequireStringLength(string value, int minLength, int maxLength, string parameterName, string customMessage = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return; // Let RequireNotNullOrWhiteSpace handle null/empty cases

        if (value.Length < minLength || value.Length > maxLength)
        {
            var message = customMessage ?? $"{parameterName} must be between {minLength} and {maxLength} characters";
            throw new ArgumentException(message, parameterName);
        }
    }

    /// <summary>
    /// Validates that a value is not negative
    /// </summary>
    public static void RequireNonNegative(long value, string parameterName, string customMessage = null)
    {
        if (value < 0)
        {
            var message = customMessage ?? $"{parameterName} cannot be negative";
            throw new ArgumentOutOfRangeException(parameterName, message);
        }
    }
}
