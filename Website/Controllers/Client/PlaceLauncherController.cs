using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Games;
using Npgsql;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Website.Controllers.Client
{


    /// <summary>
    /// Attempt at getting the place joining to work. copied from void revival. im just testing stuff locally
    /// </summary>
    [ApiController]
    [Route("Game")]
    public class PlaceLauncherController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticationTicketService _ticketService;

        // Key from void-main privatekey1 (for 2016)
        private const string PrivateKey2016Pem = 
            "-----BEGIN RSA PRIVATE KEY-----\n" +
            "MIICXAIBAAKBgQDOirFwxWKEiVdFMlqqAaIofFcG31hIdEtnoC0tx0Ykx9BpoA3f\n" +
            "bStwQfUUv7usn49qCgGh25OWrS88jkr6Y2tce663lLVVEV9pymS9APcoy4quVYn9\n" +
            "/FbaDQh/bQGyPUR8AdUKaiA74dPI9w1yVp+uzOHAxHko7ou/9YK/+l3EtQIDAQAB\n" +
            "AoGAVX9yLmV2/7g+qQVMJJ3ie3HlMJIZ4HxLjozuxsl7ztPsAR1hQMDXP3P+OOWZ\n" +
            "kb7HRjT4MgFMGg58xEt+3CF1mid0UEmRxIezvrd2X5+muYckj/qOG1LHcYhWcHsp\n" +
            "6vO5kejbHdjfY/DEpOGeLmuH6hF3HM+aD5boAgru9SDfgs0CQQDyGyVe1jc5iPZV\n" +
            "Oaw2n01uvQD59azdnUy2WAb/nB2M+87+vUMrQ7z8Iat+jwSz/EAoL06b4FmEjt30\n" +
            "ynWsuRkvAkEA2mUPw0aCnpDMCQHkzp/ASAOiIwHiTzrnPcI2af71eylXTBofD43u\n" +
            "FbZKEIq7o6eFng1YGRNDt8kHwiWqvET/WwJAaCo/0ObvycRg39g5fSLbKOsO0Xzf\n" +
            "TFZSXB3RnQZpPHBW5gk+Lg4t8Hj4FTKpflroq6F2+9/yA/OIEbtOF+tnpwJAOgpL\n" +
            "syDlC9D9eJNZRJRuHHVivJz+kQHdfKtFnMvWX4HwIlh60r5sfLayXk0QawDVYNi5\n" +
            "Bgj5oTk655ztEBXiKwJBAILWgqOVjICo4dfmM5cjmrmFydTL9QPuytmCzGNDKY2V\n" +
            "O+8xRUgVTpDWfaQ/tGj+AxdAlae1w71DARe6fWItR34=\n" +
            "-----END RSA PRIVATE KEY-----";

        public PlaceLauncherController(IConfiguration configuration, AuthenticationTicketService ticketService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        }

        [HttpGet("PlaceLauncher.ashx")]
        public async Task<IActionResult> PlaceLauncher([FromQuery] string request, [FromQuery] long? placeId)
        {
            if (placeId == null)
            {
                return Ok(new PlaceLauncherResponse { status = 8, message = "Place does not exist" });
            }

            try
            {
                return request switch
                {
                    "RequestGame" or "RequestGameJob" or "RequestFollowUser" => await HandleRequestGame(placeId.Value, request),
                    "AuthenticateTicket" => await HandleAuthenticateTicket(),
                    "LogJoinClick" => Ok(new { status = 1, message = "Logged" }),
                    _ => BadRequest(new { status = 0, message = "Invalid request type" })
                };
            }
            catch (Exception ex)
            {
                return Ok(new PlaceLauncherResponse 
                { 
                    status = 4, 
                    message = $"Unknown Error: {ex.Message}" 
                });
            }
        }

        private async Task<IActionResult> HandleRequestGame(long placeId, string requestType)
        {
            long userId = 1; 

            if (!await UserCanAccessPlaceAsync(userId, placeId))
            {
                return Ok(new PlaceLauncherResponse { status = 3, message = "Access denied" });
            }

            var ticket = await _ticketService.CreateGameTicketAsync(userId, placeId);
            var jobId = Guid.NewGuid().ToString();
            var port = 53640;
            var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/');
            int gamestatus = 2; // Success
            var joinScriptUrl = $"{baseUrl}/Game/join.ashx?serverPort={port}&gameid={placeId}&jobid={jobId}";
            var authenticationUrl = $"{baseUrl}/Login/Negotiate.ashx";

            return Ok(new PlaceLauncherResponse
            {
                jobId = jobId,
                status = gamestatus,
                joinScriptUrl = joinScriptUrl,
                authenticationUrl = authenticationUrl,
                authenticationTicket = ticket.TicketToken,
                message = null
            });
        }

        [HttpGet("join.ashx")]
        [Produces("text/plain")]
        public async Task<IActionResult> Join([FromQuery] long gameid, [FromQuery] int serverPort, [FromQuery] string jobid)
        {
            long userId = 1;
            var userName = "Admin";
            var displayName = "Admin";
            var membership = "None"; // or "BuildersClub", etc.
            var accountAge = 365;
            var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/') + "/";

            // Generate the JSON structure same like void does
            var joinScript = new JoinScriptData
            {
                MachineAddress = "127.0.0.1",
                ServerPort = serverPort,
                ServerConnections = new[] { new ServerConnection { Address = "127.0.0.1", Port = serverPort } },
                PingUrl = $"{baseUrl}Game/ClientPresence.ashx?PlaceID={gameid}&userID={userId}",
                UserName = userName,
                DisplayName = displayName,
                UserId = userId,
                RobloxLocale = "en_us",
                GameLocale = "en_us",
                CharacterAppearance = $"{baseUrl}v1.1/avatar-fetch?userId={userId}&placeId={gameid}",
                ClientTicket = GenerateAuthTicket(userId, userName, jobid),
                GameId = jobid,
                PlaceId = gameid,
                UniverseId = gameid,
                CreatorId = 1,
                CreatorTypeEnum = "User",
                MembershipType = membership,
                AccountAge = accountAge,
                BaseUrl = baseUrl
            };

            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = null,
                WriteIndented = false 
            };
            var data = JsonSerializer.Serialize(joinScript, options);

            var signature = SignData("\r\n" + data);
            var result = $"--rbxsig%{signature}%\r\n{data}";

            return Content(result, "text/plain");
        }

        private async Task<IActionResult> HandleAuthenticateTicket()
        {
            var suggest = Request.Query["suggest"].ToString();
            if (string.IsNullOrEmpty(suggest))
            {
                return Unauthorized();
            }

            var ticket = await _ticketService.ValidateTicketAsync(suggest);
            if (ticket == null)
            {
                return Unauthorized();
            }

            Response.Cookies.Append(".ROBLOSECURITY", ticket.TicketToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                Path = "/",
                HttpOnly = true
            });

            return Content(ticket.TicketToken, "text/plain");
        }

        private string SignData(string content)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(PrivateKey2016Pem);
                var data = Encoding.UTF8.GetBytes(content);
                var signature = rsa.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(signature);
            }
        }

        private string GenerateAuthTicket(long userId, string userName, string jobId)
        {
            var dateStr = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt");
            return $"{userId};{userName};{jobId};{dateStr}";
        }

        private async Task<bool> UserCanAccessPlaceAsync(long userId, long placeId)
        {
            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                select a.access_type, u.creator_user_id 
                from assets a
                left join universes u on @placeId = ANY(u.place_ids)
                where a.asset_id = @placeId and a.is_place = true
                limit 1", conn);

            cmd.Parameters.AddWithValue("placeId", placeId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var accessType = reader.IsDBNull(0) ? 1 : reader.GetInt32(0);
                var creatorUserId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);

                if (creatorUserId == userId)
                    return true;

                return accessType switch
                {
                    1 => true,
                    2 => await AreFriendsAsync(userId, creatorUserId),
                    3 => false,
                    _ => false
                };
            }

            return false;
        }

        private async Task<bool> AreFriendsAsync(long userId1, long userId2)
        {
            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                select 1 from user_friends 
                where user_id = @user1 and friend_user_id = @user2
                union
                select 1 from user_friends 
                where user_id = @user2 and friend_user_id = @user1
                limit 1", conn);

            cmd.Parameters.AddWithValue("user1", userId1);
            cmd.Parameters.AddWithValue("user2", userId2);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        public class PlaceLauncherResponse
        {
            [JsonPropertyName("jobId")]
            public string? jobId { get; set; }
            [JsonPropertyName("status")]
            public int status { get; set; }
            [JsonPropertyName("joinScriptUrl")]
            public string? joinScriptUrl { get; set; }
            [JsonPropertyName("authenticationUrl")]
            public string? authenticationUrl { get; set; }
            [JsonPropertyName("authenticationTicket")]
            public string? authenticationTicket { get; set; }
            [JsonPropertyName("message")]
            public string? message { get; set; }
        }

        public class JoinScriptData
        {
            public int ClientPort { get; set; } = 0;
            public string MachineAddress { get; set; } = "";
            public int ServerPort { get; set; }
            public ServerConnection[] ServerConnections { get; set; } = Array.Empty<ServerConnection>();
            public string PingUrl { get; set; } = "";
            public int PingInterval { get; set; } = 30;
            public string UserName { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public bool SeleniumTestMode { get; set; } = false;
            public long UserId { get; set; }
            public string RobloxLocale { get; set; } = "en_us";
            public string GameLocale { get; set; } = "en_us";
            public bool SuperSafeChat { get; set; } = false;
            public string CharacterAppearance { get; set; } = "";
            public string ClientTicket { get; set; } = "";
            public string GameId { get; set; } = "";
            public long PlaceId { get; set; }
            public string MeasurementUrl { get; set; } = "";
            public string WaitingForCharacterGuid { get; set; } = "26eb3e21-aa80-475b-a777-b43c3ea5f7d2";
            public string BaseUrl { get; set; } = "";
            public string ChatStyle { get; set; } = "ClassicAndBubble";
            public string VendorId { get; set; } = "0";
            public string ScreenShotInfo { get; set; } = "";
            public string VideoInfo { get; set; } = "";
            public long CreatorId { get; set; }
            public string CreatorTypeEnum { get; set; } = "User";
            public string MembershipType { get; set; } = "None";
            public int AccountAge { get; set; } = 0;
            public string CookieStoreFirstTimePlayKey { get; set; } = "rbx_evt_ftp";
            public string CookieStoreFiveMinutePlayKey { get; set; } = "rbx_evt_fmp";
            public bool CookieStoreEnabled { get; set; } = true;
            public bool IsRobloxPlace { get; set; } = false;
            public bool GenerateTeleportJoin { get; set; } = false;
            public bool IsUnknownOrUnder13 { get; set; } = false;
            public string SessionId { get; set; } = "";
            public int DataCenterId { get; set; } = 69420;
            public long UniverseId { get; set; }
            public long BrowserTrackerId { get; set; } = 0;
            public bool UsePortraitMode { get; set; } = false;
            public long FollowUserId { get; set; } = 0;
        }

        public class ServerConnection
        {
            public string Address { get; set; } = "";
            public int Port { get; set; }
        }
    }
}
