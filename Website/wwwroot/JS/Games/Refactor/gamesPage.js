// Games/Refactor/gamesPage.js
"use strict";
var Roblox = Roblox || {};
Roblox.GamesPage = function() {
    var r = Roblox.GamesPageConstants,
        t, i, n, f, u = {
            lastWindowWidth: null,
            lastWindowHeight: null,
            lastRightSidebarTop: null,
            lastScrollTop: 0,
            state: null,
            settings: null,
            adjustGamesListsHeights: function() {
                for (var t = this.getState(), n = 0; n < t.gamesListArray.length; n++) t.gamesListArray[n].setHeight()
            },
            bindEvents: function() {
                this.bindResize(), this.bindScroll(), this.bindVotes()
            },
            bindHistory: function() {
                var n = this.handleURLChange.bind(this),
                    t = this.history;
                t.Adapter.bind(window, "statechange", function() {
                    n()
                })
            },
            bindResize: function() {
                $(window).on("resize", this.handleResize.bind(this))
            },
            bindScroll: function() {
                $(window).on("scroll", $.proxy(this.handleScroll, this))
            },
            bindVotes: function() {
                var n = this.getGamesPageContainer();
                if (n && !Roblox.DeviceFeatureDetection.isTouch) n.find(".game-cards .game-card").on({
                    mouseover: function() {
                        $(this).find(".vote-container .vote-percentage").addClass("hover")
                    },
                    mouseout: function() {
                        $(this).find(".vote-container .vote-percentage").removeClass("hover")
                    }
                })
            },
            calculateNumberOfColumns: function() {
                var i, f = Roblox.GamesPageConstants,
                    n, t, r = this.getState(),
                    u;
                n = r.searchState === f.searchState.on ? this.getGamesListElements().filter(":visible").last() : this.getGamesListElements().filter(":visible").first(), i = n.width(), t = $("<div></div>").addClass("game-card").addClass("hidden"), n.find(".game-cards-grid").append(t), u = t.outerWidth(!0), t.remove(), r.numberOfColumns = Math.floor(i / u)
            },
            fetchThumbnails: function(n) {
                var t = this.getSettings(),
                    i = 300;
                $("[data-retry-url]").loadRobloxThumbnails(), t.isGamesThumbnailAsyncEnabled && Roblox.LazyLoadImg && Roblox.LazyLoadImg.init(i, n)
            },
            getGamesPageContainer: function() {
                return n || (n = $("#ResponsiveWrapper")), n
            },
            getGamesListElements: function() {
                return t || (t = this.getGamesListsContainer().find(".games-list-container")), t
            },
            getGamesListObject: function(n) {
                for (var i = this.getState(), t = 0; t < i.gamesListArray.length; t++)
                    if (i.gamesListArray[t].divId === "GamesListContainer" + n) return i.gamesListArray[t];
                return null
            },
            getGamesListsContainer: function() {
                return i || (i = $("#GamesListsContainer")), i
            },
            getGamesListsContainerHeight: function() {
                var n = this.getGamesListsContainer();
                return n && n.length ? parseInt(n.css("min-height")) : 0
            },
            getLeftColumnElement: function() {
                return $("#GamesPageLeftColumn")
            },
            getRightSidebarElement: function() {
                return $("#GamesPageRightColumnSidebar")
            },
            getSettings: function() {
                return this.settings
            },
            getState: function() {
                return this.state
            },
            getURLBasedOnSortFilter: function(n) {
                return $.inArray(n, r.userSpecificSortFilters) !== -1 ? r.urls.moreResultsUncached : r.urls.moreResultsCached
            },
            getURLStringAndObjectFromPageState: function() {
                var t = Roblox.GamesPage.Filters,
                    n = t.getFiltersFromDOM(),
                    i = "?" + $.param(n);
                return {
                    urlStateString: i,
                    urlStateObject: n
                }
            },
            handleResize: function() {
                var t = $(window).width(),
                    n = $(window).height(),
                    i = this.getSettings(),
                    r = this.getState();
                this.updateCarouselButtons(), t > this.lastWindowWidth || n > this.lastWindowHeight ? setTimeout(this.handleResizeGrow.bind(this), i.resizeGrowInterval) : (t < this.lastWindowWidth || n < this.lastWindowHeight) && setTimeout(this.handleResizeShrink.bind(this), i.resizeShrinkInterval), this.lastWindowWidth = t, this.lastWindowHeight = n, r.distanceFromBottomAtWhichToLoadMoreGames = n / 2
            },
            handleResizeGrow: function() {
                this.adjustGamesListsHeights(), !this.isInMultiViewMode() && this.shouldLoadMoreGames() && this.loadGames()
            },
            handleResizeShrink: function() {
                this.isInMultiViewMode() && this.adjustGamesListsHeights()
            },
            handleScroll: function() {
                var t = Roblox.GamesPageConstants,
                    n = window.scrollY,
                    i = Roblox.GamesPage.Search,
                    r = this.getState();
                window.scrollY || (n = document.documentElement.scrollTop), this.isInMultiViewMode() || n > this.lastScrollTop && this.shouldLoadMoreGames() && (r.searchState === t.searchState.on ? i.search() : this.loadGames()), this.lastScrollTop = n
            },
            handleURLChange: function() {
                var u = Roblox.AdsHelper.GamesPage,
                    e = Roblox.GamesPage.Filters,
                    n, i = Roblox.GamesPage.Search,
                    t = this.getState(),
                    f = !1,
                    r;
                if ((n = this.history.getState().data, !t.areFiltersAlreadyUpdated) && Object.keys(n).length !== 0) {
                    if (n.hasOwnProperty("keyword")) t.currentSearchQuery = n.keyword, i.getSearchResultsQueryTextElement().text() !== t.currentSearchQuery && i.search();
                    else {
                        i.toggleSearch("reset"), t.isURLAlreadyUpdated = !0;
                        for (r in n) e.handleFilterChange(r, n[r]);
                        t.isURLAlreadyUpdated = !1, t.haveGamesBeenFetched && (f = !0), this.loadInitialGames(f)
                    }
                    u.checkAdDisplayState(), u.refreshOldAds()
                }
            },
            hideGamesLists: function() {
                for (var t = this.getState(), n = 0; n < t.gamesListArray.length; n++) t.gamesListArray[n].hide();
                t.numberOfShownGamesLists = 0
            },
            init: function() {
                var n = Roblox.AdsHelper.GamesPage;
                this.initHistory(), this.bindHistory(), this.initSettings(), this.initState(), this.initSubmodules(), this.bindEvents(), document.title = this.getSettings().pageTitle, this.initGamesLists(), this.initURL(), this.saveCurrentDimensions(), this.state.haveGamesBeenFetched || this.handleURLChange(), n.checkAdDisplayState()
            },
            initGamesLists: function() {
                var t, i, r, u, f, n, e, o = this.state;
                this.getGamesListElements().each(function() {
                    t = $(this).attr("id"), i = $(this).data("sortfilter"), r = $(this).data("gamefilter"), u = $(this).data("regionfilter"), f = $(this).data("minbclevel"), e = $(this).data("personalized-universe-id"), n = new Roblox.GamesPage.GamesList(t, i, r, u, f, e), n.init(), o.gamesListArray.push(n)
                }), o.gamesListArray[0].isFirst = !0
            },
            initHistory: function() {
                this.history = window.History
            },
            initSettings: function() {
                this.settings = new Roblox.GamesPage.Settings, this.settings.init()
            },
            initState: function() {
                this.state = new Roblox.GamesPage.State
            },
            initSubmodules: function() {
                var n = Roblox.GamesPage.Filters,
                    t = Roblox.GamesPage.Search;
                t.init(), n.init()
            },
            initURL: function() {
                var n = {},
                    t = this.getSettings();
                t.gamesSearchOnPage && t.incomingSearchQuery !== "" ? this.updateURLFromSearchState() : (n = this.getURLStringAndObjectFromPageState(), this.history.replaceState(n.urlStateObject, t.pageTitle, n.urlStateString))
            },
            isInMultiViewMode: function() {
                return this.state.isInMultiViewMode
            },
            isTouch: function() {
                return Roblox.DeviceFeatureDetection ? Roblox.DeviceFeatureDetection.isTouch : !1
            },
            loadGames: function() {
                for (var t = this.state, n = 0; n < t.gamesListArray.length; n++) t.gamesListArray[n].isShown && t.gamesListArray[n].loadGames()
            },
            loadInitialGames: function(n) {
                var t = this.state;
                Roblox.GamesPage.calculateNumberOfColumns(), this.loadGames(), t.haveGamesBeenFetched = !0, n && this.refreshAds()
            },
            refreshAds: function() {
                var t = Roblox.AdsHelper.GamesPage,
                    n;
                t.refreshAds(), n = $("#GamesRightColumn")[0], n && n.contentWindow && n.contentWindow.Roblox && n.contentWindow.Roblox.RightColumnBehavior && n.contentWindow.Roblox.RightColumnBehavior.refreshAds()
            },
            resetDomSelectors: function() {
                n = null, i = null, t = null, n = null, f = null
            },
            resetGamesLists: function() {
                for (var t = this.state, n = 0; n < t.gamesListArray.length; n++) t.gamesListArray[n].reset()
            },
            saveCurrentDimensions: function() {
                var n = this.getRightSidebarElement();
                this.lastWindowWidth = $(window).width(), this.lastWindowHeight = $(window).height(), n.length && (this.lastRightSidebarTop = n.position().top)
            },
            setInfiniteScroll: function(n) {
                n ? (this.getGamesPageContainer().addClass("games-lists-single"), this.getGamesPageContainer().removeClass("games-lists-multiple")) : (this.getGamesPageContainer().removeClass("games-lists-single"), this.getGamesPageContainer().addClass("games-lists-multiple"))
            },
            shouldLoadMoreGames: function() {
                var n = this.getState();
                return $(window).scrollTop() >= $(document).height() - $(window).height() - n.distanceFromBottomAtWhichToLoadMoreGames
            },
            showGamesList: function(n) {
                var i = this.getState(),
                    t = this.getGamesListObject(n);
                t && (t.show(), i.numberOfShownGamesLists++)
            },
            updateCarouselButtons: function() {
                for (var t = this.getState(), n = 0; n < t.gamesListArray.length; n++) t.gamesListArray[n].toggleCarouselButtons()
            },
            updateURLFromPageState: function() {
                var n = this.getURLStringAndObjectFromPageState(),
                    t = this.getSettings();
                this.state.areFiltersAlreadyUpdated = !0, this.history.pushState(n.urlStateObject, t.pageTitle, n.urlStateString), this.state.areFiltersAlreadyUpdated = !1
            },
            updateURLFromSearchState: function() {
                var n = this.state.currentSearchQuery,
                    t = this.getSettings(),
                    i = {
                        keyword: n
                    },
                    r = t.pageTitle,
                    u = "?Keyword=" + encodeURIComponent(n);
                this.history.pushState(i, r, u)
            },
            updateVotes: function() {
                var n = $("[data-voting-processed=false]");
                n.each(function(n, t) {
                    var i = $(t),
                        r = i.find(".vote-container"),
                        u = parseInt(r.attr("data-upvotes")),
                        f = parseInt(r.attr("data-downvotes"));
                    Roblox.Voting.UpdateVoteBar(u, f, i), i.attr("data-voting-processed", !0)
                })
            }
        };
    return $(function() {
        u.init()
    }), u
}();