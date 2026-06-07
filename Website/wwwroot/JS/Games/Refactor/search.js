// Games/Refactor/search.js
"use strict";
if (typeof Roblox === 'undefined') {
    var Roblox = {};
}
Roblox.GamesPage.Search = function() {
    var n, t, i, r, u;
    return {
        buildSearchRequest: function(n, t) {
            var f = Roblox.AdsHelper.GamesPage,
                e = Roblox.GamesPage,
                i = this.getSettings(),
                u = this.getState(),
                r = null;
            return Roblox.ExperimentalMode && (Roblox.ExperimentalMode.getVariant() != "B" || Roblox.ExperimentalMode.isExperimentalModeEnabled() ? Roblox.ExperimentalMode.getVariant() == "A" && (r = Roblox.ExperimentalMode.loadingExperimental() ? !1 : !0) : r = !0), {
                StartRows: n,
                MaxRows: t,
                IsUserLoggedIn: i.isUserLoggedIn,
                NumberOfColumns: u.numberOfColumns,
                IsInHorizontalScrollMode: e.isInMultiViewMode(),
                DeviceTypeId: i.deviceTypeId,
                Keyword: u.currentSearchQuery,
                AdSpan: u.adSpan,
                AdAlignment: f.calcAdAlignment(),
                v: 2,
                IsSecure: r,
                UseFakeResults: i.useFakeResults,
                SuggestedCorrection: i.suggestedCorrection,
                SuggestionKeyword: i.suggestionKeyword,
                SuggestionReplacedKeyword: i.suggestionReplacedKeyword
            }
        },
        emptySearchResultsGameCards: function() {
            this.getSearchResultsGameCardsContainer().empty();
        },
        emptyExperimentalSearchResultsGameCards: function() {
            this.getExperimentalSearchResultsGameCardsContainer().empty()
        },
        getFilteredKeywordFromResponse: function(n) {
            if (!n) return null;
            var t = $(n);
            return !t.length || t.attr("id") !== "keyword" ? null : t.text()
        },
        getSearchContainer: function() {
            return n || (n = $("#GamesPageSearch")), n
        },
        getSearchResultsContainer: function() {
            return t || (t = $("#SearchResultsContainer")), t
        },
        getSearchResultsGameCardsContainer: function() {
            return i || (i = $("#search-results-game-cards")), i
        },
        getExperimentalSearchResultsGameCardsContainer: function() {
            return r || (r = $("#experimental-search-results-game-cards")), r
        },
        getSearchResultsQueryTextElement: function() {
            return u || (u = $("#SearchResultsContainer .search-query-text")), u
        },
        getSettings: function() {
            return Roblox.GamesPage.settings
        },
        getState: function() {
            return Roblox.GamesPage.getState()
        },
        init: function() {
            this.initSearchQuery()
        },
        initSearchQuery: function() {
            var t = this.getState(),
                n = this.getSettings();
            n.gamesSearchOnPage && n.incomingSearchQuery && (t.currentSearchQuery = n.incomingSearchQuery)
        },
        isSearchInProgress: function() {
            return this.getState().currentSearchInProgress
        },
        resetDomSelectors: function() {
            n = undefined, t = undefined, i = undefined, r = undefined, u = undefined
        },
        search: function() {
            var f, e, o, n, t = Roblox.AdsHelper.GamesPage,
                i = Roblox.GamesPageConstants,
                r = Roblox.GamesPage,
                h = this.getSettings(),
                u = this.getState(),
                s = h.numGamesToFetchOnSearch,
                c = r.getURLBasedOnSortFilter();
            if (this.isSearchInProgress()) return !1;
            this.setSearchInProgress(!0), n = u.currentSearchPage * s, f = n < 1, f ? Roblox.ExperimentalMode && Roblox.ExperimentalMode.getVariant() == "A" && Roblox.ExperimentalMode.loadingExperimental() ? (this.emptyExperimentalSearchResultsGameCards(), this.setSearchResultsQueryText(u.currentSearchQuery), this.toggleSearch("on"), r.calculateNumberOfColumns(), t.updateAdSpan(i.initialAdHeight)) : (this.emptySearchResultsGameCards(), this.setSearchResultsQueryText(u.currentSearchQuery), this.toggleSearch("on"), r.calculateNumberOfColumns(), t.updateAdSpan(i.initialAdHeight)) : t.updateAdSpan(i.subsequentAdHeight), o = this.buildSearchRequest(n, s), e = this.showSearchResults.bind(this, n), $.get(c, o, e)
        },
        setSearchInProgress: function(n) {
            var i = this.getSearchResultsContainer(),
                t = this.getState();
            n ? t.currentSearchInProgress || (i.addClass("search-pending"), t.currentSearchInProgress = !0) : (i.removeClass("search-pending"), t.currentSearchInProgress = !1)
        },
        setSearchResultsQueryText: function(n) {
            this.getSearchResultsQueryTextElement().text(n)
        },
        setSearchStateOff: function() {
            var t = Roblox.GamesPageConstants,
                n = Roblox.GamesPage.Filters,
                i = this.getState();
            i.searchState = t.searchState.off, n.setFiltersVisible(!0), n.setFiltersEnabled(!0), this.getSearchResultsQueryTextElement().text(""), this.getSearchResultsContainer().removeClass("search-pending"), Roblox.AdsHelper.GamesPage.checkAdDisplayState()
        },
        setSearchStateOn: function() {
            var r = Roblox.GamesPageConstants,
                t = Roblox.GamesPage.Filters,
                n = Roblox.GamesPage,
                i = this.getState();
            i.searchState = r.searchState.on, t.setFiltersVisible(!1), t.setFiltersEnabled(!1), n.hideGamesLists(), this.showSearchResultsContainer(), i.isInMultiViewMode = !1, n.setInfiniteScroll(!0), n.updateURLFromSearchState()
        },
        showSearchResults: function(n, t) {
            var f = Roblox.AdsHelper.GamesPage,
                e, h = n < 1,
                u = Roblox.GamesPage,
                i = 0,
                o = this.getSearchResultsGameCardsContainer(),
                c = this.getExperimentalSearchResultsGameCardsContainer(),
                s = this.getSettings(),
                l = this.getState(),
                a = "<strong>" + s.zeroResults + "</strong>",
                r = $("<div></div>"),
                v;
            Roblox.ExperimentalMode && Roblox.ExperimentalMode.getVariant() == "B" && !Roblox.ExperimentalMode.isExperimentalModeEnabled() && (a = "<span class='secure-results-empty'><strong>" + s.zeroResults + ". See more results by clicking<span class='icon-experimental-gray1'></span>above.</strong></span>"), t && (i = $(t).find(".game-card-container").length), i ? (r.append(t), l.currentSearchPage += 1) : h && (r.append(a), e = this.getFilteredKeywordFromResponse(t), e && this.setSearchResultsQueryText(e)), Roblox.ExperimentalMode && Roblox.ExperimentalMode.getVariant() == "A" ? Roblox.ExperimentalMode.loadingExperimental() ? c.append(r.children()) : o.append(r.children()) : o.append(r.children()), Roblox.ExperimentalMode && Roblox.ExperimentalMode.getVariant() == "A" && Roblox.ExperimentalMode.loadingExperimental() ? Roblox.SponsoredGames.getSponsoredGames(c) : Roblox.SponsoredGames.getSponsoredGames(o), f.checkAdDisplayState(), this.setSearchInProgress(!1), h && (f.refreshAds(), i && !u.isInMultiViewMode() && this.search()), u.isInMultiViewMode() || f.populateNewAds(), u.updateVotes(), u.fetchThumbnails(), v = s.numGamesToFetchOnSearch, (!i || i < v) && Roblox.ExperimentalMode && Roblox.ExperimentalMode.getVariant() == "A" && !Roblox.ExperimentalMode.loadingExperimental() && (Roblox.ExperimentalMode.setLoadingExperimental(!0), l.currentSearchPage = 0, this.search(), this.getExperimentalSearchResultsGameCardsContainer().show(), $(".experimental-games-container").show())
        },
        showSearchResultsContainer: function() {
            this.getSearchResultsContainer().removeClass("overflow-hidden").addClass("overflow-visible").removeClass("hidden")
        },
        toggleSearch: function(n) {
            switch (n) {
                case "on":
                    this.setSearchStateOn();
                    break;
                case "reset":
                    this.setSearchStateOff()
            }
        }
    }
}();