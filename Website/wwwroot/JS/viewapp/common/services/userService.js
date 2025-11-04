// ~/viewapp/common/services/userService.js
robloxAppService.factory("userService", ["$http", function(n) {
    function i(i) {
        var r = t("/thumbnail/avatar-headshot"),
            u = {
                userId: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }

    function r(i) {
        var r = t("/presence/user"),
            u = {
                userId: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }

    function u(i) {
        var r = t("/thumbnail/avatar-headshots"),
            u = {
                userIds: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }

    function f(i) {
        var r = t("/presence/users"),
            u = {
                userIds: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }
    var t = function(n) {
        return Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl(n) : n
    };
    return {
        getUserAvatar: i,
        getUserPresence: r,
        getMultiUserAvatar: u,
        getMultiUserPresence: f
    }
}]);