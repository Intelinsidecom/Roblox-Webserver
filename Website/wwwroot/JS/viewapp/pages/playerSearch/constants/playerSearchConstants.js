// ~/viewapp/pages/playerSearch/constants/playerSearchConstants.js
"use strict";
playerSearch.constant("playerSearchConstants", {
    layout: {
        statuses: {
            game: "game",
            online: "online",
            studio: "studio"
        },
        userInfo: {
            game: "inGame",
            studio: "inStudio",
            group: "primaryGroup"
        },
        friendship: {},
        inMobile: !1,
        isUserGuest: !1,
        resultsInPage: 0,
        resultsStart: 0,
        loadingImageUrl: "",
        resultsLoading: !1,
        unsafeInputDetected: !1,
        isKeywordTooShort: !1
    },
    pageData: {
        metaDataLink: "",
        keyword: null,
        totalResults: 0,
        maxRows: 0,
        startIndex: 0,
        initialized: !1,
        inApp: !0,
        inMobileOrTabletBrowser: !1,
        keywordMinLength: 0
    }
});