using System;
using System.Collections.Generic;

namespace Common;

public static class ApiResponseUtilities
{
    /// <summary>
    /// Creates a standard error response object
    /// </summary>
    public static object CreateErrorResponse(int code, string message, object detail = null)
    {
        var response = new
        {
            errors = new[] { new { code, message } }
        };

        if (detail != null)
        {
            var responseDict = new Dictionary<string, object>
            {
                ["errors"] = response.errors,
                ["detail"] = detail
            };
            return responseDict;
        }

        return response;
    }

    /// <summary>
    /// Creates a standard error response object with multiple errors
    /// </summary>
    public static object CreateErrorResponse(params (int code, string message)[] errors)
    {
        var response = new
        {
            errors = errors
        };
        return response;
    }

    /// <summary>
    /// Creates a standard success response object
    /// </summary>
    public static object CreateSuccessResponse(object data)
    {
        return data;
    }

    /// <summary>
    /// Creates a standard not found response object
    /// </summary>
    public static object CreateNotFoundResponse(string message = "Resource not found")
    {
        var response = new
        {
            errors = new[] { new { code = 404, message } }
        };
        return response;
    }

    /// <summary>
    /// Creates a standard forbidden response object
    /// </summary>
    public static object CreateForbiddenResponse(string message = "Access denied")
    {
        var response = new
        {
            errors = new[] { new { code = 403, message } }
        };
        return response;
    }

    /// <summary>
    /// Creates a standard unauthorized response object
    /// </summary>
    public static object CreateUnauthorizedResponse(string message = "Authentication required")
    {
        var response = new
        {
            errors = new[] { new { code = 401, message } }
        };
        return response;
    }
}
