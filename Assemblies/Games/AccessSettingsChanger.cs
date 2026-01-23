using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Games
{
    public static class DeviceCompatibilityHelper
    {
        // Device type mappings based on database schema
        private static readonly Dictionary<string, int> DeviceMap = new()
        {
            { "Computer", 1 },
            { "Phone", 2 },
            { "Tablet", 3 },
            { "Console", 4 }
        };

        /// <summary>
        /// Converts device selection string to JSON array of device numbers
        /// Expected format: "Computer,Phone,Tablet" or similar comma-separated values
        /// </summary>
        /// <param name="playableDevices">Comma-separated device names</param>
        /// <returns>JSON array string with device numbers</returns>
        public static string ConvertToDeviceCompatibilityJson(string playableDevices)
        {
            if (string.IsNullOrWhiteSpace(playableDevices))
            {
                return "[1, 2, 3]"; // Default: Computer, Phone, Tablet
            }

            var deviceNumbers = new List<int>();
            var deviceNames = playableDevices.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var deviceName in deviceNames)
            {
                var trimmedName = deviceName.Trim();
                if (DeviceMap.TryGetValue(trimmedName, out var deviceNumber))
                {
                    deviceNumbers.Add(deviceNumber);
                }
            }

            // If no valid devices found, return default
            if (deviceNumbers.Count == 0)
            {
                return "[1, 2, 3]";
            }

            // Sort for consistency and remove duplicates
            deviceNumbers = deviceNumbers.Distinct().OrderBy(x => x).ToList();
            return JsonSerializer.Serialize(deviceNumbers);
        }

        /// <summary>
        /// Converts JSON array from database to list of device names
        /// </summary>
        /// <param name="deviceCompatibilityJson">JSON array string from database</param>
        /// <returns>List of device names</returns>
        public static List<string> ConvertFromDeviceCompatibilityJson(string deviceCompatibilityJson)
        {
            if (string.IsNullOrWhiteSpace(deviceCompatibilityJson))
            {
                return new List<string> { "Computer", "Phone", "Tablet" }; // Default
            }

            try
            {
                var deviceNumbers = JsonSerializer.Deserialize<List<int>>(deviceCompatibilityJson);
                var deviceNames = new List<string>();

                foreach (var deviceNumber in deviceNumbers ?? new List<int>())
                {
                    var deviceName = DeviceMap.FirstOrDefault(x => x.Value == deviceNumber).Key;
                    if (!string.IsNullOrWhiteSpace(deviceName))
                    {
                        deviceNames.Add(deviceName);
                    }
                }

                return deviceNames;
            }
            catch
            {
                return new List<string> { "Computer", "Phone", "Tablet" }; // Default on error
            }
        }
    }

    public static class AccessSettingsChanger
    {
        /// <summary>
        /// Converts social slot type string to database integer value
        /// </summary>
        /// <param name="socialSlotType">Social slot type string</param>
        /// <returns>Integer value for database storage</returns>
        private static int GetServerFillTypeValue(string socialSlotType)
        {
            return socialSlotType?.ToLower() switch
            {
                "automatic" => 0,
                "empty" => 1,
                "custom" => 2,
                _ => 0 // default to automatic
            };
        }

        /// <summary>
        /// Converts database integer value to social slot type string
        /// </summary>
        /// <param name="serverFillType">Integer value from database</param>
        /// <returns>Social slot type string</returns>
        public static string GetServerFillTypeString(int serverFillType)
        {
            return serverFillType switch
            {
                0 => "Automatic",
                1 => "Empty",
                2 => "Custom",
                _ => "Automatic" // default to automatic
            };
        }

        /// <summary>
        /// Converts access string to database integer value
        /// </summary>
        /// <param name="access">Access string ("Everyone" or "Friends")</param>
        /// <returns>Integer value for database storage (1 = Everyone, 2 = Friends)</returns>
        private static int GetAccessTypeValue(string access)
        {
            return access?.ToLower() switch
            {
                "friends" => 2,
                "everyone" => 1,
                _ => 1 // default to everyone
            };
        }

        /// <summary>
        /// Converts database integer value to access string
        /// </summary>
        /// <param name="accessType">Integer value from database</param>
        /// <returns>Access string ("Everyone" or "Friends")</returns>
        public static string GetAccessTypeString(int accessType)
        {
            return accessType switch
            {
                2 => "Friends",
                1 => "Everyone",
                _ => "Everyone" // default to everyone
            };
        }

        /// <summary>
        /// Updates access settings for a place, ensuring that user owns the place
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="placeId">ID of place to update</param>
        /// <param name="userId">ID of user making the request</param>
        /// <param name="maxPlayers">Maximum number of players allowed</param>
        /// <param name="socialSlotType">Server fill type (Automatic, Empty, Custom)</param>
        /// <param name="numberOfCustomSocialSlots">Number of custom social slots (if applicable)</param>
        /// <param name="access">Access level (Everyone, Friends)</param>
        /// <param name="arePrivateServersAllowed">Whether private servers are allowed</param>
        /// <param name="isFreePrivateServer">Whether private servers are free</param>
        /// <param name="privateServersPrice">Price for private servers (if not free)</param>
        /// <param name="playableDevices">Comma-separated list of playable devices</param>
        /// <param name="sellGameAccess">Whether paid access is enabled</param>
        /// <param name="paidAccessPrice">Price for paid access</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false if place not found or user doesn't own it</returns>
        public static async Task<bool> UpdatePlaceAccessSettingsAsync(
            string connectionString,
            long placeId,
            long userId,
            int maxPlayers,
            string socialSlotType,
            int numberOfCustomSocialSlots,
            string access,
            bool arePrivateServersAllowed,
            bool isFreePrivateServer,
            int privateServersPrice,
            string playableDevices,
            bool sellGameAccess,
            int paidAccessPrice,
            CancellationToken cancellationToken = default)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new ArgumentException("connectionString is required", nameof(connectionString));
                if (placeId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(placeId));
                if (userId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(userId));

                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                // First verify user owns the place
                const string ownershipCheckSql = @"SELECT owner_user_id 
                                          FROM assets 
                                          WHERE asset_id = @placeId AND is_place = true";

                using var ownershipCmd = new NpgsqlCommand(ownershipCheckSql, conn);
                ownershipCmd.Parameters.AddWithValue("placeId", placeId);

                var ownerResult = await ownershipCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (ownerResult == null || Convert.ToInt64(ownerResult) != userId)
                {
                        return false; // User doesn't own this place
                }

                // Business rule: Private servers are not allowed when access is Friends
                if (GetAccessTypeValue(access) == 2) // 2 = Friends
                {
                    arePrivateServersAllowed = false;
                    isFreePrivateServer = true;
                    privateServersPrice = 100; // Reset to default
                }


                // Update access settings
                const string sql = @"UPDATE assets 
                         SET max_visitor_count = @maxVisitorCount,
                             server_fill_type = @serverFillType,
                             device_compatibility = @deviceCompatibility::jsonb,
                             number_of_custom_social_slots = @numberOfCustomSocialSlots,
                             access_type = @accessType,
                             private_servers_allowed = @arePrivateServersAllowed,
                             private_servers_free = @isFreePrivateServer,
                             private_servers_price = @privateServersPrice,
                             paid_access_enabled = @sellGameAccess,
                             paid_access_price = @paidAccessPrice,
                             last_updated = NOW()
                         WHERE asset_id = @placeId 
                         AND is_place = true";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("maxVisitorCount", maxPlayers);
                cmd.Parameters.AddWithValue("serverFillType", GetServerFillTypeValue(socialSlotType));
                cmd.Parameters.AddWithValue("deviceCompatibility", DeviceCompatibilityHelper.ConvertToDeviceCompatibilityJson(playableDevices));
                cmd.Parameters.AddWithValue("numberOfCustomSocialSlots", numberOfCustomSocialSlots);
                cmd.Parameters.AddWithValue("accessType", GetAccessTypeValue(access));
                cmd.Parameters.AddWithValue("arePrivateServersAllowed", arePrivateServersAllowed);
                cmd.Parameters.AddWithValue("isFreePrivateServer", isFreePrivateServer);
                cmd.Parameters.AddWithValue("privateServersPrice", privateServersPrice);
                cmd.Parameters.AddWithValue("sellGameAccess", sellGameAccess);
                cmd.Parameters.AddWithValue("paidAccessPrice", paidAccessPrice);
                cmd.Parameters.AddWithValue("placeId", placeId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                
                return rowsAffected > 0;
            }
            catch (NpgsqlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
