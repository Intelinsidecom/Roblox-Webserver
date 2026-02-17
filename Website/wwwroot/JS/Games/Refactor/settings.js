// Games/Refactor/settings.js
"use strict";
Roblox.GamesPage.Settings = function() {
    var n;
    this.adRefreshRateMilliSeconds = null, this.adsHidden = !1, this.adsInGameSearchResultsEnabled = !1, this.defaultWeeklyRatings = !1, this.deviceTypeId = 1, this.filterValueToGamesListsIdSuffixMapping = {
        "default": []
    }, this.gamesPageDefaultTopPaidToWeekly = !1, this.gamesSearchOnPage = !1, this.inApp = !1, this.incomingSearchQuery = null, this.isCreateNewAd = !0, this.isUserEligibleForMultirowFirstSort = !1, this.isUserLoggedIn = !1, this.minNumGamesForPersonalizedByLikedToAppear = 0, this.numGamesToFetchOnSearch = 40, this.numRowsForTopList = 4, this.columnWidthIncludingPadding = 161, this.extraHeightToShowHover = 50, this.interGamesListSpace = 40, this.gamesListHeaderHeight = 40, this.gamesListNarrowSquishHeight = 115, this.gamesListNarrowWidthThreshold = 544, this.gamesListSquishHeight = 51, this.gameTileHeight = 235, this.gameTilePadding = 36, this.gameTileNarrowHeight = 143, this.gameTileNarrowPadding = 36, this.pageHeaderHeight = 240, this.rowHeightIncludingPadding = 220, this.scrollbarWidth = 18, this.maxNumGamesToFetchPerRequest = 200, this.maxNumGamesToFetchInHScrollMode = 60, this.numGamesToAppendInHScrollMode = 12, this.numGamesToFetchInHScrollMode = 40, this.numGamesToFetchInHScrollModeSubsequently = 40, this.numGamesToFetchInMobileHScrollMode = 16, this.numGamesToFetchInVScrollMode = 40, this.numGamesToFetchInMultirowsMode = 40, this.numGamesToFetchInMultirowsModeSubsequently = 30, this.resizeGrowInterval = 300, this.resizeShrinkInterval = 50, this.pageTitle = null, this.refreshAdsInGamesPageEnabled = !1, this.searchDefaultKeyword = null, this.zeroResults = null, this.useFakeResults = !1, this.suggestedCorrection = "", this.suggestionKeyword = "", this.suggestionReplacedKeyword = "", this.isGamesThumbnailAsyncEnabled = !1, this.getSettingsElement = function() {
        return n || $("#games-page-settings")
    }, this.init = function() {
        var n = this.getSettingsElement(),
            r = Roblox.GamesPage.DataAttributes,
            t, i;
        for (t in r) i = r[t], this.hasOwnProperty(t) && typeof n.data(i) != "undefined" && (this[t] = n.data(i));
        n.data("filtervaluetogameslistsidsuffixmapping") && (this.filterValueToGamesListsIdSuffixMapping = {
            "default": n.data("filtervaluetogameslistsidsuffixmapping").split(",")
        })
    }
}, Roblox.GamesPage.DataAttributes = {
    adRefreshRateMilliSeconds: "adrefreshratemilliseconds",
    adsHidden: "adshidden",
    adsInGameSearchResultsEnabled: "adsingamesearchresultsenabled",
    defaultWeeklyRatings: "defaultweeklyratings",
    deviceTypeId: "devicetypeid",
    gamesPageDefaultTopPaidToWeekly: "gamespagedefaulttoppaidweekly",
    gamesSearchOnPage: "gamessearchonpage",
    inApp: "inapp",
    incomingSearchQuery: "incomingsearchquery",
    isCreateNewAd: "iscreatenewad",
    isUserEligibleForMultirowFirstSort: "isusereligibleformultirowfirstsort",
    isUserLoggedIn: "isuserloggedin",
    minNumGamesForPersonalizedByLikedToAppear: "minimumnumberofgamesforpersonalizedbylikedtoappear",
    pageTitle: "pagetitle",
    refreshAdsInGamesPageEnabled: "refreshadsingamespageenabled",
    searchDefaultKeyword: "searchdefaultkeyword",
    zeroResults: "zeroresults",
    useFakeResults: "usefakeresults",
    suggestedCorrection: "suggestedcorrection",
    suggestionKeyword: "suggestionkeyword",
    suggestionReplacedKeyword: "suggestionreplacedkeyword",
    isGamesThumbnailAsyncEnabled: "isgamesthumbnailasyncenabled"
};