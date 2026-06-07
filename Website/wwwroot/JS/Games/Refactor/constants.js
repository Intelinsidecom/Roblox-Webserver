// Games/Refactor/constants.js
"use strict";
if (typeof Roblox === 'undefined') {
    var Roblox = {};
}
Roblox.GamesPageConstants = {
    initialAdHeight: 1326,
    subsequentAdHeight: 800,
    rowHeightIncludingPadding: 220,
    rowHeightOffset: 20,
    numberOfGamesToInitiallyFetchInHScrollMode: 14,
    numberOfGamesToPrefetchAfterInitialFetchInHScrollMode: 14,
    numberOfGamesToInitiallyFetchInMobileHScrollMode: 16,
    numberOfGamesToFetchInVScrollMode: 60,
    maxNumberOfGamesToFetchInHScrollMode: 60,
    numberOfGamesToInitiallyFetchInMultirowsMode: 40,
    numberOfGamesToFetchAfterInitialFetchInMultirowsMode: 30,
    searchState: {
        on: "on",
        off: "off"
    },
    sortFilters: {
        sortFilter: "SortFilter",
        timeFilter: "TimeFilter",
        genreFilter: "GenreFilter"
    },
    sortFilterTypeIds: {
        popular: 1,
        topFavorite: 2,
        featured: 3,
        myFavorite: 5,
        myRecent: 6,
        experimental: 7,
        topEarning: 8,
        topPaid: 9,
        purchased: 10,
        topRated: 11,
        buildersClub: 14,
        topRetaining: 16,
        friendActivity: 17,
        personalizedByLiked: 18,
        popularInVR: 19,
        popularInCountry: 20
    },
    timeFilters: {
        current: 0,
        daily: 1,
        weekly: 2,
        allTime: 4
    },
    genreFilter: {
        all: 0
    },
    urls: {
        moreResultsCached: "/games/moreresultscached",
        moreResultsUncached: "/games/moreresultsuncached"
    }
}, Roblox.GamesPageConstants.userSpecificSortFilters = [Roblox.GamesPageConstants.sortFilterTypeIds.myFavorite, Roblox.GamesPageConstants.sortFilterTypeIds.myRecent, Roblox.GamesPageConstants.sortFilterTypeIds.experimental, Roblox.GamesPageConstants.sortFilterTypeIds.purchased, Roblox.GamesPageConstants.sortFilterTypeIds.friendActivity, Roblox.GamesPageConstants.sortFilterTypeIds.personalizedByLiked];