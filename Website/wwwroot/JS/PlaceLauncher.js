// PlaceLauncher.js
var RBX = {},
    RobloxLaunchStates = {
        StartingServer: "StartingServer",
        StartingClient: "StartingClient",
        Upgrading: "Upgrading",
        None: "None"
    },
    RobloxLaunch = {
        launchGamePage: "/install/download.aspx",
        launcher: null,
        googleAnalyticsCallback: function() {
            RobloxLaunch._GoogleAnalyticsCallback && RobloxLaunch._GoogleAnalyticsCallback()
        },
        state: RobloxLaunchStates.None,
        protocolNameForClient: "",
        protocolNameForStudio: "",
        protocolUrlSeparator: ""
    },
    RobloxPlaceLauncherService = {
        LogJoinClick: function() {
            $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "LogJoinClick"
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done()
        },
        RequestGame: function(n, t, i, r, u, f) {
            i = i !== null && i !== undefined ? i : "", $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "RequestGame",
                    placeId: n,
                    isPartyLeader: t,
                    gender: i
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done(function(n) {
                n.Error ? u(n.Error, f) : r(n, f)
            })
        },
        RequestPlayWithParty: function(n, t, i, r, u, f) {
            $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "RequestPlayWithParty",
                    placeId: n,
                    partyGuid: t,
                    gameId: i
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done(function(n) {
                n.Error ? u(n.Error, f) : r(n, f)
            })
        },
        RequestGroupBuildGame: function(n, t, i, r) {
            $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "RequestGroupBuildGame",
                    placeId: n
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done(function(n) {
                n.Error ? i(n.Error, r) : t(n, r)
            })
        },
        RequestFollowUser: function(n, t, i, r) {
            $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "RequestFollowUser",
                    userId: n
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done(function(n) {
                n.Error ? i(n.Error, r) : t(n, r)
            })
        },
        RequestGameJob: function(n, t, i, r, u) {
            $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "RequestGameJob",
                    placeId: n,
                    gameId: t
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done(function(n) {
                n.Error ? r(n.Error, u) : i(n, u)
            })
        },
        CheckGameJobStatus: function(n, t, i, r) {
            $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "CheckGameJobStatus",
                    jobId: n
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done(function(n) {
                n.Error ? i(n.Error, r) : t(n, r)
            })
        },
        RequestPrivateGame: function(n, t, i, r, u, f, e) {
            i = i !== null && i !== undefined ? i : "", $.ajax({
                method: "GET",
                url: "/Game/PlaceLauncher.ashx",
                data: {
                    request: "RequestPrivateGame",
                    placeId: n,
                    accessCode: t,
                    gender: i,
                    linkCode: r
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }).done(function(n) {
                n.Error ? f(n.Error, e) : u(n, e)
            })
        }
    };
RobloxLaunch.RequestPlayWithParty = function(n, t, i, r) {
    EventTracker.start("Launch"), RobloxPlaceLauncherService.LogJoinClick(), RobloxLaunch.state = RobloxLaunchStates.None, checkRobloxInstall() && (RobloxLaunch.launcher === null && (RobloxLaunch.launcher = new RBX.PlaceLauncher(n)), RobloxLaunch.launcher.RequestPlayWithParty(t, i, r))
}, RobloxLaunch.RequestGame = function(n, t, i) {
    EventTracker.start("Launch"), RobloxPlaceLauncherService.LogJoinClick(), RobloxLaunch.state = RobloxLaunchStates.None, checkRobloxInstall() && (RobloxLaunch.launcher === null && (RobloxLaunch.launcher = new RBX.PlaceLauncher(n)), RobloxLaunch.launcher.RequestGame(t, i))
}, RobloxLaunch.RequestPrivateGame = function(n, t, i, r, u) {
    EventTracker.start("Launch"), RobloxPlaceLauncherService.LogJoinClick(), RobloxLaunch.state = RobloxLaunchStates.None, checkRobloxInstall() && (RobloxLaunch.launcher === null && (RobloxLaunch.launcher = new RBX.PlaceLauncher(n)), RobloxLaunch.launcher.RequestPrivateGame(t, i, r, u))
}, RobloxLaunch.RequestGroupBuildGame = function(n, t) {
    EventTracker.start("Launch"), RobloxPlaceLauncherService.LogJoinClick(), RobloxLaunch.state = RobloxLaunchStates.None, checkRobloxInstall() && (RobloxLaunch.launcher === null && (RobloxLaunch.launcher = new RBX.PlaceLauncher(n)), RobloxLaunch.launcher.RequestGroupBuildGame(t))
}, RobloxLaunch.RequestGameJob = function(n, t, i) {
    EventTracker.start("Launch"), RobloxPlaceLauncherService.LogJoinClick(), RobloxLaunch.state = RobloxLaunchStates.None, checkRobloxInstall() && (RobloxLaunch.launcher === null && (RobloxLaunch.launcher = new RBX.PlaceLauncher(n)), RobloxLaunch.launcher.RequestGameJob(t, i))
}, RobloxLaunch.RequestFollowUser = function(n, t) {
    EventTracker.start("Launch"), RobloxPlaceLauncherService.LogJoinClick(), RobloxLaunch.state = RobloxLaunchStates.None, checkRobloxInstall() && (RobloxLaunch.launcher === null && (RobloxLaunch.launcher = new RBX.PlaceLauncher(n)), RobloxLaunch.launcher.RequestFollowUser(t))
}, RobloxLaunch.StartGame = function(n, t, i, r, u) {
    var f = function(r) {
            RobloxLaunch.StartGameWork(n, t, i, r, u)
        },
        e;
    r == "FETCH" ? (e = Roblox.Endpoints.getAbsoluteUrl("/Game/GetAuthTicket"), $.ajax({
        method: "GET",
        url: e,
        crossDomain: !0,
        xhrFields: {
            withCredentials: !0
        }
    }).done(f)) : f(r)
}, RobloxLaunch.StartGameWork = function(n, t, i, r, u) {
    var o, f, s, e, h;
    i = i.replace("http://", "https://"), n.indexOf("http") >= 0 && (n = typeof RobloxLaunch.SeleniumTestMode == "undefined" ? n + "&testmode=false" : n + "&testmode=true"), GoogleAnalyticsEvents && GoogleAnalyticsEvents.ViewVirtual("Visit/Try/" + t), RobloxLaunch.state = RobloxLaunchStates.StartingClient, RobloxLaunch.googleAnalyticsCallback !== null && RobloxLaunch.googleAnalyticsCallback(), o = null;
    try {
        if (typeof window.external != "undefined" && window.external.IsRoblox2App && (n.indexOf("visit") != -1 || u)) window.external.StartGame(r, i, n);
        else if (o = "RobloxProxy/", f = Roblox.Client.CreateLauncher(!0), f) {
            o = "RobloxProxy/StartGame/";
            try {
                try {
                    Roblox.Client.IsIE() ? f.AuthenticationTicket = r : f.Put_AuthenticationTicket(r), u && f.SetEditMode()
                } catch (l) {}
                try {
                    if (Roblox.Client._silentModeEnabled) f.SetSilentModeEnabled(!0), s = Roblox.VideoPreRollDFP ? Roblox.VideoPreRollDFP : Roblox.VideoPreRoll, s && s.videoInitialized && s.isPlaying() && Roblox.Client.SetStartInHiddenMode(!0), f.StartGame(i, n), RobloxLaunch.CheckGameStarted(f, t);
                    else throw new Error("silent mode is disabled, fall back");
                } catch (l) {
                    if (f.StartGame(i, n), Roblox.Client._bringAppToFrontEnabled) try {
                        f.BringAppToFront()
                    } catch (a) {}
                    Roblox.Client.ReleaseLauncher(f, !0, !1), $.modal.close()
                }
            } catch (l) {
                Roblox.Client.ReleaseLauncher(f, !0, !1);
                throw l;
            }
        } else {
            try {
                parent.playFromUrl(n);
                return
            } catch (c) {}
            if (Roblox.Client.isRobloxBrowser()) try {
                window.external.StartGame(r, i, n)
            } catch (c) {
                throw new Error("window.external fallback failed, Roblox must not be installed or IE cannot access ActiveX");
            } else throw new Error("launcher is null or undefined and external is missing");
            RobloxLaunch.state = RobloxLaunchStates.None, $.modal.close()
        }
    } catch (l) {
        if (GoogleAnalyticsEvents.ViewVirtual("Visit/CreateLauncher/" + Roblox.Client._whyIsRobloxLauncherNotCreated), e = l.message, e === "User cancelled") return GoogleAnalyticsEvents && GoogleAnalyticsEvents.ViewVirtual("Visit/UserCancelled/" + t), !1;
        try {
            h = new ActiveXObject("Microsoft.XMLHTTP")
        } catch (v) {
            e = "FailedXMLHTTP/" + e
        }
        return Roblox.Client.isRobloxBrowser() ? GoogleAnalyticsEvents && GoogleAnalyticsEvents.ViewVirtual("Visit/Fail/" + o + encodeURIComponent(e)) : (GoogleAnalyticsEvents && GoogleAnalyticsEvents.ViewVirtual("Visit/Redirect/" + o + encodeURIComponent(e)), window.location = RobloxLaunch.launchGamePage), !1
    }
    return GoogleAnalyticsEvents && GoogleAnalyticsEvents.ViewVirtual("Visit/Success/" + t), !0
}, RobloxLaunch.CheckGameStarted = function(n, t) {
    function u() {
        var e, o;
        try {
            if (r || (r = Roblox.Client.IsIE() ? n.IsGameStarted : n.Get_GameStarted()), r && !f && (EventTracker.endSuccess("StartClient"), EventTracker.endSuccess("Launch"), $("#PlaceLauncherStatusPanel").data("new-plugin-events-enabled") == "True" && (e = $("#PlaceLauncherStatusPanel").data("os-name"), e == "Windows" && (e = "Win32"), EventTracker.fireEvent("GameLaunchSuccessWeb_" + e), EventTracker.fireEvent("GameLaunchSuccessWeb_" + e + "_Plugin"), GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["Plugin", "Launch Success", t]), $("#PlaceLauncherStatusPanel").data("event-stream-for-plugin-enabled") == "True" && typeof Roblox.GamePlayEvents != "undefined" && Roblox.GamePlayEvents.SendClientStartSuccessWeb(null, play_placeId)), f = !0), o = Roblox.VideoPreRollDFP ? Roblox.VideoPreRollDFP : Roblox.VideoPreRoll, r && !o.isPlaying()) {
                if (MadStatus.stop("Connecting to Players..."), RobloxLaunch.state = RobloxLaunchStates.None, $.modal.close(), i._cancelled = !0, Roblox.Client._hiddenModeEnabled && Roblox.Client.UnhideApp(), Roblox.Client._bringAppToFrontEnabled) try {
                    n.BringAppToFront()
                } catch (s) {}
                Roblox.Client.ReleaseLauncher(n, !0, !1), googletag.cmd.push(function() {
                    googletag.pubads().refresh()
                })
            } else i._cancelled || setTimeout(u, 1e3)
        } catch (h) {
            i._cancelled || setTimeout(u, 1e3)
        }
    }
    var f = !1,
        i = RobloxLaunch.launcher,
        r;
    i === null && (i = new RBX.PlaceLauncher("PlaceLauncherStatusPanel"), i._showDialog(), i._updateStatus(0)), r = !1, u()
}, RobloxLaunch.CheckRobloxInstall = function(n) {
    if (Roblox.Client.IsRobloxInstalled()) return Roblox.Client.Update(), !0;
    window.location = n
}, RBX.PlaceLauncher = function(n) {
    this._cancelled = !1, this._popupID = n, this._popup = $("#" + n)
}, RBX.PlaceLauncher.prototype = {
    _showDialog: function() {
        var n, t, i;
        this._cancelled = !1, _popupOptions = {
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            zIndex: 1031
        }, n = Roblox.VideoPreRollDFP ? Roblox.VideoPreRollDFP : Roblox.VideoPreRoll, this._popupID == "PlaceLauncherStatusPanel" && (n && n.showVideoPreRoll && !n.isExcluded() ? (this._popup = $("#videoPrerollPanel"), _popupOptions.onShow = function(t) {
            n.correctIEModalPosition(t), n.start(), $("#prerollClose").hide(), $("#prerollClose").delay(1e3 * n.adTime + n.videoLoadingTimeout).show(300)
        }, _popupOptions.onClose = function() {
            n.close()
        }, _popupOptions.closeHTML = '<a href="#" id = "prerollClose" class="ImageButton closeBtnCircle_35h ABCloseCircle VprCloseButton"></a>') : (this._popup = $("#" + this._popupID), _popupOptions.onClose = function() {
            n.logVideoPreRoll(), $.modal.close()
        })), t = this, setTimeout(function() {
            t._popup.modal(_popupOptions)
        }, 0), i = this, $(".CancelPlaceLauncherButton").click(function() {
            i.CancelLaunch()
        }), $(".CancelPlaceLauncherButton").show()
    },
    _onGameStatus: function(n) {
        if (this._cancelled) {
            EventTracker.endCancel("GetConnection"), EventTracker.endCancel("Launch");
            return
        }
        if (this._updateStatus(n.status), n.status === 2) EventTracker.endSuccess("GetConnection"), EventTracker.start("StartClient"), RobloxLaunch.StartGame(n.joinScriptUrl, "Join", n.authenticationUrl, n.authenticationTicket);
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
        } else n.status === 4 && (EventTracker.endFailure("GetConnection"), EventTracker.endFailure("Launch"))
    },
    _updateStatus: function(n) {
        MadStatus.running || (MadStatus.init($(this._popup).find(".MadStatusField"), $(this._popup).find(".MadStatusBackBuffer"), 2e3, 800), MadStatus.start());
        switch (n) {
            case 0:
                break;
            case 1:
                MadStatus.manualUpdate("A server is loading the game...", !0);
                break;
            case 2:
                MadStatus.manualUpdate("The server is ready. Joining the game...", !0, !1);
                break;
            case 3:
                MadStatus.manualUpdate("Joining games is temporarily disabled while we upgrade. Please try again soon.", !1);
                break;
            case 4:
                MadStatus.manualUpdate("An error occurred. Please try again later.", !1);
                break;
            case 5:
                MadStatus.manualUpdate("The game you requested has ended.", !1);
                break;
            case 6:
                MadStatus.manualUpdate("The game you requested is currently full. Waiting for an opening...", !0, !0);
                break;
            case 7:
                MadStatus.manualUpdate("Roblox is updating. Please wait...", !0);
                break;
            case 8:
                MadStatus.manualUpdate("Requesting a server", !0);
                break;
            case 10:
                MadStatus.manualUpdate("The user has left the game.", !1);
                break;
            case 11:
                MadStatus.manualUpdate("The game you requested is not available on your platform.", !1);
                break;
            case 12:
                MadStatus.manualUpdate("You are not authorized to join this game.", !1);
                break;
            default:
                MadStatus.stop("Connecting to Players...")
        }
        $(this._popup).find(".MadStatusStarting").css("display", "none"), $(this._popup).find(".MadStatusSpinner").css("visibility", n === 3 || n === 4 || n === 5 ? "hidden" : "visible")
    },
    _onGameError: function() {
        this._updateStatus(4)
    },
    _startUpdatePolling: function(n) {
        var t, i;
        try {
            if (RobloxLaunch.state = RobloxLaunchStates.Upgrading, t = Roblox.Client.CreateLauncher(!0), i = Roblox.Client.IsIE() ? t.IsUpToDate : t.Get_IsUpToDate(), i || i === undefined) {
                try {
                    t.PreStartGame()
                } catch (e) {}
                Roblox.Client.ReleaseLauncher(t, !0, !1), RobloxLaunch.state = RobloxLaunchStates.StartingServer, EventTracker.endSuccess("UpdateClient"), n();
                return
            }
            var r = function(t, i, r) {
                    r._onUpdateStatus(t, i, n)
                },
                u = function(n, t) {
                    t._onUpdateError(n)
                },
                f = this;
            this.CheckUpdateStatus(r, u, t, n, f)
        } catch (e) {
            Roblox.Client.ReleaseLauncher(t, !0, !1), EventTracker.endSuccess("UpdateClient"), n()
        }
    },
    _onUpdateStatus: function(n, t, i) {
        if (!this._cancelled)
            if (this._updateStatus(n), n === 8) Roblox.Client.ReleaseLauncher(t, !0, !0), Roblox.Client.Refresh(), RobloxLaunch.state = RobloxLaunchStates.StartingServer, EventTracker.endSuccess("UpdateClient"), i();
            else if (n === 7) {
            var u = function(n, t, r) {
                    r._onUpdateStatus(n, t, i)
                },
                f = function(n, t) {
                    t._onUpdateError(n)
                },
                r = this,
                e = function() {
                    r.CheckUpdateStatus(u, f, t, i, r)
                };
            window.setTimeout(e, 2e3)
        } else alert("Unknown status from CheckUpdateStatus")
    },
    _onUpdateError: function() {
        this._updateStatus(2)
    },
    CheckUpdateStatus: function(n, t, i, r, u) {
        try {
            if (i.PreStartGame(), Roblox.Client.IsIE()) var f = i.IsUpToDate;
            else f = i.Get_IsUpToDate();
            f || f === undefined ? n(8, i, u) : n(7, i, u)
        } catch (e) {
            n(8, i, u)
        }
    },
    RequestGame: function(n, t) {
        var r;
        this._showDialog();
        var u = function(n, t) {
                t._onGameStatus(n)
            },
            f = function(n, t) {
                t._onGameError(n)
            },
            e = this,
            i = !1;
        return typeof Party != "undefined" && typeof Party.AmILeader == "function" && (i = Party.AmILeader()), typeof angular == "undefined" || angular.isUndefined(angular.element("#chat-container").scope()) || (i = angular.element("#chat-container").scope().isPartyLeader()), r = function() {
            EventTracker.start("GetConnection"), RobloxPlaceLauncherService.RequestGame(n, i, t, u, f, e)
        }, this._startUpdatePolling(r), !1
    },
    RequestPrivateGame: function(n, t, i, r) {
        this._showDialog();
        var u = function(n, t) {
                t._onGameStatus(n)
            },
            f = function(n, t) {
                t._onGameError(n)
            },
            e = this,
            o = function() {
                EventTracker.start("GetConnection"), RobloxPlaceLauncherService.RequestPrivateGame(n, t, i, r, u, f, e)
            };
        return this._startUpdatePolling(o), !1
    },
    RequestPlayWithParty: function(n, t, i) {
        this._showDialog();
        var r = function(n, t) {
                t._onGameStatus(n)
            },
            u = function(n, t) {
                t._onGameError(n)
            },
            f = this,
            e = function() {
                EventTracker.start("GetConnection"), RobloxPlaceLauncherService.RequestPlayWithParty(n, t, i, r, u, f)
            };
        return this._startUpdatePolling(e), !1
    },
    RequestGroupBuildGame: function(n) {
        this._showDialog();
        var t = function(n, t) {
                t._onGameStatus(n, !0)
            },
            i = function(n, t) {
                t._onGameError(n)
            },
            r = this,
            u = function() {
                EventTracker.start("GetConnection"), RobloxPlaceLauncherService.RequestGroupBuildGame(n, t, i, r)
            };
        return this._startUpdatePolling(u), !1
    },
    RequestFollowUser: function(n) {
        this._showDialog();
        var t = function(n, t) {
                t._onGameStatus(n)
            },
            i = function(n, t) {
                t._onError(n)
            },
            r = this,
            u = function() {
                EventTracker.start("GetConnection"), RobloxPlaceLauncherService.RequestFollowUser(n, t, i, r)
            };
        return this._startUpdatePolling(u), !1
    },
    RequestGameJob: function(n, t) {
        this._showDialog();
        var i = function(n, t) {
                t._onGameStatus(n)
            },
            r = function(n, t) {
                t._onGameError(n)
            },
            u = this,
            f = function() {
                EventTracker.start("GetConnection"), RobloxPlaceLauncherService.RequestGameJob(n, t, i, r, u)
            };
        return this._startUpdatePolling(f), !1
    },
    CancelLaunch: function() {
        return this._cancelled = !0, $.modal.close(), !1
    },
    dispose: function() {
        RBX.PlaceLauncher.callBaseMethod(this, "dispose")
    }
};