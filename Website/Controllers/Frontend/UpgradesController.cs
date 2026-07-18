using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Users;

namespace Website.Controllers;

    public class UpgradesController : Controller
    {
        private readonly IConfiguration _configuration;

        public UpgradesController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [Authorize]
        [HttpGet("upgrades/membership")]
        public async Task<IActionResult> UpgradeMembership([FromQuery] string? membershipType)
        {
            var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
            if (!isValid)
                return Redirect("/");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Redirect("/premium/membership");

            short statusValue;
            string? subscriptionType;
            bool premiumMember;

            switch (membershipType?.ToLowerInvariant())
            {
                case "buildersclub":
                case "bc":
                case "1":
                    statusValue = 1;
                    subscriptionType = "BuildersClub";
                    premiumMember = true;
                    break;
                case "turbobuildersclub":
                case "tbc":
                case "2":
                    statusValue = 2;
                    subscriptionType = "TurboBuildersClub";
                    premiumMember = true;
                    break;
                case "outrageousbuildersclub":
                case "obc":
                case "3":
                    statusValue = 3;
                    subscriptionType = "OutrageousBuildersClub";
                    premiumMember = true;
                    break;
                case "none":
                case "regular":
                case "0":
                    statusValue = 0;
                    subscriptionType = null;
                    premiumMember = false;
                    break;
                default:
                    return Redirect("/premium/membership");
            }

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync().ConfigureAwait(false);

                await using var cmd = new NpgsqlCommand(@"
                    update users
                    set membership_status = @status,
                        subscription_type = @subType,
                        premium_member = @premium
                    where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("status", statusValue);
                cmd.Parameters.AddWithValue("subType", (object?)subscriptionType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("premium", premiumMember);
                cmd.Parameters.AddWithValue("uid", userId);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch
            {
                // Update failed silently
            }

            return Redirect("/premium/membership");
        }

        [Authorize]
        [HttpGet("upgrades/PaymentMethods")]
        public async Task<IActionResult> PaymentMethods([FromQuery] int ap, [FromQuery] int amount)
        {
            var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
            if (!isValid)
                return Redirect("/upgrades/robux");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Redirect("/upgrades/robux");

            var allowFreeRobux = _configuration.GetValue<bool>("Economy:AllowFreeRobux");
            var maxPerTransaction = _configuration.GetValue<int>("Economy:MaxFreeRobuxFromApiAtTime");
            var maxBalance = _configuration.GetValue<int>("Economy:MaxFreeRobuxFromApi");

            if (!allowFreeRobux)
                return Redirect("/upgrades/robux");

            if (amount <= 0)
                return Redirect("/upgrades/robux");

            if (amount > maxPerTransaction)
                return Redirect("/upgrades/robux");

            try
            {
                var currentBalance = await UserQueries.GetCurrencyByIdAsync(connStr, userId, "robux").ConfigureAwait(false);

                if (currentBalance + amount > maxBalance)
                    return Redirect("/upgrades/robux");

                await UserQueries.IncrementCurrencyByIdAsync(connStr, userId, "robux", amount).ConfigureAwait(false);
            }
            catch
            {
            }

            return Redirect("/upgrades/robux");
        }
    }
