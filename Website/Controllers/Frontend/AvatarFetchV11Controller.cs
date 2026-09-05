using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Avatar;

namespace Website.Controllers
{
    [ApiController]
    [Route("v1.1/avatar-fetch")]
    public class AvatarFetchV11Controller : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAvatarFetch(
            [FromQuery] long? userId,
            [FromQuery] long? placeId,
            [FromServices] IConfiguration config)
        {
            long effectiveUserId = userId.GetValueOrDefault();

            if (effectiveUserId <= 0)
            {
                var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(claimVal))
                    long.TryParse(claimVal, out effectiveUserId);
            }

            if (effectiveUserId <= 0)
            {
                var cookie = Request.Cookies[".ROBLOSECURITY"];
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    try
                    {
                        var connString = config.GetConnectionString("Default");
                        if (!string.IsNullOrWhiteSpace(connString))
                        {
                            await using var conn = new NpgsqlConnection(connString);
                            await conn.OpenAsync();
                            const string sql = "select user_id from sessions where token = @t and (expires_at is null or expires_at > now() at time zone 'utc')";
                            await using var cmd = new NpgsqlCommand(sql, conn);
                            cmd.Parameters.AddWithValue("t", cookie);
                            var obj = await cmd.ExecuteScalarAsync();
                            if (obj is long uid) effectiveUserId = uid;
                            else if (obj is int iid) effectiveUserId = iid;
                            else if (obj != null)
                            {
                                try { effectiveUserId = Convert.ToInt64(obj); } catch { effectiveUserId = 0; }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (effectiveUserId <= 0)
                return StatusCode(403);

            var connectionString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500);

            AvatarState state;
            try
            {
                var repo = new AvatarRepository();
                state = await repo.GetAvatarAsync(connectionString, effectiveUserId);
            }
            catch
            {
                return StatusCode(403);
            }

            var wornIds = state.Assets.Select(a => a.id).ToArray();
            var assetAndTypeList = new System.Collections.Generic.List<object>();
            var gearIds = new System.Collections.Generic.List<long>();

            if (wornIds.Length > 0)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connectionString);
                    await conn.OpenAsync();

                    const string sql = "select asset_id, asset_type_id from assets where asset_id = any(@ids)";
                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("ids", wornIds);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var assetId = reader.GetInt64(0);
                        var assetTypeId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                        var entry = new
                        {
                            assetId,
                            assetTypeId
                        };

                        assetAndTypeList.Add(entry);

                        if (assetTypeId == 19)
                        {
                            gearIds.Add(assetId);
                        }
                    }
                }
                catch
                {
                    assetAndTypeList = wornIds
                        .Select(id => (object)new { assetId = id, assetTypeId = 0 })
                        .ToList();
                    gearIds.Clear();
                }
            }

            var accessoryVersionIds = wornIds
                .Where(id => !gearIds.Contains(id))
                .ToArray();

            var place = placeId.GetValueOrDefault();
            long[] equippedGearVersionIds;
            long[] backpackGearVersionIds;

            if (place != 0)
            {
                equippedGearVersionIds = Array.Empty<long>();
                backpackGearVersionIds = gearIds.ToArray();
            }
            else
            {
                equippedGearVersionIds = gearIds.ToArray();
                backpackGearVersionIds = gearIds.ToArray();
            }

            var bodyColors = new
            {
                HeadColor = state.BodyColors.headColorId,
                LeftArmColor = state.BodyColors.leftArmColorId,
                LeftLegColor = state.BodyColors.leftLegColorId,
                RightArmColor = state.BodyColors.rightArmColorId,
                RightLegColor = state.BodyColors.rightLegColorId,
                TorsoColor = state.BodyColors.torsoColorId
            };

            var scales = new
            {
                Height = state.Scales.height,
                Width = state.Scales.width,
                Head = state.Scales.head,
                Depth = 1.0,
                Proportion = 0.0,
                BodyType = 0.0
            };

            var animationAssetIds = new { };
            var emotes = Array.Empty<object>();

            var publicBaseUrl = config["PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;
            if (string.IsNullOrWhiteSpace(publicBaseUrl))
            {
                var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "https" : Request.Scheme;
                publicBaseUrl = $"{scheme}://freblx.com";
            }

            var bodyColorsUrl = $"{publicBaseUrl}/Asset/BodyColors.ashx?userId={effectiveUserId}";

            var response = new
            {
                resolvedAvatarType = "R15",
                accessoryVersionIds,
                equippedGearVersionIds,
                assetAndAssetTypeIds = assetAndTypeList.ToArray(),
                backpackGearVersionIds,
                animationAssetIds,
                playerAvatarType = "R15",
                scales,
                bodyColorsUrl,
                bodyColors,
                emotes
            };

            return Ok(response);
        }
    }
}
