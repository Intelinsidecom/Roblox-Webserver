using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Website.Services;

namespace Website.Controllers.Frontend;

[ApiController]
public class PresenceApiController : ControllerBase
{
    private readonly PresenceTracker _presenceTracker;
    private readonly IConfiguration _configuration;

    public PresenceApiController(PresenceTracker presenceTracker, IConfiguration configuration)
    {
        _presenceTracker = presenceTracker ?? throw new ArgumentNullException(nameof(presenceTracker));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    [Authorize]
    [HttpPost("v1/presence/users")]
    public async Task<IActionResult> GetUserPresence(
        [FromBody] PresenceRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request?.UserIds == null || request.UserIds.Length == 0)
            return Content(Serialize(new { userPresences = new object[] { } }), "application/json");

        try
        {
            var userIds = request.UserIds.Take(100).ToArray();
            var connStr = _configuration.GetConnectionString("Default");

            var presences = new List<object>();

            foreach (var userId in userIds)
            {
                int userPresenceType = 0;
                long lastLocation = 0;
                long lastPlaceId = 0;
                string lastLocationUniverse = "";
                string lastGame = "";
                string gameId = "";
                string placeId = "";
                string lastOnline = "";
                bool isOnline = false;

                if (_presenceTracker.TryGetValue(userId, out var entry))
                {
                    var age = DateTime.UtcNow - entry.LastRequestTime;
                    if (age < TimeSpan.FromMinutes(5))
                    {
                        isOnline = true;
                        userPresenceType = 1;

                        if (!string.IsNullOrWhiteSpace(connStr))
                        {
                            try
                            {
                                await using var conn = new Npgsql.NpgsqlConnection(connStr);
                                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                                using var cmd = new Npgsql.NpgsqlCommand(
                                    "SELECT in_game, in_studio, current_place_id FROM users WHERE user_id = @userId", conn);
                                cmd.Parameters.AddWithValue("@userId", userId);
                                using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    var inGame = !reader.IsDBNull(0) && reader.GetBoolean(0);
                                    var inStudio = !reader.IsDBNull(1) && reader.GetBoolean(1);
                                    var currentPlaceId = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);

                                    if (inGame)
                                    {
                                        userPresenceType = 2;
                                        placeId = currentPlaceId > 0 ? currentPlaceId.ToString() : "";
                                    }
                                    else if (inStudio)
                                    {
                                        userPresenceType = 3;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

                presences.Add(new
                {
                    userPresenceType,
                    lastLocation,
                    lastPlaceId,
                    lastLocationUniverse,
                    lastGame,
                    gameId,
                    placeId,
                    lastOnline,
                    userId,
                    isOnline
                });
            }

            return Content(Serialize(new { userPresences = presences }), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PresenceApiController] GetUserPresence error: {ex.Message}");
            return Content(Serialize(new { userPresences = new object[] { } }), "application/json");
        }
    }

    public class PresenceRequest
    {
        [JsonPropertyName("userIds")]
        public long[]? UserIds { get; set; }
    }
}
