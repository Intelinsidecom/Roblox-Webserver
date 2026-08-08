// Games/Refactor/ads.js
var Roblox = Roblox || {};
Roblox.AdsHelper = Roblox.AdsHelper || {}, Roblox.AdsHelper.AdRefresher = Roblox.AdsHelper.AdRefresher || {}, Roblox.AdsHelper.DynamicAdCreator = Roblox.AdsHelper.DynamicAdCreator || {}, Roblox.AdsHelper.GamesPage = function() {
    var n = {
        leftGutterId: "LeftGutterAdContainer",
        rightGutterId: "RightGutterAdContainer",
        adDiv1Id: "GamePageAdDiv1",
        adDiv2Id: "GamePageAdDiv2",
        adDiv3Id: "GamePageAdDiv3",
        leaderboardId: "Leaderboard-Abp",
        bindFilmStripHidden: function() {
            $(document).on("FilmStripHidden", this.handleFilmStripHidden.bind(this))
        },
        bindGuttersHidden: function() {
            $(document).on("GuttersHidden", this.handleGuttersHidden.bind(this))
        },
        calcAdAlignment: function() {
            var n = this.getAdsInSearchResults().last(),
                t = 0;
            return n.length > 0 && n.hasClass("ad-order-even") && (t = 1), t
        },
        calcAdSpan: function(n, t, i) {
            var r, u, f = this.getSettings();
            return Roblox.GamesPage.isInMultiViewMode() || !f.adsInGameSearchResultsEnabled ? 0 : (u = Math.ceil(i / n), r = u * t)
        },
        getAdDiv3: function() {
            return $("#" + this.adDiv3Id)
        },
        getAdsInSearchResults: function() {
            return $(".overflow-visible .in-game-search-ad")
        },
        getGutterAdStyles: function() {
            return $("#GutterAdStyles")
        },
        getGutterIframeUrl: function() {
            return Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("games/rightcolumn") : "games/rightcolumn"
        },
        getGutterIframe: function() {
            var n = this.getGutterIframeUrl();
            return "<iframe id='GamesRightColumn' src='" + n + "' scrolling='no' frameBorder='0' style='height:550px;width:330px;border:0px;overflow:hidden'></iframe>"
        },
        getLeftGutter: function() {
            return $("#" + this.leftGutterId)
        },
        getRightGutter: function() {
            return $("#" + this.rightGutterId)
        },
        getSettings: function() {
            return Roblox.GamesPage.settings
        },
        getState: function() {
            return Roblox.GamesPage.state
        },
        getTopAdContainer: function() {
            return $("#" + this.leaderboardId)
        },
        handleFilmStripHidden: function() {
            var t = Roblox.GamesPage.getRightSidebarElement(),
                n;
            this.getAdDiv3().hide(), n = this.getGutterIframe(), t.html(n)
        },
        handleGuttersHidden: function() {
            var n = this.getState(),
                i;
            if (n.guttersEnabled) {
                var t = Roblox.AdsHelper.AdRefresher,
                    r = Roblox.GamesPage.getLeftColumnElement(),
                    u = Roblox.GamesPage.getRightSidebarElement();
                n.guttersEnabled = !1, t.registerAd && (t.registerAd(this.adDiv1Id), t.registerAd(this.adDiv2Id)), this.getLeftGutter().hide(), this.getRightGutter().hide(), r.addClass("gutters-hidden"), this.getGutterAdStyles().remove(), i = this.getGutterIframe(), u.html(i)
            }
        },
        init: function() {
            if (Roblox.GamesPage) {
                var n = Roblox.AdsHelper.AdRefresher,
                    i = this.getLeftGutter(),
                    r = this.getTopAdContainer(),
                    t = this.getSettings();
                t && t.adsInGameSearchResultsEnabled && (googletag && (googletag.pubads && googletag.pubads() ? googletag.pubads().disableInitialLoad() : googletag.cmd.push(function() {
                    googletag.pubads().disableInitialLoad()
                })), i.length ? (n.registerAd && (n.registerAd(this.leftGutterId), n.registerAd(this.rightGutterId))) : (n.registerAd && (n.registerAd(this.adDiv1Id), n.registerAd(this.adDiv2Id), n.registerAd(this.adDiv3Id))), r.length && n.registerAd && n.registerAd(this.leaderboardId), this.bindGuttersHidden(), this.bindFilmStripHidden())
            }
        },
        populateNewAds: function() {
            Roblox.GamesPage && Roblox.GamesPage.settings.adsInGameSearchResultsEnabled && !Roblox.GamesPage.isInMultiViewMode() && Roblox.AdsHelper.DynamicAdCreator.populateNewAds()
        },
        refreshOldAds: function() {
            Roblox.GamesPage && Roblox.GamesPage.settings.adsInGameSearchResultsEnabled && !Roblox.GamesPage.isInMultiViewMode() && (this.setHiddenAdsToSkipRefresh(), this.populateNewAds())
        },
        refreshAds: function() {
            var n = Roblox.GamesPage.settings,
                t = Roblox.GamesPage.state;
            if (n.isCreateNewAd) n.isCreateNewAd = !1, t.setIntervalId && clearInterval(t.setIntervalId), t.setIntervalId = setInterval(function() {
                n.isCreateNewAd = !0
            }, n.adRefreshRateMilliSeconds), this.refreshOldAds();
            else return !0
        },
        checkAdDisplayState: function() {
            if (Roblox && Roblox.GamesPage && Roblox.GamesPage.settings) {
                var n = Roblox.GamesPage.settings.adsInGameSearchResultsEnabled && !Roblox.GamesPage.isInMultiViewMode();
                return n ? Roblox.GamesPage.getGamesPageContainer().addClass("ads-in-game-search") : Roblox.GamesPage.getGamesPageContainer().removeClass("ads-in-game-search"), !0
            }
            return !1
        },
        setHiddenAdsToSkipRefresh: function() {
            $(".overflow-visible .in-game-search-ad").each(function() {
                var i = $(this);
                i.children(".ad-slot").length > 0 && i.children(".ad-slot").html().trim() == "" && !i.hasClass("unpopulated") && (i.addClass("unpopulated"), i.children(".ad-slot").attr("id", "adx" + Math.floor(Math.random() * 1e8)))
            })
        },
        updateAdSpan: function(n) {
            var t = this.getState();
            t.adSpan = this.calcAdSpan(Roblox.GamesPageConstants.rowHeightIncludingPadding, t.numberOfColumns, n)
        }
    };
    return $(function() {
        n.init()
    }), n
}();