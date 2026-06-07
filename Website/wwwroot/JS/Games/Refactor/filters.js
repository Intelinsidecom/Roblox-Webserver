// Games/Refactor/filters.js
"use strict";
if (typeof Roblox === 'undefined') {
    var Roblox = {};
}
Roblox.GamesPage.Filters = function() {
    var n = Roblox.GamesPageConstants,
        t, r, u, f;
    return {
        selectorFilterSelected: ".rbx-selection-label",
        sortFiltersForWhichGenreFiltersShouldBeHidden: [],
        sortFiltersForWhichTimeFiltersShouldBeHidden: [],
        bindFilterSelectActions: function() {
            var t, u = this.handleFilterChange.bind(this),
                r = Roblox.GamesPage,
                i, n, f = this.getState();
            this.getFilterElements().each(function() {
                $(this).on("change", function() {
                    t = $(this).attr("id"), i = $(this).find("select"), n = i && i.length ? $(this).find("option:checked").data("value") : $(this).find("[data-bind='label']").data("value");
                    n && t && (u(t, n), f.isURLAlreadyUpdated || r.updateURLFromPageState(), r.loadInitialGames(!0))
                })
            })
        },
        bindFilterSelectBehavior: function() {
            $('.input-group-btn ul[data-toggle="dropdown-menu"] li').click(function(n) {
                var t = $(n.currentTarget);
                t.closest(".input-group-btn").find('[data-bind="label"]').data("value", t.data("value")).end().end().trigger("change")
                
               
                setTimeout(function() {
                    var dropdownButton = t.closest(".input-group-btn").find('.input-dropdown-btn[data-toggle="dropdown"]');
                    var dropdownContainer = t.closest(".input-group-btn");

                    if (dropdownContainer.hasClass('open') || dropdownContainer.hasClass('show')) {
                        if (dropdownButton.length && typeof dropdownButton.dropdown !== 'undefined') {
                            dropdownButton.dropdown('toggle');
                        } else {
                            dropdownContainer.removeClass('open show');
                        }
                    }
                }, 10); 
            }), $('.games-selection [data-toggle="selection"]').change(function(n) {
                var t = $(n.currentTarget),
                    i = $(this).find("option:selected").data("value");
                t.parent().find('[data-bind="label"]').data("value", i)
            })
        },
        filtersObjectCorrectTimeFilter: function(n) {
            var t = Roblox.GamesPageConstants;
            typeof n[t.sortFilters.timeFilter] != "undefined" && n[t.sortFilters.timeFilter] === t.timeFilters.current && typeof n[t.sortFilters.sortFilter] != "undefined" && this.isSortFilterNeedingWeeklyDefault(n[t.sortFilters.sortFilter]) && (n[t.sortFilters.timeFilter] = t.timeFilters.weekly)
        },
        filtersObjectAppendBC: function(n) {
            var r = this.getSettings(),
                u = this.getState(),
                i = u.gamesListArray,
                t;
            if (!r.isInMultiViewMode)
                for (t = 0; t < i.length; t++) i[t].isShown && i[t].minBCLevel === 1 && (n.BC = 1);
            return n
        },
        getDefaultTimeFilterForSortFilter: function(n, t) {
            var i = Roblox.GamesPageConstants,
                r = this.getTimeFilterContainer().find(this.selectorFilterSelected).data("default"),
                u = Roblox.GamesPage.isInMultiViewMode(),
                f = this.isSortFilterNeedingWeeklyDefault(n);
            return f ? (t === i.timeFilters.current || u) && (t = i.timeFilters.weekly) : (r === i.timeFilters.current || t === i.timeFilters.current) && (t = r), t
        },
        getFiltersContainer: function() {
            return t || (t = $("#FiltersAndSort")), t
        },
        getFilterElements: function() {
            return $(".games-page-filters .games-page-filter")
        },
        getFiltersFromDOM: function() {
            var n = {},
                t = this.selectorFilterSelected;
            return this.getFilterElements().each(function() {
                var i = $(this).attr("id"),
                    r = $(this).find(t).data("value");
                typeof r != "undefined" && i && !$(this).hasClass("hidden") && (n[i] = r)
            }), this.filtersObjectCorrectTimeFilter(n), this.filtersObjectAppendBC(n), n
        },
        getGenreFilterContainer: function() {
            return $("#" + n.sortFilters.genreFilter)
        },
        getSettings: function() {
            return Roblox.GamesPage.settings
        },
        getFilterElementByType: function(n) {
            var t = Roblox.GamesPageConstants,
                i = [t.sortFilters.genreFilter, t.sortFilters.sortFilter, t.sortFilters.timeFilter];
            return $.inArray(n, i) !== -1 ? $("#" + n) : null
        },
        getSortFilterContainer: function() {
            return $("#" + n.sortFilters.sortFilter)
        },
        getState: function() {
            return Roblox.GamesPage.state
        },
        getTimeFilterContainer: function() {
            return $("#" + n.sortFilters.timeFilter)
        },
        handleFilterChange: function(n, t) {
            var i = Roblox.GamesPageConstants,
                r = Roblox.GamesPage;
            r.resetGamesLists();
            switch (n) {
                case i.sortFilters.sortFilter:
                    this.toggleFiltersEnabled(t), this.handleSortFilterChange(t);
                    break;
                case i.sortFilters.timeFilter:
                    this.handleTimeFilterChange(t);
                    break;
                case i.sortFilters.genreFilter:
                    this.handleGenreFilterChange(t)
            }
            this.setFilterGroupButtonValue(n, t)
        },
        handleGamesFilterResetClick: function() {
            var t = Roblox.GamesPage;
            Roblox.GamesPage.Search.toggleSearch("reset"), this.handleFilterChange("SortFilter", "default"), t.updateURLFromPageState(), t.loadInitialGames(!0)
        },
        handleGenreFilterChange: function(n) {
            for (var i = this.getState(), t = 0; t < i.gamesListArray.length; t++) i.gamesListArray[t].genreId = n
        },
        handleSortFilterChange: function(n) {
            var t = Roblox.GamesPage;
            t.hideGamesLists(), this.showGamesLists(n), this.resetTimeFilter(n), this.resetGenreFilter()
        },
        handleTimeFilterChange: function(n) {
            for (var i = this.getState(), t = 0; t < i.gamesListArray.length; t++) i.gamesListArray[t].timeFilter = n
        },
        init: function() {
            var n = Roblox.GamesPageConstants,
                t, i, r;
            i = this.getSortFilterContainer().find(this.selectorFilterSelected).data("default"), this.setFilterGroupButtonValue(n.sortFilters.sortFilter, i), t = this.getGenreFilterContainer().find(this.selectorFilterSelected).data("default"), this.setFilterGroupButtonValue(n.sortFilters.genreFilter, t), r = this.getTimeFilterContainer().find(this.selectorFilterSelected).data("default"), this.setFilterGroupButtonValue(n.sortFilters.timeFilter, r), this.initSortFiltersHidden(), this.bindFilterSelectActions(), this.bindFilterSelectBehavior()
        },
        initSortFiltersHidden: function() {
            var n = this.getSortFilterContainer(),
                t = this.sortFiltersForWhichTimeFiltersShouldBeHidden,
                i = this.sortFiltersForWhichGenreFiltersShouldBeHidden;
            n.find("[data-hidetimefilter]").each(function(n, i) {
                t.push($(i).data("value"))
            }), n.find("[data-hidegenrefilter]").each(function(n, t) {
                i.push($(t).data("value"))
            })
        },
        isSortFilterNeedingWeeklyDefault: function(t) {
            var i = this.getSettings(),
                r = i.defaultWeeklyRatings,
                u = i.gamesPageDefaultTopPaidToWeekly;
            return r && t === n.sortFilterTypeIds.topRated || u && t === n.sortFilterTypeIds.topPaid || t === n.sortFilterTypeIds.experimental || t === n.sortFilterTypeIds.buildersClub || t === n.sortFilterTypeIds.topFavorite || t === n.sortFilterTypeIds.topRetaining ? !0 : !1
        },
        resetTimeFilter: function(n) {
            var t = Roblox.GamesPageConstants;
            this.isSortFilterNeedingWeeklyDefault(n) ? this.setFilterGroupButtonValue(t.sortFilters.timeFilter, t.timeFilters.weekly) : this.setFilterGroupButtonValue(t.sortFilters.timeFilter, t.timeFilters.current)
        },
        resetGenreFilter: function() {
            var t = Roblox.GamesPageConstants;
            this.setFilterGroupButtonValue(t.sortFilters.genreFilter, t.genreFilter.all)
        },
        setFiltersEnabled: function(n) {
            n ? this.getFiltersContainer().removeClass("disabled") : this.getFiltersContainer().addClass("disabled")
        },
        setFiltersVisible: function(n) {
            n ? (this.getSortFilterContainer().removeClass("hidden"), this.getTimeFilterContainer().removeClass("hidden"), this.getGenreFilterContainer().removeClass("hidden")) : (this.getSortFilterContainer().addClass("hidden"), this.getTimeFilterContainer().addClass("hidden"), this.getGenreFilterContainer().addClass("hidden"))
        },
        setFilterGroupButtonValue: function(n, t) {
            var u = this.getFilterElementByType(n),
                i, r, f;
            if (f = u.find(this.selectorFilterSelected), r = u.find("select"), r && r.length && (i = r.find("option").filter(function() {
                    return $(this).data("value") === t
                }), i && i.length)) return i.attr("selected", !0), f.data("value", i.data("value")), !0;
            r = u.find("ul.dropdown-menu"), r.length && (i = r.find("li").filter(function() {
                return $(this).data("value") === t
            }), i && i.length && f.text(i.text().trim()).data("value", i.data("value")))
        },
        showGamesLists: function(n) {
            var t = Roblox.GamesPage,
                f = this.getSettings(),
                u = this.getState(),
                r = f.filterValueToGamesListsIdSuffixMapping,
                i;
            if (r.hasOwnProperty(n)) {
                for (u.isInMultiViewMode = !0, i = 0; i < r[n].length; i++) t.showGamesList(r[n][i]);
                t.setInfiniteScroll(!1)
            } else u.isInMultiViewMode = !1, t.showGamesList(n), t.setInfiniteScroll(!0);
            t.adjustGamesListsHeights()
        },
        toggleFiltersEnabled: function(n) {
            var t = this.getGenreFilterContainer(),
                i = this.getTimeFilterContainer();
            $.inArray(n, this.sortFiltersForWhichTimeFiltersShouldBeHidden) !== -1 ? i.addClass("hidden") : i.removeClass("hidden"), $.inArray(n, this.sortFiltersForWhichGenreFiltersShouldBeHidden) !== -1 ? t.addClass("hidden") : t.removeClass("hidden"), this.getFiltersContainer().removeClass("loading")
        }
    }
}();