using System;
using System.Security.Claims;

namespace Users
{
    /// <summary>
    /// Helper methods for user authentication operations
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Extracts and validates the current user ID from claims
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the current user</param>
        /// <returns>Tuple containing (isValid, userId) where isValid indicates if authentication was successful</returns>
        public static (bool isValid, long userId) GetCurrentUserId(ClaimsPrincipal user)
        {
            if (user == null)
                return (false, 0);

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                return (false, 0);

            return (true, userId);
        }

        /// <summary>
        /// Gets the current user ID or throws if not authenticated
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the current user</param>
        /// <returns>The user ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
        public static long RequireCurrentUserId(ClaimsPrincipal user)
        {
            var (isValid, userId) = GetCurrentUserId(user);
            if (!isValid)
                throw new UnauthorizedAccessException("User not authenticated");

            return userId;
        }

        /// <summary>
        /// Checks if the current user is authenticated and returns the user ID
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the current user</param>
        /// <param name="userId">Output parameter for the user ID</param>
        /// <returns>True if authenticated, false otherwise</returns>
        public static bool TryGetCurrentUserId(ClaimsPrincipal user, out long userId)
        {
            var (isValid, id) = GetCurrentUserId(user);
            userId = id;
            return isValid;
        }

        /// <summary>
        /// Validates that a user owns a specific resource
        /// </summary>
        /// <param name="currentUserId">The current authenticated user's ID</param>
        /// <param name="resourceOwnerId">The owner ID of the resource</param>
        /// <returns>True if the user owns the resource, false otherwise</returns>
        public static bool ValidateResourceOwnership(long currentUserId, long? resourceOwnerId)
        {
            return resourceOwnerId.HasValue && resourceOwnerId.Value == currentUserId;
        }

        /// <summary>
        /// Validates that a user owns a specific resource (non-nullable version)
        /// </summary>
        /// <param name="currentUserId">The current authenticated user's ID</param>
        /// <param name="resourceOwnerId">The owner ID of the resource</param>
        /// <returns>True if the user owns the resource, false otherwise</returns>
        public static bool ValidateResourceOwnership(long currentUserId, long resourceOwnerId)
        {
            return resourceOwnerId == currentUserId;
        }

        /// <summary>
        /// Gets a user-friendly authentication error message
        /// </summary>
        /// <param name="isAjax">Whether this is an AJAX request</param>
        /// <returns>Appropriate error message</returns>
        public static string GetAuthenticationErrorMessage(bool isAjax = false)
        {
            return isAjax ? "User not authenticated" : "You must be logged in to access this page.";
        }

        /// <summary>
        /// Gets a user-friendly authorization error message
        /// </summary>
        /// <param name="isAjax">Whether this is an AJAX request</param>
        /// <returns>Appropriate error message</returns>
        public static string GetAuthorizationErrorMessage(bool isAjax = false)
        {
            return isAjax ? "Access denied" : "You don't have permission to access this resource.";
        }

        /// <summary>
        /// Checks if a request is an AJAX request based on headers
        /// </summary>
        /// <param name="requestHeaders">The request headers</param>
        /// <returns>True if this appears to be an AJAX request</returns>
        public static bool IsAjaxRequest(Microsoft.AspNetCore.Http.IHeaderDictionary requestHeaders)
        {
            return requestHeaders.ContainsKey("X-Requested-With") && 
                   requestHeaders["X-Requested-With"].ToString().Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
                   requestHeaders.ContainsKey("Accept") && 
                   requestHeaders["Accept"].ToString().IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}