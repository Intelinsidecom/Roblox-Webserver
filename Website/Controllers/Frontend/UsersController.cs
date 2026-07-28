using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Users;
using Games;

namespace Website.Controllers;

    public class UsersController : Controller
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public class StatusUpdateRequest
        {
            public string Status { get; set; } = "";
        }

    [Authorize]
    [HttpPost("user/follow")]
    public async Task<IActionResult> Follow(
        [FromBody] FollowRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0 || request?.targetUserId == null || request.targetUserId <= 0)
            return Json(new { success = false, message = "Invalid parameters" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, message = "Database not configured" });

        var result = await UserQueries.FollowUserAsync(
            connStr, currentUserId, request.targetUserId.Value, cancellationToken).ConfigureAwait(false);

        return Json(result);
    }

    [Authorize]
    [HttpPost("api/user/unfollow")]
    public async Task<IActionResult> Unfollow(
        [FromBody] FollowRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0 || request?.targetUserId == null || request.targetUserId <= 0)
            return Json(new { success = false });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false });

        var result = await UserQueries.UnfollowUserAsync(
            connStr, currentUserId, request.targetUserId.Value, cancellationToken).ConfigureAwait(false);

        return Json(result);
    }

    [HttpGet("users/{id}/profile")]
    public async Task<IActionResult> Profile(long id)
    {
        var connStr = _configuration.GetConnectionString("Default");
        var profileUserName = "Guest";
        long currentUserId = 0;

        var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(idClaim))
            long.TryParse(idClaim, out currentUserId);

        Dictionary<string, object?>? profileData = null;

        if (!string.IsNullOrWhiteSpace(connStr) && id > 0)
        {
            profileData = await Users.UserQueries.GetUserProfileDataAsync(connStr, id).ConfigureAwait(false);
            if (profileData != null)
            {
                var name = profileData.GetValueOrDefault("userName") as string;
                if (!string.IsNullOrEmpty(name))
                    profileUserName = name;
            }
            else
            {
                var name = await Users.UserQueries.GetUserNameByIdAsync(connStr, id).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(name))
                    profileUserName = name;
            }
        }

        Dictionary<string, object?>? currentUserData = null;
        if (currentUserId > 0 && !string.IsNullOrWhiteSpace(connStr) && currentUserId != id)
        {
            currentUserData = await Users.UserQueries.GetUserProfileDataAsync(connStr, currentUserId).ConfigureAwait(false);
        }

        var isOwnProfile = currentUserId > 0 && currentUserId == id;
        var isLoggedIn = currentUserId > 0;

        // Determine subscription level
        var profileSubType = profileData?.GetValueOrDefault("subscriptionType") as string;
        var membershipStatus = profileData?.GetValueOrDefault("membershipStatus") as short? ?? 0;
        var isObc = profileSubType == "OutrageousBuildersClub" || membershipStatus >= 3;
        var isTbc = profileSubType == "TurboBuildersClub" || membershipStatus >= 2;
        var isAnyBc = profileSubType == "BuildersClub" || profileSubType == "TurboBuildersClub" || profileSubType == "OutrageousBuildersClub" || membershipStatus >= 1;
        var profileCanTrade = profileData?.GetValueOrDefault("canTrade") as bool? ?? false;
        var profileCanPm = profileData?.GetValueOrDefault("canPm") as bool? ?? false;
        var profileCanChat = profileData?.GetValueOrDefault("canChat") as bool? ?? false;
        var profileVisibility = profileData?.GetValueOrDefault("profileVisibility") as string ?? "public";
        var profileDescription = profileData?.GetValueOrDefault("descriptionBio") as string ?? "";

        var currentUserSubType = currentUserData?.GetValueOrDefault("subscriptionType") as string;
        var currentMembershipStatus = currentUserData?.GetValueOrDefault("membershipStatus") as short? ?? 0;
        var currentUserIsObc = currentUserSubType == "OutrageousBuildersClub" || currentMembershipStatus >= 3;
        var currentUserIsTbc = currentUserSubType == "TurboBuildersClub" || currentMembershipStatus >= 2;
        var currentUserIsAnyBc = currentUserSubType == "BuildersClub" || currentUserSubType == "TurboBuildersClub" || currentUserSubType == "OutrageousBuildersClub" || currentMembershipStatus >= 1;
        var currentUserCanTrade = currentUserData?.GetValueOrDefault("canTrade") as bool? ?? false;

        var friendsCount = 0;
        if (!string.IsNullOrWhiteSpace(connStr) && id > 0)
        {
            try
            {
                friendsCount = await Users.UserQueries.GetFriendListTotalCountAsync(connStr, id, "AllFriends").ConfigureAwait(false);
            }
            catch { }
        }
        var followersCount = profileData?.GetValueOrDefault("followersCount") as int? ?? 0;
        var followingsCount = profileData?.GetValueOrDefault("followingCount") as int? ?? 0;
        var headshotThumbnailUrl = profileData?.GetValueOrDefault("headshotThumbnailUrl") as string;
        var avatarThumbnailUrl = profileData?.GetValueOrDefault("avatarThumbnailUrl") as string;

        var inGame = profileData?.GetValueOrDefault("inGame") as bool? ?? false;
        var currentPlaceId = profileData?.GetValueOrDefault("currentPlaceId") as long?;
        var gameName = "";
        if (inGame && currentPlaceId.HasValue && currentPlaceId.Value > 0 && !string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                await using var gameConn = new NpgsqlConnection(connStr);
                await gameConn.OpenAsync().ConfigureAwait(false);
                await using var gameCmd = new NpgsqlCommand(@"
                    select u.name from universes u
                    where @placeId = any(u.place_ids)
                    limit 1", gameConn);
                gameCmd.Parameters.AddWithValue("placeId", currentPlaceId.Value);
                var result = await gameCmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (result != null && result != DBNull.Value)
                    gameName = Convert.ToString(result) ?? "";
            }
            catch
            {
                // Game name lookup failed silently
            }
        }

        var statusText = profileData?.GetValueOrDefault("statusText") as string ?? "";
        var userCreated = profileData?.GetValueOrDefault("userCreated") as DateTime?;

        List<Dictionary<string, object?>> wornAssets = new();
        var totalPlaceVisits = 0;
        if (!string.IsNullOrWhiteSpace(connStr) && id > 0)
        {
            try
            {
                wornAssets = await UserQueries.GetWornAssetDetailsAsync(connStr, id).ConfigureAwait(false);
            }
            catch
            {
            }
            try
            {
                totalPlaceVisits = await UserQueries.GetTotalPlaceVisitsAsync(connStr, id).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        var userGames = new List<UniverseListEntry>();
        if (!string.IsNullOrWhiteSpace(connStr) && id > 0)
        {
            try
            {
                userGames = (await GameListingService.GetUniversesForUserAsync(connStr, id).ConfigureAwait(false)).ToList();
            }
            catch
            {
            }
        }

        // Determine friendship/request/follow status between current user and profile user
        var areFriends = false;
        var friendRequestPending = false;
        var incomingFriendRequestPending = false;
        var incomingFriendRequestId = 0L;
        var maySendFriendInvitation = false;
        var isFollowing = false;
        var mayFollow = false;
        var canBeFollowed = false;

        var isVieweeBlocked = false;

        if (currentUserId > 0 && currentUserId != id && !string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                areFriends = await UserQueries.AreFriendsAsync(connStr, currentUserId, id).ConfigureAwait(false);
                isFollowing = await UserQueries.IsFollowingAsync(connStr, currentUserId, id).ConfigureAwait(false);
                var (hasPending, requestId, isIncoming) = await UserQueries.GetPendingFriendRequestAsync(connStr, currentUserId, id).ConfigureAwait(false);
                friendRequestPending = hasPending && !isIncoming;
                incomingFriendRequestPending = hasPending && isIncoming;
                incomingFriendRequestId = hasPending && isIncoming ? requestId : 0;
                isVieweeBlocked = await UserQueries.IsBlockedAsync(connStr, currentUserId, id).ConfigureAwait(false);
            }
            catch
            {
                // Defaults remain false
            }

            maySendFriendInvitation = !areFriends && !friendRequestPending && !incomingFriendRequestPending && !isVieweeBlocked;
            canBeFollowed = profileVisibility != "private" && !isVieweeBlocked;
            mayFollow = canBeFollowed;
        }

        ViewBag.UserGames = userGames;

        ViewBag.ProfileUserId = id.ToString();
        ViewBag.ProfileUserName = profileUserName;
        ViewBag.CurrentUserId = currentUserId.ToString();
        ViewBag.FriendsCount = friendsCount;
        ViewBag.FollowersCount = followersCount;
        ViewBag.FollowingsCount = followingsCount;
        ViewBag.IsOwnProfile = isOwnProfile;
        ViewBag.IsLoggedIn = isLoggedIn;
        ViewBag.ProfileCanTrade = isObc || isTbc;
        ViewBag.ProfileCanPm = profileCanPm;
        ViewBag.ProfileCanChat = profileCanChat;
        ViewBag.AreFriends = areFriends;
        ViewBag.FriendRequestPending = friendRequestPending;
        ViewBag.IncomingFriendRequestPending = incomingFriendRequestPending;
        ViewBag.IncomingFriendRequestId = incomingFriendRequestId;
        ViewBag.MaySendFriendInvitation = maySendFriendInvitation;
        ViewBag.IsBlockButtonVisible = isLoggedIn && !isOwnProfile;
        ViewBag.IsVieweeBlocked = isVieweeBlocked;
        ViewBag.IsFollowing = isFollowing;
        ViewBag.MayFollow = mayFollow;
        ViewBag.CanBeFollowed = canBeFollowed;
        ViewBag.ProfileVisibility = profileVisibility;
        ViewBag.ProfileDescription = profileDescription;
        ViewBag.ProfileSubscriptionType = profileSubType ?? "";
        ViewBag.ProfileMembershipStatus = membershipStatus;
        ViewBag.IsOBC = isObc;
        ViewBag.IsTBC = isTbc;
        ViewBag.IsAnyBC = isAnyBc;
        ViewBag.CurrentUserCanTrade = currentUserIsObc || currentUserIsTbc;
        var defaultThumb = _configuration["Thumbnails:DefaultThumbnailUrl"];
        if (string.IsNullOrWhiteSpace(defaultThumb)) defaultThumb = "/images/default.png";
        var hasHeadshot = !string.IsNullOrWhiteSpace(headshotThumbnailUrl);
        var hasAvatar = !string.IsNullOrWhiteSpace(avatarThumbnailUrl);
        var profileUserId = ViewBag.ProfileUserId as string ?? id.ToString();
        ViewBag.HeadshotThumbnailUrl = hasHeadshot ? headshotThumbnailUrl : defaultThumb;
        ViewBag.HeadshotFinal = hasHeadshot;
        ViewBag.HeadshotRetryJson = hasHeadshot ? "null" : $"\"/thumbnail/avatar-headshot?userId={profileUserId}\"";
        ViewBag.AvatarThumbnailUrl = hasAvatar ? avatarThumbnailUrl : defaultThumb;
        ViewBag.InGame = inGame;
        ViewBag.CurrentPlaceId = currentPlaceId?.ToString() ?? "";
        ViewBag.GameName = gameName;
        ViewBag.GamePlaceId = currentPlaceId?.ToString() ?? "";
        var inStudio = profileData?.GetValueOrDefault("inStudio") as bool? ?? false;
        var lastActivity = profileData?.GetValueOrDefault("lastActivity") as DateTime?;
        var isOnline = (DateTime.UtcNow - (lastActivity ?? DateTime.MinValue)).TotalMinutes < 5;
        ViewBag.InStudio = inStudio;
        ViewBag.IsOnline = isOnline;
        var profileCollectables = profileData?.GetValueOrDefault("profileCollectables") as int[];
        ViewBag.ProfileCollectables = profileCollectables ?? Array.Empty<int>();
        ViewBag.HasProfileCollectables = profileCollectables is { Length: > 0 };
        ViewBag.StatusText = statusText;
        ViewBag.WornAssets = wornAssets;
        ViewBag.JoinDate = userCreated?.ToString("M/d/yyyy") ?? "";
        ViewBag.PlaceVisits = totalPlaceVisits;

        return View("~/Views/Pages/users/{id}/profile.cshtml");
    }

    [HttpGet("users/{id}/inventory")]
    public async Task<IActionResult> Inventory(long id)
    {
        var connStr = _configuration.GetConnectionString("Default");
        long currentUserId = 0;

        var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(idClaim))
            long.TryParse(idClaim, out currentUserId);

        var isLoggedIn = currentUserId > 0;
        var isOwnPage = currentUserId > 0 && currentUserId == id;

        var profileUserName = "Guest";
        if (!string.IsNullOrWhiteSpace(connStr) && id > 0)
        {
            var name = await Users.UserQueries.GetUserNameByIdAsync(connStr, id).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(name))
                profileUserName = name;
        }

        ViewBag.ProfileUserId = id.ToString();
        ViewBag.ProfileUserName = profileUserName;
        ViewBag.CurrentUserId = currentUserId.ToString();
        ViewBag.IsLoggedIn = isLoggedIn;
        ViewBag.IsOwnPage = isOwnPage;

        return View("~/Views/Pages/users/{id}/inventory.cshtml");
    }

    [HttpGet("users/{id}/user-status")]
    public IActionResult UserStatus(long id)
    {
        return Content("<div class=header-userstatus><div class=header-userstatus-text ng-hide=profileHeaderLayout.statusFormShown><span id=userStatusText class=text-overflow ng-class=\"{'userstatus-editable':profileHeaderLayout.mayUpdateStatus}\" ng-bind=profileHeaderLayout.statusText|statusfilter ng-click=revealStatusForm() ng-cloak></span><span ng-if=\"profileHeaderLayout.mayUpdateStatus &amp;&amp; !profileHeaderLayout.statusText\" id=userStatusText class=text-overflow ng-class=\"{'userstatus-editable':profileHeaderLayout.mayUpdateStatus}\" ng-click=revealStatusForm() ng-cloak style=\"cursor:pointer\">Add a status</span></div><div class=form-horizontal id=statusForm role=form ng-cloak ng-show=profileHeaderLayout.mayUpdateStatus&amp;&amp;profileHeaderLayout.statusFormShown ng-class=\"{'form-has-error':profileHeaderLayout.hasError}\"><div class=form-group><input class=\"form-control input-field\" id=txtStatusMessage maxlength={{profileHeaderLayout.editStatusMaxLength}} ng-cloak placeholder=\"What are you up to?\" ng-model=profileHeaderLayout.statusTextInput status-input-element key-press-enter=updateStatus(true) key-press-escape=blurStatusForm($event)></div><button class=\"btn-fixed-width btn-control-xs header-userstatus-share-button\" ng-click=updateStatus(true) ng-hide=profileHeaderLayout.statusFormSending>Save</button> <span class=\"spinner spinner-sm header-userstatus-share-progress\" id=loadingImage ng-show=profileHeaderLayout.statusFormSending title=Sharing...></span></div></div>", "text/html");
    }

    [Authorize]
    [HttpPost("users/{id}/user-status")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> UpdateUserStatus(long id, [FromForm] StatusUpdateRequest? request)
    {
        var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim) || !long.TryParse(idClaim, out var currentUserId) || currentUserId != id)
            return Json(new { success = false, message = "Unauthorized" });

        var statusText = request?.Status ?? "";
        statusText = statusText.Replace("{", "").Replace("}", "");
        if (statusText.Length > 254)
            statusText = statusText.Substring(0, 254);

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, message = "Database not configured" });

        var ok = await Users.UserQueries.UpdateUserStatusTextAsync(connStr, currentUserId, statusText).ConfigureAwait(false);
        if (!ok)
            return Json(new { success = false, message = "Failed to update status" });

        return Json(new { success = true, message = statusText });
    }

    [HttpGet("users/{id}/system-feedback")]
    public IActionResult SystemFeedback(long id)
    {
        return Content("<div class=alert-system-feedback><div class=\"alert alert-warning\"></div></div>", "text/html");
    }

    [HttpGet("users/{id}/profile-groups-section")]
    public IActionResult ProfileGroupsSection(long id)
    {
        return Content("<div ng-controller=profileGroupController ng-class=\"{'section':!layout.isGridOn,'container-list':layout.isGridOn}\" ng-show=\"groups.length>0\" ng-init=getGroupsData()><div class=container-header><h3>Groups</h3></div><div class=profile-slide-container><div id=groups-switcher class=\"switcher slide-switcher groups\" slide-switcher collection=groups ng-hide=layout.isGridOn></div></div></div>", "text/html");
    }

    [HttpGet("users/{id}/profile-collections-section")]
    public IActionResult ProfileCollectionsSection(long id)
    {
        return Content("<ul class=\"hlist collections-list item-list\" ng-init=getCollectionsData()><li class=\"list-item asset-item collections-item\" ng-repeat=\"item in collections\"><a ng-href={{item.AssetSeoUrl}} class=collections-link title={{item.Name}}><div class=img-container><img lazy-img={{item.Thumbnail.Url}} thumbnail=item.Thumbnail reset-src=true image-retry><div class=asset-restriction-icon><span ng-show=item.AssetRestrictionIcon&amp;&amp;item.AssetRestrictionIcon.CssTag class=icon-label ng-class=\"'icon-'+item.AssetRestrictionIcon.CssTag+'-label'\"></span></div></div><span class=\"text-overflow item-name\">{{item.Name}}</span></a></ul>", "text/html");
    }

    [HttpGet("users/{id}/slide-switcher")]
    public IActionResult SlideSwitcher(long id)
    {
        return Content(@"<ul class=""slide-items-container switcher-items hlist""><li class=""switcher-item slide-item-container"" ng-repeat=""item in collection"" ng-show=shouldPreLoad($index) ng-class=""{'active':curIdx===$index}""><div class=""col-sm-6 slide-item-container-left""><div class=slide-item-emblem-container><a ng-href={{item.GroupUrl}}><img class=slide-item-image lazy-img={{item.Emblem.Url}} thumbnail=item.Emblem reset-src=true image-retry alt={{item.Name}}></a></div></div><div class=""col-sm-6 text-overflow slide-item-container-right groups""><div class=slide-item-info><h2 class=""slide-item-name groups"">{{item.Name}}</h2><p class=""text-description slide-item-description groups"">{{item.Description}}</div><div class=slide-item-stats><ul class=hlist><li class=list-item><div class=""text-label slide-item-stat-title"">Members</div><div class=""text-lead slide-item-members-count"">{{item.Members|abbreviate}}</div><li class=list-item><div class=""text-label slide-item-stat-title"">Rank</div><div class=""text-lead text-overflow slide-item-my-rank groups"">{{item.Rank}}</div></ul></div></div></ul><a class=""carousel-control left"" ng-if=""collection.length>1"" ng-click=slidePrev()><span class=icon-carousel-left></span></a> <a class=""carousel-control right"" ng-if=""collection.length>1"" ng-click=slideNext()><span class=icon-carousel-right></span></a>", "text/html");
    }

    [HttpGet("users/{id}/profile-player-assets")]
    public IActionResult ProfilePlayerAssets(long id)
    {
        return Content(@"<div class=container-header><h3>{{layout.title}}</h3><a ng-href={{layout.assetUrl}} ng-show=layout.showSeeAllButton class=""btn-fixed-width btn-secondary-xs btn-more"">See All</a></div><div class=section-content><ul class=""hlist item-list""><li class=""list-item asset-item"" ng-repeat=""item in assets""><a ng-href={{item.AssetSeoUrl}} title={{item.Name}}><img lazy-img={{item.Thumbnail.Url}} thumbnail=item.Thumbnail reset-src=true image-retry> <span class=""text-overflow item-name"">{{item.Name}}</span></a></ul></div>", "text/html");
    }

    [HttpGet("users/profile/playerassets-json")]
    public async Task<IActionResult> PlayerAssetsJson(
        [FromQuery] int assetTypeId,
        [FromQuery] long userId,
        CancellationToken cancellationToken = default)
    {
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr) || userId <= 0)
            return Json(new { Assets = new object[] { }, Title = "", AssetTypeInventoryUrl = "", IsSeeAllHeaderButtonVisible = false });

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"
                select a.asset_id, a.name, coalesce(a.thumbnail_url, '') as thumbnail_url
                from assets a
                where a.owner_user_id = @uid and a.asset_type_id = @typeId
                order by a.created_at desc
                limit 10";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("typeId", assetTypeId);

            var title = Assets.AssetTypeNames.GetTypeName(assetTypeId);
            var items = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var thumb = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var slug = System.Text.RegularExpressions.Regex.Replace(name.Trim(), @"[^a-zA-Z0-9\s-]", "").Replace(" ", "-");

                items.Add(new
                {
                    AssetSeoUrl = $"/catalog/{assetId}/{slug}",
                    Name = name,
                    Thumbnail = new { Url = thumb, Final = true }
                });
            }

            return Json(new
            {
                Assets = items,
                Title = title + "s",
                AssetTypeInventoryUrl = $"/develop?view={assetTypeId}",
                IsSeeAllHeaderButtonVisible = items.Count > 0
            });
        }
        catch
        {
            return Json(new { Assets = new object[] { }, Title = "", AssetTypeInventoryUrl = "", IsSeeAllHeaderButtonVisible = false });
        }
    }

    [HttpGet("users/profile/robloxcollections-json")]
    public async Task<IActionResult> CollectionsJson(
        [FromQuery] long userId,
        CancellationToken cancellationToken = default)
    {
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr) || userId <= 0)
            return Json(new { CollectionsItems = new object[] { } });

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            // First, get the user's explicitly selected profile collectables
            int[] collectableIds;
            {
                await using var collectCmd = new NpgsqlCommand(
                    "select profile_collectables from users where user_id = @uid", conn);
                collectCmd.Parameters.AddWithValue("uid", userId);
                var result = await collectCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                collectableIds = result == null || result == DBNull.Value
                    ? Array.Empty<int>()
                    : (int[])result;
            }

            if (collectableIds.Length == 0)
                return Json(new { CollectionsItems = new object[] { } });

            const string sql = @"
                select a.asset_id, a.name, coalesce(a.thumbnail_url, '') as thumbnail_url
                from assets a
                join user_assets ua on ua.asset_id = a.asset_id and ua.user_id = @uid
                where a.asset_id = any(@ids)
                order by a.name";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("ids", collectableIds);

            var items = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var thumb = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var slug = System.Text.RegularExpressions.Regex.Replace(name.Trim(), @"[^a-zA-Z0-9\s-]", "").Replace(" ", "-");

                items.Add(new
                {
                    AssetSeoUrl = $"/catalog/{assetId}/{slug}",
                    Name = name,
                    Thumbnail = new { Url = thumb, Final = true },
                    AssetRestrictionIcon = (object?)null
                });
            }

            return Json(new { CollectionsItems = items });
        }
        catch
        {
            return Json(new { CollectionsItems = new object[] { } });
        }
    }

    [HttpGet("users/profile/playergroups-json")]
    public IActionResult GroupsJson(
        [FromQuery] long userId)
    {
        return Json(new
        {
            Groups = new object[] { }
        });
    }

    /// <summary>
    /// Cursor-based inventory listing used by the avatar editor.
    /// Example: /users/inventory/list-json?assetTypeId=2&cursor=&itemsPerPage=50&sortOrder=Desc
    /// </summary>
    [HttpGet("users/{id}/inventory/assets-list")]
    [HttpGet("users/{id}/assets-list")]
    public IActionResult InventoryAssetsList(long id)
    {
        return Content("<div class=current-items ng-class=\"{'hide-items':!currentData.templateVisible}\"><div class=container-header ng-class=\"{'place-header':currentData.category.name=='Places'&amp;&amp;staticData.isOwnPage}\"><div class=assets-explorer-title><div ng-class=\"{'hidden-xs':currentData.category.items.length>1}\"><ul class=breadcrumb-container><li><span>{{currentData.category.name}}</span><li ng-show=\"currentData.category.items.length>1\"><span class=icon-right-16x16></span><li ng-show=\"currentData.category.items.length>1\"><span>{{currentData.subcategory.name}}</span></ul></div></div><div class=header-content ng-hide=\"currentData.itemSection===null\"><a ng-href={{currentData.assetTypeUrl}} class=\"btn btn-more btn-primary-md\">Get More</a><div class=\"small get-more\">Explore the {{currentData.itemSection}} to find more {{currentData.category.name}}!</div></div></div><div ng-show=\"assets.length&lt;1\" class=item-cards><div class=section-content-off><span ng-hide=staticData.isOwnPage>This user has</span> <span ng-show=staticData.isOwnPage>You have</span> <span ng-show=\"pageType==='favorites'\">not favorited any {{currentData.category.name|lowercase}}.</span> <span ng-hide=\"pageType==='favorites'\">no <span ng-show=\"currentData.category.name=='Accessories'||currentData.category.name=='Avatar Animations'\">{{currentData.subcategory.name|lowercase}} </span><span>{{currentData.category.name|lowercase}}.</span></span> <span ng-hide=\"pageType==='favorites'||currentData.subcategory.name=='Badges'||currentData.subcategory.name=='Game Passes'||currentData.category.name=='Places'\">Try using the <a ng-if=\"staticData.isLibraryLinkEnabled||currentData.itemSection==='catalog'\" class=text-link ng-href={{currentData.assetTypeUrl}}>{{currentData.itemSection}}</a> <span ng-if=\"!staticData.isLibraryLinkEnabled&amp;&amp;currentData.itemSection!=='catalog'\">{{currentData.itemSection}}</span> to find new items.</span></div></div><ul id=assetsItems class=\"hlist item-cards item-cards-embed\"><li ng-repeat=\"item in assets\" class=\"list-item item-card\" ng-class=\"{'place-item':currentData.category.name=='Places'}\"><div class=item-card-container><a ng-href={{item.Item.AbsoluteUrl}} class=item-card-link><div class=item-card-thumb-container><div ng-hide=\"item.Product.SerialNumber==null\" class=item-serial-number>#{{item.Product.SerialNumber}}</div><img ng-src={{item.Thumbnail.Url}} thumbnail=item.Thumbnail image-retry class=item-card-thumb><div class=\"item-expire-time-label text-overflow\" ng-hide=\"item.UserItem.RentalExpireTime==null\">Exp: {{item.UserItem.RentalExpireTime}}</div><span ng-show=item.AssetRestrictionIcon ng-class=\"'icon-'+item.AssetRestrictionIcon.CssTag+'-label'\"></span></div><div class=\"text-overflow item-card-name\" title={{item.Item.Name}}>{{item.Item.Name}}</div></a><div ng-if=item.Item.AudioUrl class=MediaPlayerControls><div class=\"MediaPlayerIcon icon-play\" data-mediathumb-url={{item.Item.AudioUrl}} data-jplayer-version={{staticData.jPlayerVersion}}></div></div><div class=\"text-overflow item-card-creator\"><span class=\"xsmall text-label\">By</span> <a class=\"xsmall text-overflow text-link\" ng-href={{item.Creator.CreatorProfileLink}} ng-hide=\"pageType!=='favorites'&amp;&amp;currentData.category.name=='Places'&amp;&amp;(currentData.subcategory.name=='My VIP Servers'||currentData.subcategory.name=='Other VIP Servers')&amp;&amp;staticData.isOwnPage\">{{item.Creator.Name}}</a> <a class=\"xsmall text-overflow text-link\" ng-href={{item.PrivateServer.OwnerProfileLink}} ng-show=\"pageType!=='favorites'&amp;&amp;(currentData.subcategory.name=='My VIP Servers'||currentData.subcategory.name=='Other VIP Servers')\">{{item.PrivateServer.OwnerName}}</a></div><div class=item-card-price><span class=icon-robux-16x16 ng-show=item.HasPrice></span> <span class=text-robux ng-show=item.HasPrice>{{item.Product.PriceInRobux|abbreviate:0}}</span> <span class=text-label ng-hide=item.HasPrice><span ng-if=\"item.Product===null||item.Product.NoPriceText===null\">{{\"Offsale\"}}</span> <span ng-if=\"item.Product.NoPriceText.length>0\" ng-class=\"{'text-robux':item.Product.NoPriceText==='Free'}\">{{item.Product.NoPriceText}}</span></span></div></div></ul><div class=pager-holder cursor-pagination=assetsPager></div></div>", "text/html");
    }

    [HttpGet("users/{id}/inventory/rbx-cursor-pagination")]
    [HttpGet("users/{id}/rbx-cursor-pagination")]
    public IActionResult InventoryCursorPagination(long id)
    {
        return Content("<ul class=pager><li class=pager-prev ng-class={disabled:!cursorPaging.canLoadPreviousPage()}><a ng-click=cursorPaging.loadPreviousPage()><span class=icon-back></span></a><li><span>Page {{cursorPaging.getCurrentPageNumber()}}</span><li class=pager-next ng-class={disabled:!cursorPaging.canLoadNextPage()}><a ng-click=cursorPaging.loadNextPage()><span class=icon-next></span></a></ul>", "text/html");
    }

    [HttpGet("users/inventory/list-json")]
    public async Task<IActionResult> GetInventory(
        [FromQuery] long userId,
        [FromQuery] int assetTypeId,
        [FromQuery] string? cursor,
        [FromQuery] int itemsPerPage = 50,
        [FromQuery] string? sortOrder = "Desc",
        [FromQuery] string? placeTab = null,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return new JsonResult(new { isValid = false, Data = "Invalid userId", data = "Invalid userId" });

        if (itemsPerPage <= 0 || itemsPerPage > 100)
            itemsPerPage = 50;

        var isDesc = string.IsNullOrWhiteSpace(sortOrder) ||
                     sortOrder.Equals("Desc", StringComparison.OrdinalIgnoreCase);

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            var errorData = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["Items"] = new System.Collections.Generic.List<object>(),
                ["nextPageCursor"] = null
            };

            var errorResponse = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["isValid"] = false,
                ["Data"] = "Database connection string is not configured."
            };

            return new JsonResult(errorResponse);
        }

        long? lastAssetId = null;
        if (!string.IsNullOrWhiteSpace(cursor) && long.TryParse(cursor, out var parsedCursor) && parsedCursor > 0)
        {
            lastAssetId = parsedCursor;
        }

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var isCreated = string.Equals(placeTab, "Created", StringComparison.OrdinalIgnoreCase);
            var isMyGames = string.Equals(placeTab, "MyGames", StringComparison.OrdinalIgnoreCase);

            string sql;
            string idColumn;

            if (isMyGames)
            {
                sql = @"select u.universe_id,
       u.name,
       COALESCE(a.thumbnail_url, u.thumbnail_url, '/images/white-outline.png') as thumbnail_url,
       c.user_name,
       u.creator_user_id
from universes u
left join assets a on a.asset_id = u.root_place_id
left join users c on c.user_id = u.creator_user_id
where u.creator_user_id = @uid";
                idColumn = "u.universe_id";
            }
            else if (isCreated)
            {
                sql = @"select a.asset_id,
       a.name,
       coalesce(a.thumbnail_url, '') as thumbnail_url,
       u.user_name,
       u.user_id
from assets a
join users u on u.user_id = a.owner_user_id
where a.owner_user_id = @uid
  and a.asset_type_id = @assetTypeId";
                idColumn = "a.asset_id";
            }
            else
            {
                sql = @"select a.asset_id,
       a.name,
       coalesce(a.thumbnail_url, '') as thumbnail_url,
       u.user_name,
       u.user_id
from user_assets ua
join assets a on a.asset_id = ua.asset_id
join users u on u.user_id = a.owner_user_id
where ua.user_id = @uid
  and a.asset_type_id = @assetTypeId";
                idColumn = "a.asset_id";
            }

            if (lastAssetId.HasValue)
            {
                if (isDesc)
                {
                    sql += $" and {idColumn} < @cursorAssetId";
                }
                else
                {
                    sql += $" and {idColumn} > @cursorAssetId";
                }
            }

            sql += isDesc ? $" order by {idColumn} desc" : $" order by {idColumn} asc";
            sql += " limit @limit";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("assetTypeId", assetTypeId);
            cmd.Parameters.AddWithValue("limit", itemsPerPage + 1); // fetch one extra to detect next page
            if (lastAssetId.HasValue)
            {
                cmd.Parameters.AddWithValue("cursorAssetId", lastAssetId.Value);
            }

            var items = new List<object>();
            long? nextCursor = null;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var rawThumb = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var thumbUrl = string.IsNullOrWhiteSpace(rawThumb)
                    ? (assetTypeId == 3 ? (_configuration["AudioThumbnailUrl"] ?? "/images/audio.png") : (_configuration["DefaultThumbnailUrl"] ?? "/images/default.png"))
                    : rawThumb;
                var creatorName = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3);
                var creatorId = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);

                if (items.Count >= itemsPerPage)
                {
                    // This row exists only to indicate there is another page.
                    nextCursor = assetId;
                    break;
                }

                var itemObject = new System.Collections.Generic.Dictionary<string, object?>
                {
                    ["Item"] = new System.Collections.Generic.Dictionary<string, object?>
                    {
                        ["AssetId"] = assetId,
                        ["Name"] = name,
                        ["AbsoluteUrl"] = isMyGames ? $"/games/{assetId}/" : $"/catalog/{assetId}/item",
                        ["AudioUrl"] = assetTypeId == 3 && !isMyGames ? $"/asset/?id={assetId}" : null
                    },
                    ["Thumbnail"] = new System.Collections.Generic.Dictionary<string, object?>
                    {
                        ["Url"] = thumbUrl,
                        ["Final"] = true
                    },
                    ["Creator"] = new System.Collections.Generic.Dictionary<string, object?>
                    {
                        ["Name"] = creatorName,
                        ["CreatorProfileLink"] = $"/users/{creatorId}/profile"
                    },
                    ["UserItem"] = new System.Collections.Generic.Dictionary<string, object?>
                    {
                        ["IsRentalExpired"] = false
                    }
                };

                items.Add(itemObject);
            }

            var dataObject = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["Items"] = items,
                ["items"] = items,
                ["nextPageCursor"] = nextCursor?.ToString()
            };

            var response = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["isValid"] = true,
                ["Data"] = dataObject,
                ["data"] = dataObject
            };

            return new JsonResult(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["isValid"] = false,
                ["Data"] = ex.Message,
                ["data"] = ex.Message
            };

            return new JsonResult(errorResponse);
        }
    }

    private long GetCurrentUserId()
    {
        var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim))
            return 0;
        if (long.TryParse(idClaim, out var id))
            return id;
        return 0;
    }

    public class FollowRequest
    {
        public long? targetUserId { get; set; }
    }
}

