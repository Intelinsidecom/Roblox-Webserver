// Games/SponsoredGames/SponsoredGames.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.SponsoredGames == "undefined" && (Roblox.SponsoredGames = new function() {
    function t(n, t) {
        n.find(".game-card-thumb").attr("src", t.ThumbnailUrl), n.find(".game-card-link").attr("href", t.GameUrl), n.find(".game-card-name").attr("title", t.UniverseName).html(t.UniverseName), n.css("display", "inline-block")
    }

    function n(n) {
        $(n).remove()
    }

    function i(i) {
        for (var u = [], r = 0, f = i.length; r < f; r++) u.push({
            deviceType: i[r].deviceType,
            sortFilter: i[r].sortFilter,
            genreId: i[r].genreId,
            timeFilter: i[r].timeFilter,
            pageType: i[r].pageType,
            position: i[r].position
        });
        $.ajax({
            url: "/sponsored-games/serve-list",
            type: "POST",
            data: JSON.stringify(u),
            dataType: "json",
            contentType: "application/json",
            async: !0,
            success: function(r) {
                var u;
                if (r)
                    for (u = 0; u < i.length; u++) r[u] ? t(i[u].nativeAdPositionDom, r[u]) : n(i[u].nativeAdPositionDom);
                else
                    for (u = 0; u < i.length; u++) n(i[u].nativeAdPositionDom)
            },
            error: function() {
                for (var t = 0; t < i.length; t++) n(i[t].nativeAdPositionDom)
            }
        })
    }

    function r(n) {
        var t = [];
        $(n.find(".sponsored-game")).each(function() {
            if (!$(this).find("a").attr("href")) {
                var n = $(this),
                    i = {
                        nativeAdPositionDom: n,
                        deviceType: Roblox.GamesPageContainerBehavior.getDeviceTypeId(),
                        sortFilter: n.data("sort-filter"),
                        genreId: n.data("genre-filter"),
                        timeFilter: n.data("time-filter"),
                        pageType: n.data("page-type"),
                        position: n.data("position")
                    };
                t.push(i)
            }
        }), t.length > 0 && Roblox.SponsoredGames.serveNativeAds(t)
    }
    return {
        serveNativeAds: i,
        getSponsoredGames: r
    }
});