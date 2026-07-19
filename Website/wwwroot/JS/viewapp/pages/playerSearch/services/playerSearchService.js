// ~/viewapp/pages/playerSearch/services/playerSearchService.js
"use strict";
playerSearch.factory("playerSearchService", ["$log", "$q", "_", "httpService", "urlService", function(n, t, i, r, u) {
    function l(i, e) {
        if (!i) return f;
        var s = {
                url: u.getAbsoluteUrl(i)
            },
            h = {
                keyword: e
            };
        return r.httpGet(s, h).then(function(i) {
            return i.hasOwnProperty("Keyword") ? (n.debug("got metadata: ", i), f = i) : typeof i == "string" && i.indexOf(y) ? t.reject(o) : t.reject("Exception occured " + i)
        })
    }

    function a(t) {
        if (!f.Links || !f.Links.Search) return !1;
        var i = {
                url: f.Links.Search,
                noCache: !0
            },
            u = {
                keyword: t.keyword,
                startIndex: t.startIdx || 0,
                maxRows: t.numResults || 12
            };
        return r.httpGet(i, u).then(function(t) {
            return (n.debug("got results: ", t), t.TotalResults > 0) ? p(t) : t
        })
    }

    function p(n) {
        var o = n.UserSearchResults,
            r, u;
        return i.each(o, function(n, t) {
            f.CurrentUserId === n.UserId && (n.YourOwnResult = !0), n.sortOrder = t
        }), r = i.map(o, function(n) {
            return n.UserId
        }), u = {
            avatar: e(r)
        }, f.IsGuest ? u.presence = h(r) : u.relationAndPresence = s(r), t.all(u).then(function(t) {
            var r = i.indexBy(o, "UserId");
            return i.each(t, function(n) {
                i.map(n, function(n) {
                    return i.extend(r[n.UserId], n)
                })
            }), n.processedResult = i.sortBy(i.values(r), "sortOrder"), n
        })
    }

    function c(n) {
        if (!f.Links || !f.Links.Friendship) return !1;
        var t = {
                url: f.Links.Friendship,
                noCache: !0
            },
            i = {
                userIds: n
            };
        return r.httpGet(t, i).then(function(n) {
            return n.PlayerRelationships
        })
    }

    function h(n) {
        if (!f.Links || !f.Links.Presence) return !1;
        var t = {
                url: f.Links.Presence,
                noCache: !0
            },
            i = {
                userIds: n
            };
        return r.httpGet(t, i).then(function(n) {
            return n.PlayerPresences
        })
    }

    function s(n) {
        if (!f.Links || !f.Links.RelationAndPresence) return !1;
        var t = {
                url: f.Links.RelationAndPresence,
                noCache: !0
            },
            u = {
                userIds: n
            };
        return r.httpGet(t, u).then(function(n) {
            return i.isArray(n.PlayerPresences) && i.isArray(n.PlayerRelationships) ? n.PlayerPresences.concat(n.PlayerRelationships) : []
        })
    }

    function e(n) {
        if (!f.Links || !f.Links.Avatars) return !1;
        var t = {
                url: f.Links.Avatars
            },
            i = {
                userIds: n,
                isHeadshot: f.IsPhone || !1
            };
        return r.httpGet(t, i).then(function(n) {
            return n.PlayerAvatars
        })
    }

    function v(n) {
        if (!f.Links || !f.Links.AddFriend) return !1;
        var t = {
                url: f.Links.AddFriend
            },
            i = {
                targetUserID: n
            };
        return r.httpPost(t, i)
    }

    function w(n, t) {
        if (!f.Links || !f.Links.AcceptFriendRequest) return !1;
        var i = {
                url: f.Links.AcceptFriendRequest
            },
            u = {
                targetUserID: n,
                invitationID: t
            };
        return r.httpPost(i, u)
    }
    var f = {},
        y = "/Error/UnsafeInput",
        o = "unsafeInput";
    return {
        getSearchResults: a,
        getUserFriendship: c,
        getUserPresence: h,
        getUserRelationshipAndPresence: s,
        getUserAvatar: e,
        getMetaData: l,
        addFriend: v,
        acceptFriend: w,
        unsafeInputText: o
    }
}]);