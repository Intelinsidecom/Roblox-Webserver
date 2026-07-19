// ~/viewapp/pages/playerSearch/controllers/playerSearchController.js
"use strict";
playerSearch.controller("playerSearchController", ["_", "$location", "$log", "$scope", "$state", "$window", "adsService", "cardLabels", "chatDispatchService", "playerSearchService", "playerSearchConstants", "urlService", function(n, t, i, r, u, f, e, o, s, h, c, l) {
    function p(n, t) {
        r.pageData.keyword = n.Keyword, r.formData.keyword = n.Keyword, y(t), v(t.processedResult || [], n.Keyword), a(), r.pageData.initialized || (r.pageData.initialized = !0)
    }

    function w() {
        r.results = [], r.mobilePageData.resultCount = 0, r.layout.unsafeInputDetected = !1, r.pageData.keyword = null, r.pageData.totalResults = 0, r.layout.isKeywordTooShort = !1
    }

    function a() {
        r.layout.resultsLoading = !r.layout.resultsLoading
    }

    function b() {
        r.pageData.adsInitialized || (r.pageData.adsInitialized = !0, e.registerAd(e.adIds.leaderboardAbp))
    }

    function y(t) {
        var i = h.getMetaData(),
            o = {
                startIndex: r.layout.inMobile ? 0 : t.StartIndex || 0,
                totalResults: t.TotalResults || 0,
                maxRows: i.MaxRows,
                currentUserId: i.CurrentUserId,
                inApp: i.InApp,
                inAndroidApp: i.InAndroidApp,
                iniOSApp: i.IniOSApp,
                inMobileOrTabletBrowser: (i.IsPhone || i.IsTablet) && !i.InApp,
                keywordMinLength: i.KeywordMinLength
            };
        n.extend(r.pageData, o);
        var u = r.pageData.startIndex,
            f = r.pageData.maxRows,
            e = r.pageData.totalResults,
            s = {
                resultsInPage: Math.min(e, u + f),
                resultsStart: u + 1,
                curPage: Math.ceil(u / f) + 1,
                numPages: Math.ceil(e / f)
            };
        n.extend(r.layout, s), r.layout.inMobile && (r.mobilePageData.resultCount += r.pageData.maxRows)
    }

    function v(t, u) {
        n.each(t, function(t) {
            if (t.IsOnline && (t.StatusClass = r.layout.statuses.online, t.InGame ? t.StatusClass = r.layout.statuses.game : t.InStudio && (t.StatusClass = r.layout.statuses.studio)), t.PreviousUserNamesCsv.length > 0) {
                var i = u.toLowerCase();
                t.MatchingPreviousName = n.chain(t.PreviousUserNamesCsv.split(",")).map(function(n) {
                    return n.trim().toLowerCase()
                }).find(function(n) {
                    return n.indexOf(i) === 0 && n !== t.Name
                }).value()
            }
        }), r.results = r.layout.inMobile ? r.results.concat(t) : t, i.debug("my data", r.results)
    }
    r.layout = n.extend({}, c.layout), r.pageData = n.extend({}, c.pageData), r.mobilePageData = {
        resultCount: 0
    }, r.formData = {
        keyword: ""
    }, r.results = [], r.labelToShow = function(n) {
        return n.YourOwnResult && (r.layout.inMobile || r.pageData.inApp) ? o.yourself : n.MatchingPreviousName ? o.aka : n.FriendshipStatus !== r.layout.friendship.Friends || n.YourOwnResult ? n.IsFollowed ? o.following : o.presence : o.friends
    }, r.getUserInfo = function(n) {
        if (r.layout.inMobile) return "";
        var t = r.layout.userInfo;
        return n.InGame ? t.game : n.InStudio ? t.studio : n.PrimaryGroup.length > 0 ? t.group : ""
    }, r.showButtonsForFriends = function(n) {
        return n.FriendshipStatus === r.layout.friendship.Friends && !n.YourOwnResult && !r.layout.isUserGuest
    }, r.showButtonsForNonFriends = function(n) {
        return n.FriendshipStatus !== r.layout.friendship.Friends && !n.YourOwnResult && !r.layout.isUserGuest
    }, r.isMobileButtonHidden = function(n) {
        return r.layout.isUserGuest || n.FriendshipStatus === r.layout.friendship.Friends && !n.IsOnline
    }, r.showNoMatches = function() {
        return r.pageData.totalResults < 1 && !r.layout.resultsLoading && !r.layout.isKeywordTooShort && !r.layout.unsafeInputDetected
    }, r.openProfile = function(n, t) {
        n && (f.location.href = l.getAbsoluteUrl(n)), t.preventDefault(), t.stopPropagation()
    }, r.addFriend = function(t) {
        return h.addFriend(t).then(function(u) {
            u.success ? n.find(r.results, function(n) {
                return n.UserId === t ? (n.FriendshipStatus = r.layout.friendship.PendingOnOtherUser, !0) : !1
            }) : i.debug("add friend failed: ", u.message)
        })
    }, r.acceptFriend = function(t, u) {
        return h.acceptFriend(t, u).then(function(u) {
            u.success ? n.find(r.results, function(n) {
                return n.UserId === t ? (n.FriendshipStatus = r.layout.friendship.Friends, !0) : !1
            }) : i.debug("accept friend failed: ", u.message)
        })
    }, r.startChat = function(n) {
        s.startChat(n, r.pageData)
    }, r.joinGame = function(n) {
        if (n && n.FollowToGameScript && n.FollowToGameScript.length > 0) {
            var t = new Function(n.FollowToGameScript);
            t()
        }
    }, r.adRefresh = function() {
        e.refreshAllAds()
    }, r.startNewSearch = function(i) {
        i && i.target && i.target.blur();
        var u = t.search();
        n.extend(u, {
            keyword: r.formData.keyword,
            startIndex: 0
        }), t.search(u)
    }, r.pageModelChanged = function() {
        return r.pageData.keyword + "_" + r.layout.curPage
    }, r.pageUpdate = function(n) {
        i.debug("new page: ", n);
        var u = (n - 1) * r.pageData.maxRows;
        t.search("startIndex", u)
    }, r.getNextScrollResults = function() {
        var t = r.results.length < r.pageData.totalResults,
            n;
        !r.layout.resultsLoading && t && (i.debug("scroll called"), a(), n = {
            keyword: r.pageData.keyword,
            startIdx: r.mobilePageData.resultCount
        }, h.getSearchResults(n).then(function(n) {
            y(n), v(n.processedResult || [], r.pageData.keyword), a()
        }))
    }, r.init = function(u, f, e) {
        var c, o, s;
        a(), b(), f && f.length > 0 ? (c = f, o = {
            keyword: f,
            startIdx: e || 0
        }) : (s = t.search(), c = s.keyword || "", o = {
            keyword: s.keyword || "",
            startIdx: s.startIndex || 0
        }), r.pageData.metaDataLink = u, w(), h.getMetaData(u, c).then(function(t) {
            r.layout.friendship = n.object(t.FriendshipStatusValues, t.FriendshipStatusValues), r.layout.inMobile = t.IsPhone, r.layout.isUserGuest = t.IsGuest, r.layout.loadingImageUrl = t.LoadingImageUrl, o.keyword.length < t.KeywordMinLength ? (p(t, {}), r.layout.isKeywordTooShort = !0) : h.getSearchResults(o).then(function(n) {
                p(t, n)
            })
        }, function(n) {
            i.debug("Error: ", n), n === h.unsafeInputText && (a(), w(), r.layout.unsafeInputDetected = !0)
        })
    }, r.$on("$locationChangeSuccess", function(n, u, f) {
        var e, o;
        i.debug("url changed: ", n, u, f), r.pageData.initialized && (e = t.search(), e.keyword !== r.pageData.keyword ? r.init(r.pageData.metaDataLink, e.keyword, e.startIndex) : (o = {
            keyword: e.keyword,
            startIdx: e.startIndex
        }, a(), h.getSearchResults(o).then(function(n) {
            y(n), v(n.processedResult || [], e.keyword), a()
        })), r.adRefresh())
    })
}]);