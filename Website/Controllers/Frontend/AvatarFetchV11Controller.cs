using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<int, string> AnimationTypeMap = new()
        {
            { 48, "climb" },
            { 49, "death" },
            { 50, "fall" },
            { 51, "idle" },
            { 52, "jump" },
            { 53, "run" },
            { 54, "swim" },
            { 55, "walk" },
            { 56, "pose" },
        };

        private static string GetAnimationKey(int assetTypeId)
        {
            return AnimationTypeMap.TryGetValue(assetTypeId, out var key) ? key : null;
        }

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

            var assetAndTypeList = new List<object>();
            var gearIds = new List<long>();
            var animationAssetIds = new Dictionary<string, long>();

            foreach (var asset in state.Assets)
            {
                var assetId = asset.id;
                var assetTypeId = asset.assetType.id;

                var animationKey = GetAnimationKey(assetTypeId);
                if (animationKey != null)
                {
                    if (!animationAssetIds.ContainsKey(animationKey))
                    {
                        animationAssetIds[animationKey] = assetId;
                    }
                    continue;
                }

                if (assetTypeId == 61)
                    continue;

                assetAndTypeList.Add(new { assetId, assetTypeId });

                if (assetTypeId == 19)
                {
                    gearIds.Add(assetId);
                }
            }

            var accessoryVersionIds = state.Assets
                .Select(a => a.id)
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
                headColorId = state.BodyColors.headColorId,
                leftArmColorId = state.BodyColors.leftArmColorId,
                leftLegColorId = state.BodyColors.leftLegColorId,
                rightArmColorId = state.BodyColors.rightArmColorId,
                rightLegColorId = state.BodyColors.rightLegColorId,
                torsoColorId = state.BodyColors.torsoColorId,

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
                Depth = state.Scales.depth,
                Proportion = state.Scales.proportion,
                BodyType = state.Scales.bodyType
            };

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
                resolvedAvatarType = state.PlayerAvatarType,
                accessoryVersionIds,
                equippedGearVersionIds,
                assetAndAssetTypeIds = assetAndTypeList.ToArray(),
                backpackGearVersionIds,
                animationAssetIds,
                playerAvatarType = state.PlayerAvatarType,
                scales,
                bodyColorsUrl,
                bodyColors,
                emotes
            };

            return Ok(response);
        }
    }
}