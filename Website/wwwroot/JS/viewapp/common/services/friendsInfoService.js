// ~/viewapp/common/services/friendsInfoService.js
"use strict";
robloxApp.factory("friendsInfoService", ["$document", "$timeout", "httpService", "$filter", "$q", "localStorageService", "localStorageNames", function(n, t, i, r, u, f, e) {
    var o = function() {
        function l() {
            this.isInitialized = !1
        }

        function v(n) {
            for (var f = [], t = 0, o = {
                    url: Roblox.EnvironmentUrls.presenceApi + "/v1/presence/users",
                    retryable: !0,
                    withCredentials: !0
                }, r = n.slice(t, s), e; r.length > 0;) e = {
                userIds: r
            }, f.push(i.httpPost(o, e)), t++, r = n.slice(t * s, t * s + s);
            return u.all(f)
        }

        function a(i) {
            if (i) n.on("Roblox.Presence.Update", function(n, r) {
                r && t(function() {
                    r.forEach(function(n) {
                        var t = n.userId;
                        i[t] && (i[t].presence = n)
                    }), f.saveDataByTimeStamp(e.friendsDict, i)
                })
            })
        }

        function y() {
            n.on("Roblox.Logout", function() {
                f.removeLocalStorage(e.friendsDict)
            })
        }

        function p(n, t) {
            var l, s, p, h, w;
            return t && t.isEnabled && (l = f.fetchNonExpiredCachedData(e.friendsDict, t.expirationMS), l) ? (s = l.data, o = s, c = !0, a(s), p = u.defer(), p.resolve(s), p.promise) : (h = {
                url: Roblox.EnvironmentUrls.friendsApi + "/v1/users/{userId}/friends",
                retryable: !0,
                withCredentials: !0
            }, w = "/users/{userId}/profile", h.url = r("formatString")(h.url, {
                userId: Roblox.CurrentUser.userId
            }), i.httpGet(h).then(function(n) {
                var u = n.data ? n.data : n,
                    i = [];
                return angular.forEach(u, function(n) {
                    var t = n.id;
                    i.push(t), n.profileUrl = r("formatString")(w, {
                        userId: t
                    }), o[t] = n
                }), v(i).then(function(n) {
                    if (n && n.length > 0) {
                        var i = [];
                        angular.forEach(n, function(n) {
                            var t = n.userPresences;
                            i = i.concat(t)
                        }), i.forEach(function(n) {
                            o[n.userId].presence = n
                        })
                    }
                    return t && t.isEnabled && (f.saveDataByTimeStamp(e.friendsDict, o), a(o), y()), c = !0, o
                })
            }))
        }
        var h = [],
            o = {},
            s = 100,
            c = !1;
        return l.prototype.register = function(n, t) {
            typeof n == "function" && h.push(n), this.isInitialized ? c && n(o) : (this.isInitialized = !0, Roblox.CurrentUser && p(h, t).then(function(n) {
                h.forEach(function(t) {
                    t(n)
                })
            }))
        }, l
    }();
    return Roblox.sharedFriendsService || (Roblox.sharedFriendsService = new o), Roblox.sharedFriendsService
}]);