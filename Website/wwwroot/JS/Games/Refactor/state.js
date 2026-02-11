// Games/Refactor/state.js
"use strict";
Roblox.GamesPage.State = function() {
    this.adSpan, this.areAdsInGameSearchResults = !1, this.areFiltersAlreadyUpdated = !1, this.currentSearchInProgress = !1, this.currentSearchPage = 0, this.currentSearchQuery = null, this.distanceFromBottomAtWhichToLoadMoreGames = 400, this.gamesListArray = [], this.guttersEnabled = !1, this.haveGamesBeenFetched = !1, this.History = window.History, this.isGameSearchOnPage = !1, this.isInMultiViewMode = !0, this.isURLAlreadyUpdated = !1, this.numberOfColumns = 0, this.numberOfShownGamesLists = 0, this.searchState = "off", this.setIntervalId = null
};