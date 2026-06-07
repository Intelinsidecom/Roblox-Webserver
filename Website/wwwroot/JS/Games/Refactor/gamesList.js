// Games/Refactor/gamesList.js
Roblox = Roblox || {}, Roblox.GamesPage = Roblox.GamesPage || {}, Roblox.GamesPage.GamesList = function(n, t, i, r, u, f) {
    var e = Roblox.GamesPageConstants;
    this.parent = Roblox.GamesPage, this.divId = n, this.id = n.replace("GamesListContainer", ""), this.container = null, this.gamesListElement = null, this.haveGamesBeenLoaded = !1, this.isFirst = !1, this.isShown = !1, this.sortFilter = t, this.gameFilter = i, this.regionFilter = r, this.timeFilter = e.timeFilters.current, this.genreId = 1, this.minBcLevel = u, this.personalizedUniverseId = f, this.numberOfRowsToOccupy = 0, this.numberOfGamesToFetch = 0, this.numberOfGamesOnScreen = 0, this.startIndex = 0, this.jqxhr = null, this.reachedHorizontalScrollMax = !1, this.numberOfGamesOnLastRow = 0, this.isSearchResults = !1, this.getContainer().hasClass("search-results-container") && (this.isSearchResults = !0)
}, Roblox.GamesPage.GamesList.prototype = {
    bindFilterClick: function() {
        var n = this.handleGamesFilterChangerClick.bind(this);
        this.getContainer().on("click", ".games-filter-changer", n)
    },
    bindScrollHandlers: function() {
        var n = this.handleHorizontalScroll.bind(this);
        this.getContainer().on("click", ".scroller", function() {
            var t = $(this).hasClass("prev") ? "prev" : "next";
            n(t)
        })
    },
    buildFetchRequest: function(n, t) {
        var u = this.parent.isInMultiViewMode(),
            i = this.getSettings(),
            r = this.getState();
        return {
            SortFilter: this.sortFilter,
            TimeFilter: this.timeFilter,
            RegionFilter: this.regionFilter,
            GenreID: this.genreId,
            GameFilter: this.gameFilter,
            MinBCLevel: this.minBcLevel,
            StartRows: this.startIndex,
            MaxRows: n,
            IsUserLoggedIn: i.isUserLoggedIn,
            NumberOfRowsToOccupy: this.numberOfRowsToOccupy,
            NumberOfColumns: r.numberOfColumns,
            IsInHorizontalScrollMode: u,
            DeviceTypeId: i.deviceTypeId,
            AdSpan: r.adSpan,
            AdAlignment: t,
            v: 2,
            PersonalizedUniverseId: this.personalizedUniverseId,
            useFakeResults: i.useFakeResults,
            IsGamesThumbnailAsyncEnabled: i.isGamesThumbnailAsyncEnabled
        }
    },
    clearGamesListHeight: function() {
        var n = this.getRowsClass(this.numberOfRowsToOccupy);
        this.getGamesListElement().removeClass(n)
    },
    doesNextButtonFitContainer: function(n, t, i, r) {
        var f = n / t,
            u;
        return f = isNaN(f) ? 0 : Math.floor(f), r = !r || isNaN(parseInt(r)) ? 0 : Math.abs(parseInt(r)), u = r / t, u = isNaN(u) ? 0 : Math.floor(u), i - u <= f
    },
    emptyGamesList: function(n) {
        var i = this.getSettings();
        n.empty();
    },
    getClosestColumnBoundary: function(n) {
        var t = this.getColumnWidth(),
            i = Math.abs(n % t);
        return n + i
    },
    getColumnWidth: function() {
        return this.getContainer().find(".list-item.game-card").first().outerWidth(!0)
    },
    getContainer: function() {
        return this.container || (this.container = $("#" + this.divId)), this.container
    },
    getGamesListElement: function() {
        return this.gamesListElement || (this.gamesListElement = this.getContainer().find(".games-list")), this.gamesListElement
    },
    getLeftBoundaryOfLastVisibleColumn: function() {
        var n, t, i = this.getSettings();
        return n = this.getContainer().find(".games-list").width(), t = Math.floor(n / i.columnWidthIncludingPadding), t * i.columnWidthIncludingPadding
    },
    getNumGamesToFetch: function() {
        var t, n = this.getSettings(),
            i = this.getState();
        return t = this.parent.isInMultiViewMode() ? this.numberOfRowsToOccupy > 1 ? this.haveGamesBeenLoaded ? n.numGamesToFetchInMultirowsModeSubsequently : Math.max(n.numGamesToFetchInMultirowsMode, (i.numberOfColumns + 1) * this.numberOfRowsToOccupy) : this.haveGamesBeenLoaded ? n.numGamesToFetchInHScrollModeSubsequently : this.parent.isTouch() ? n.numGamesToFetchInMobileHScrollMode : n.numGamesToFetchInHScrollMode : n.numGamesToFetchInVScrollMode, Math.min(t, n.maxNumGamesToFetchPerRequest)
    },
    getRowsClass: function(n) {
        return n > 0 ? "rows-" + n : null
    },
    getScroller: function() {
        return this.getContainer().find(".horizontally-scrollable")
    },
    getScrollerNext: function() {
        return this.getContainer().find(".scroller.next")
    },
    getScrollerPrev: function() {
        return this.getContainer().find(".scroller.prev")
    },
    getScrollerMultiView: function() {
        return this.getContainer().find(".games-list .show-in-multiview-mode-only")
    },
    getSettings: function() {
        return Roblox.GamesPage.settings
    },
    getState: function() {
        return Roblox.GamesPage.state
    },
    handleGamesFilterChangerClick: function() {
        var t = Roblox.AdsHelper.GamesPage,
            i = Roblox.GamesPageConstants,
            r = Roblox.GamesPage.Filters;
        return r.handleFilterChange(i.sortFilters.sortFilter, this.sortFilter), this.parent.updateURLFromPageState(), this.parent.loadInitialGames(!0), t.checkAdDisplayState(), !1
    },
    handleHorizontalScroll: function(n) {
        var f = this.getSettings(),
            h = this.parent.isInMultiViewMode ? this.getScrollerMultiView() : null,
            u = this.getScroller(),
            i, t, r, e, o, s;
        u.length > 0 && ($(u).stop("", !0, !0), i = parseInt($(u).css("left")) || 0, i = this.getClosestColumnBoundary(i), r = this.getScrollerNext(), e = this.getScrollerPrev(), n === "prev" ? (t = i + this.getLeftBoundaryOfLastVisibleColumn(), t = Math.min(t, 0), r.removeClass("disabled")) : r.hasClass("disabled") || (this.loadGames(f.numGamesToAppendInHScrollMode), t = i - this.getLeftBoundaryOfLastVisibleColumn(), (this.noMoreGamesToLoad || this.reachedHorizontalScrollMax) && !this.isAvailableWidthFullyOccupied(t) && r.addClass("disabled")), f.isGamesThumbnailAsyncEnabled && Roblox.LazyLoadImg && (o = function() {
            Roblox.LazyLoadImg.updateImageScroll(h)
        }), $(u).animate({
            left: t + "px"
        }, {
            complete: o
        }), f.refreshAdsInGamesPageEnabled && this.parent.refreshAds(), s = t < 0, s ? e.removeClass("disabled") : r.hasClass("disabled") || e.addClass("disabled"))
    },
    hide: function() {
        this.isShown = !1, this.getContainer().addClass("hidden")
    },
    hideLoadingIndicator: function() {
        this.getContainer().removeClass("cursor-wait")
    },
    hideOverflow: function() {
        this.getContainer().removeClass("overflow-visible"), this.getContainer().addClass("overflow-hidden")
    },
    init: function() {
        this.bindScrollHandlers(), this.bindFilterClick(), this.preventScrollerDoubleClick()
    },
    initScrollers: function() {
        var n = this.parent.isInMultiViewMode();
        n && this.numberOfRowsToOccupy > 1 ? this.getContainer().find(".scroller").remove() : (this.reachedHorizontalScrollMax = !1, this.getScrollerNext().removeClass("hidden"))
    },
    isAvailableWidthFullyOccupied: function(n) {
        var t, i, r, u;
        return t = this.getColumnWidth(), i = this.getGamesListElement().width(), r = Math.ceil(i / t), u = Math.abs(n / t), this.numberOfGamesOnScreen >= u + r
    },
    loadGames: function(n) {
        var t, i, r = Roblox.AdsHelper.GamesPage,
            f = Roblox.GamesPageConstants,
            e, o, l = Roblox.GamesPage.Filters,
            s = this.startIndex < 1,
            h = this.parent.isInMultiViewMode(),
            u, c;
        if (s) this.jqxhr && this.jqxhr.abort();
        else if (this.jqxhr || h && this.reachedHorizontalScrollMax) return;
        c = this.parent.getURLBasedOnSortFilter(this.sortFilter), u = n || this.getNumGamesToFetch(), this.timeFilter = l.getDefaultTimeFilterForSortFilter(this.sortFilter, this.timeFilter), s ? (r.checkAdDisplayState(), this.numberOfGamesOnLastRow = 0, t = 0, i = f.initialAdHeight, this.initScrollers()) : (t = r.calcAdAlignment(), i = f.subsequentAdHeight), r.updateAdSpan(i), o = this.buildFetchRequest(u, t), e = this.renderGamesFetched.bind(this, u), h || this.showLoadingIndicator(), this.jqxhr = $.get(c, o, e)
    },
    preventScrollerDoubleClick: function() {
        var n = this.getContainer();
        n.find(".scroller, .scroller .arrow, .scroller .arrow img").on("dblclick", function(n) {
            return n.preventDefault(), window.getSelection ? window.getSelection().removeAllRanges() : document.selection && document.selection.empty(), !1
        })
    },
    renderGamesFetched: function(n, t) {
        var u = Roblox.AdsHelper.GamesPage,
            i, f = this.startIndex < 1,
            e = this.parent.isInMultiViewMode(),
            r, c, o, s = this.getSettings(),
            h;
        this.jqxhr = null, o = $(t).find(".game-card-container").length, o && (this.haveGamesBeenLoaded = !0, this.startIndex += n), h = $("<div></div>").append(t), Roblox.SponsoredGames.getSponsoredGames(h), e ? (i = this.getScroller().find(".games"), this.numberOfRowsToOccupy > 1 && (r = i.find(".multi-rows"), r.length || (r = $("<div/>").addClass("multi-rows"), i.append(r)), i = r)) : i = this.getGamesListElement().find(".game-cards-grid"), f && this.emptyGamesList(i), i.append(h.children()), this.numberOfGamesOnScreen = $(i).find(".game-card-container").length, e ? (this.reachedHorizontalScrollMax = this.numberOfGamesOnScreen > s.maxNumGamesToFetchInHScrollMode, f && (this.numberOfGamesOnScreen < s.minNumGamesForPersonalizedByLikedToAppear && this.personalizedUniverseId !== 0 && this.getContainer().hide(), this.resetHorizontalScroller(), o && (this.noMoreGamesToLoad = this.numberOfGamesOnScreen < n, c = this.getNumGamesToFetch(), this.noMoreGamesToLoad || this.reachedHorizontalScrollMax || !c || this.loadGames())), this.toggleCarouselButtons()) : f && this.numberOfGamesOnScreen > s.numGamesToFetchInVScrollMode && this.loadGames(), u.checkAdDisplayState(), u.populateNewAds(), u.refreshOldAds(), this.hideLoadingIndicator(), this.parent.updateVotes(), e ? this.parent.fetchThumbnails(this.getScrollerMultiView()) : this.parent.fetchThumbnails()
    },
    reset: function() {
        this.startIndex = 0, this.haveGamesBeenLoaded = !1, this.parent.isInMultiViewMode() && this.resetHorizontalScroller()
    },
    resetHorizontalScroller: function() {
        var n = this.getScroller();
        n.length > 0 && $(n).css("left", 0), this.getScrollerPrev().addClass("disabled"), this.getScrollerNext().removeClass("disabled")
    },
    setHeight: function() {
        var n = Roblox.GamesPageConstants,
            i = this.getGamesListElement(),
            t, r = this.getSettings(),
            u = this.getState(),
            f, e, o, s;
        this.isShown ? u.isInMultiViewMode ? (this.hideOverflow(), t = this.isFirst && r.isUserEligibleForMultirowFirstSort ? r.numRowsForTopList : this.getContainer().data("numberofrows"), this.numberOfRowsToOccupy = t, this.setGamesListHeight()) : (this.clearGamesListHeight(), this.showOverflow(), e = this.parent.getGamesListsContainerHeight(), o = i ? Math.round(i.offset().top) : 0, f = Math.max($(window).height(), e) - (o + n.rowHeightOffset), s = Math.floor(f / n.rowHeightIncludingPadding), this.numberOfRowsToOccupy = Math.max(1, Math.floor(2 * s))) : this.hideOverflow(), this.isSearchResults && u.searchState === n.searchState.on && this.showOverflow()
    },
    setGamesListHeight: function() {
        var n = this.getRowsClass(this.numberOfRowsToOccupy);
        this.getGamesListElement().addClass(n)
    },
    show: function() {
        this.parent.isInMultiViewMode() ? (this.getGamesListElement().find(".game-card-container,.games-list-column,.in-game-search-ad, .list-item.game-card").remove(), this.getContainer().find(".show-in-multiview-mode-only").removeClass("hidden")) : this.getContainer().find(".show-in-multiview-mode-only").addClass("hidden"), this.isShown = !0, this.getContainer().removeClass("hidden")
    },
    showLoadingIndicator: function() {
        this.getContainer().addClass("cursor-wait")
    },
    showOverflow: function() {
        this.clearGamesListHeight(), this.getContainer().removeClass("overflow-hidden"), this.getContainer().addClass("overflow-visible")
    },
    toggleCarouselButtons: function() {
        var t = this.getScroller().css("left"),
            i = this.numberOfGamesOnScreen,
            r = this.getContainer().find(".list-item").first().outerWidth(!0),
            u = this.doesNextButtonFitContainer(this.getContainer().width(), r, i, t);
        this.getContainer().find(".scroller.next").toggleClass("disabled", u)
    }
};