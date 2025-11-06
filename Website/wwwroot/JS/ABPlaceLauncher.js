// ABPlaceLauncher.js
typeof Roblox == "undefined" && (Roblox = {});
var RobloxABLaunch = {
    launchGamePage: null,
    launcher: null
};
RobloxABLaunch.RequestGame = function(n, t, i) {
    RobloxPlaceLauncherService.LogJoinClick(), RobloxABLaunch.launcher === null && (RobloxABLaunch.launcher = new Roblox.ABPlaceLauncher), RobloxABLaunch.launcher.RequestGame(t, i)
}, RobloxABLaunch.RequestGroupBuildGame = function(n, t) {
    RobloxPlaceLauncherService.LogJoinClick(), RobloxABLaunch.launcher === null && (RobloxABLaunch.launcher = new Roblox.ABPlaceLauncher), RobloxABLaunch.launcher.RequestGroupBuildGame(t)
}, RobloxABLaunch.RequestGameJob = function(n, t, i) {
    RobloxPlaceLauncherService.LogJoinClick(), RobloxABLaunch.launcher === null && (RobloxABLaunch.launcher = new Roblox.ABPlaceLauncher), RobloxABLaunch.launcher.RequestGameJob(t, i)
}, RobloxABLaunch.StartGame = function(n, t, i, r) {
    i = i.replace("http://", "https://");
    try {
        typeof window.external != "undefined" && window.external.IsRobloxABApp && window.external.StartGame(r, i, n)
    } catch (f) {
        return !1
    }
    return !0
}, Roblox.ABPlaceLauncher = function() {}, Roblox.ABPlaceLauncher.prototype = {
    _onGameStatus: function(n) {
        if (n.status === 2) RobloxABLaunch.StartGame(n.joinScriptUrl, "Join", n.authenticationUrl, n.authenticationTicket);
        else if (n.status < 2 || n.status === 6) {
            var t = function(n, t) {
                    t._onGameStatus(n)
                },
                i = function(n, t) {
                    t._onGameError(n)
                },
                r = this,
                u = function() {
                    RobloxPlaceLauncherService.CheckGameJobStatus(n.jobId, t, i, r)
                };
            window.setTimeout(u, 2e3)
        }
    },
    _onGameError: function(n) {
        console.log("An error occurred. Please try again later -" + n)
    },
    _startUpdatePolling: function(n) {
        try {
            n()
        } catch (t) {
            n()
        }
    },
    RequestGame: function(n, t) {
        var u = function(n, t) {
                t._onGameStatus(n)
            },
            f = function(n, t) {
                t._onGameError(n)
            },
            e = this,
            i = !1,
            r;
        return typeof Party != "undefined" && typeof Party.AmILeader == "function" && (i = Party.AmILeader()), r = function() {
            RobloxPlaceLauncherService.RequestGame(n, i, t, u, f, e)
        }, this._startUpdatePolling(r), !1
    },
    RequestGroupBuildGame: function(n) {
        var t = function(n, t) {
                t._onGameStatus(n, !0)
            },
            i = function(n, t) {
                t._onGameError(n)
            },
            r = this,
            u = function() {
                RobloxPlaceLauncherService.RequestGroupBuildGame(n, t, i, r)
            };
        return this._startUpdatePolling(u), !1
    },
    RequestGameJob: function(n, t) {
        var i = function(n, t) {
                t._onGameStatus(n)
            },
            r = function(n, t) {
                t._onGameError(n)
            },
            u = this,
            f = function() {
                RobloxPlaceLauncherService.RequestGameJob(n, t, i, r, u)
            };
        return this._startUpdatePolling(f), !1
    },
    dispose: function() {
        Roblox.ABPlaceLauncher.callBaseMethod(this, "dispose")
    }
};