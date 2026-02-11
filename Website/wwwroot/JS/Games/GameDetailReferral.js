// Games/GameDetailReferral.js
Roblox = Roblox || {}, Roblox.GameDetailReferral = Roblox.GameDetailReferral || {}, Roblox.GameDetailReferral.AppendUrl = function(n, t, i) {
    if (!n.data("modified")) {
        var e = $(t).length,
            u = i || n,
            f = u.attr("href"),
            r = f;
        r += f.indexOf("?") !== -1 ? "&" : "?", r += "LocalTimestamp=" + (new Date).toISOString() + "&TotalInSort=" + e, u.attr("href", r), n.data("modified", !0)
    }
}, $(function() {
    var t = ".game-card-link",
        n = "mousedown touchstart";
    $("#recently-visited-places").on(n, ".game-card", function() {
        Roblox.GameDetailReferral.AppendUrl($(this).find(t), "#recently-visited-places .list-item")
    });
    $("#my-favorites-games").on(n, ".game-card", function() {
        Roblox.GameDetailReferral.AppendUrl($(this).find(t), "#my-favorites-games .game-card")
    });
    $("#friend-activity").on(n, ".game-card", function() {
        Roblox.GameDetailReferral.AppendUrl($(this).find(t), "#friend-activity .game-card")
    });
    $("#UserPlaces div.Thumbnail").on(n, function() {
        Roblox.GameDetailReferral.AppendUrl($(this), ".Thumbnail", $(this).find("a"))
    });
    $("#GamesListsContainer").on(n, ".game-card", function() {
        var n = $(this).parent().siblings().length + 1;
        Roblox.GameDetailReferral.AppendUrl($(this).find(t), new Array(n))
    });
    $("#my-recommended-games").on(n, ".game-card", function() {
        Roblox.GameDetailReferral.AppendUrl($(this).find(t), "#my-recommended-games .game-card")
    });
    $("#HomeContainer #FeaturedGamesContainer").on(n, ".item-place a", function() {
        Roblox.GameDetailReferral.AppendUrl($(this), "#FeaturedGamesContainer .item-place")
    })
});