using Npgsql;
using Assets;

namespace Thumbnails;

/// <summary>
/// Helper class for place validation operations
/// </summary>
public static class PlaceValidationHelper
{
    /// <summary>
    /// Validates that an asset is a place and the user owns it
    /// </summary>
    /// <param name="placeId">The place ID to validate</param>
    /// <param name="currentUserId">The current user ID</param>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="assetRepository">Asset metadata repository</param>
    /// <returns>True if valid place and user owns it, false otherwise</returns>
    public static async Task<bool> ValidatePlaceOwnershipAsync(long placeId, long currentUserId, string connectionString, AssetMetadataRepository assetRepository)
    {
        var placeAsset = await assetRepository.GetAssetByIdAsync(connectionString, placeId);
        
        if (placeAsset == null)
        {
            return false;
        }

        // Check if asset is actually a place by querying the database directly
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        using var checkCmd = new NpgsqlCommand("SELECT is_place FROM assets WHERE asset_id = @assetId", conn);
        checkCmd.Parameters.AddWithValue("assetId", placeId);
        var isPlaceResult = await checkCmd.ExecuteScalarAsync();
        
        if (isPlaceResult == null || !Convert.ToBoolean(isPlaceResult))
        {
            return false; // Not a place
        }

        // Check if user owns this place
        if (placeAsset.OwnerUserId != currentUserId)
        {
            return false; // Not owner
        }

        return true; // Valid place owner
    }
}
