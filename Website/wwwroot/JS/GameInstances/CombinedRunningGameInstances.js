// GameInstances/CombinedRunningGameInstances.js
var Roblox = Roblox || {};
Roblox.FriendsRunningGameInstances = function() {
    function u(n) {
        n.addClass("section-content-off"), n.find(".no-servers-message").remove(), n.append($("<p class='no-servers-message'> No Servers Found. </p>"))
    }
    var n = "#rbx-friends-running-games",
        s = n + " .rbx-friends-game-server-item-container",
        h = n + " .rbx-friends-game-server-template",
        f = 0,
        t = $(s),
        e, o;
    $(".rbx-tab a[href='#game-instances']").on("shown.bs.tab", function() {
        Roblox.FriendsRunningGameInstances.init()
    });
    var c = function() {
            if (!(f > 0)) {
                f += 1, Roblox.FriendsRunningGameInstances.fetchAllServers();
                $("body").on("click", function(n) {
                    $('[data-toggle="popover-dynamic"]').each(function() {
                        var $trigger = $(this);
                        var $popover = $trigger.siblings(".popover");
                        if (!$trigger.is(n.target) && !$trigger.has(n.target).length !== 0 && !$popover.has(n.target).length !== 0) {
                            $(this).popover("hide");
                        }
                    })
                });
                t.on("click", ".rbx-friends-game-server-shutdown", function(t) {
                    t.preventDefault();
                    var i = $(n).attr("data-showshutdown"),
                        r = $(n).attr("data-placeid"),
                        u = $(t.target).parents(".rbx-friends-game-server-item").attr("data-gameid");
                    i && Roblox.FriendsRunningGameInstances.shutdownInstance(r, u), $(this).parents(".rbx-friends-game-server-menu").find(".rbx-menu-item").popover("hide")
                })
            }
        },
        l = function(n, t) {
            var r = $(h).clone(),
                l = n.Capacity,
                s = n.CurrentPlayers,
                a = s.length,
                f = Math.floor(Math.random() * 1e6),
                v = '<a class="rbx-menu-item" data-toggle="popover-dynamic" data-bind="game-server-context-menu-' + f + '" data-original-title="" title="" data-viewport=".rbx-friends-game-server-item" ><i class="icon-more"></i></a><div class="rbx-popover-content" data-toggle="game-server-context-menu-' + f + '"><ul class="dropdown-menu" role="menu"><!--<li><a href="#">Configure</a></li>--><li><a href="#" class="rbx-friends-game-server-shutdown">Shut Down This Server</a></li></ul>',
                e, c, u, o;
            r.find(".rbx-game-server-title").html("&nbsp;"), r.find(".rbx-friends-game-server-status").text(a + " of " + l + " Players Max"), n.FriendsMouseover != "" && r.find(".rbx-friends-game-server-status").append("<div title='" + n.FriendsMouseover + "'>" + n.FriendsDescription + "</div>"), r.find(".rbx-friends-game-server-join").attr("data-placeid", n.PlaceId), r.find(".rbx-friends-game-server-item").attr("data-gameid", n.Guid).attr("data-show-shutdown-all", n.ShowShutdownAllButton), t && r.find(".rbx-friends-game-server-menu").html(v), r.find(".rbx-menu-item").attr("data-bind", "game-server-context-menu-" + f), r.find(".rbx-popover-content").attr("data-toggle", "game-server-context-menu-" + f), r.find(".rbx-friends-game-server-join").attr("onclick", n.JoinScript), n.ShowSlowGameMessage || r.find(".rbx-friends-game-server-alert").addClass("hidden"), e = "", c = "headshot-thumbnail";
            for (i in s) u = s[i], o = "", u.Id > 0 ? (u.Thumbnail.IsFinal || (o = "data-retry-url='/" + c + "/json?userId=" + u.Id + "&width=48&height=48&format=PNG'"), e += "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' href='" + Roblox.Endpoints.getAbsoluteUrl("/users/" + u.Id + "/profile") + "' " + o + " title='" + u.Username + "'><img class='avatar-card-image' src='" + u.Thumbnail.Url + "'></a></span>") : e += "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' " + o + " title='" + u.Username + "'><img class='avatar-card-image' src='" + u.Thumbnail.Url + "'></a></span>";
            return r.find(".rbx-friends-game-server-players").html(e), r = r.html()
        },
        a = function() {
            $(n + " .player-avatar a").loadRobloxThumbnails()
        },
        v = function() {
            $(n + ' [data-toggle="popover-dynamic"]').popover({
                html: !0,
                placement: "bottom",
                content: function() {
                    var n = $(this).attr("data-bind");
                    return $('[data-toggle="' + n + '"]').html()
                }
            })
        },
        y = function() {
            t.empty()
        },
        p = function(n, t, i) {
            var r = {
                __RequestVerificationToken: $("[name=__RequestVerificationToken]").val(),
                placeId: n,
                gameId: t
            };
            typeof i != "undefined" && (r.privateServerId = i), $.ajax({
                type: "POST",
                url: "/game-instances/shutdown",
                data: r,
                success: function() {},
                error: function() {}
            })
        },
        r = [],
        w = function(n) {
            var i = $(".rbx-game-server-item[data-gameid=" + n + "]"),
                t;
            i.remove(), t = $("#rbx-running-games .rbx-game-server-item-container .rbx-game-server-item").length, t == 0 && u($("#rbx-running-games .rbx-game-server-item-container"))
        };
    return e = function(i) {
        $.ajax({
            type: "GET",
            url: "/Games/GetFriendsGameInstances",
            data: {
                placeId: i
            },
            cache: !1,
            contentType: "application/json; charset=utf-8",
            success: function(i) {
                var f = i.Collection,
                    s = i.ShowShutdownAllButton,
                    o, e;
                $(n).attr("data-showshutdown", s), o = $("<div></div>"), r = [];
                for (e in f) r.push(f[e].Guid), w(f[e].Guid), o.append(l(f[e], s));
                t.html(o.html()), f.length == 0 ? u(t) : t.removeClass("section-content-off"), Roblox.FriendsRunningGameInstances.populateAvatarImages(".player-avatar a"), Roblox.FriendsRunningGameInstances.bindPopovers()
            },
            error: function() {
                t.find(".loading").remove().append("<div class='empty'>Sorry, something went wrong loading places.</div>")
            }
        })
    }, o = function() {
        var t = $(n).attr("data-placeid");
        Roblox.FriendsRunningGameInstances.fetchServers(t)
    }, {
        init: c,
        fetchServers: e,
        fetchAllServers: o,
        populateAvatarImages: a,
        bindPopovers: v,
        clearInstances: y,
        shutdownInstance: p,
        friendServerGuids: function() {
            return r
        },
        setGameInstanceContainerEmpty: u
    }
}(), Roblox.AllRunningGameInstances = function() {
    var n = "#rbx-running-games",
        r = n + " .rbx-game-server-item-container",
        f = n + " .rbx-game-server-template",
        u = 0,
        t = $(r);
    $(".rbx-tab a[href='#game-instances']").on("shown.bs.tab", function() {
        Roblox.AllRunningGameInstances.init()
    });
    var e = function() {
            if (!(u > 0)) {
                u += 1, Roblox.AllRunningGameInstances.fetchFirstServers();
                t.on("click", ".rbx-game-server-shutdown", function(t) {
                    t.preventDefault();
                    var i = $(n).attr("data-showshutdown"),
                        r = $(n).attr("data-placeid"),
                        u = $(t.target).parents(".rbx-game-server-item").attr("data-gameid");
                    i && Roblox.AllRunningGameInstances.shutdownInstance(r, u), $(this).parents(".rbx-game-server-menu").find(".rbx-menu-item").popover("hide")
                });
                var i = "#rbx-friends-running-games",
                    f = i + " .rbx-friends-game-server-item-container";
                $(".rbx-running-games-load-more").on("click", function() {
                    var t = $(r + " .rbx-game-server-item").length + $(f + " .rbx-friends-game-server-item").length,
                        i = $(n).attr("data-placeId");
                    Roblox.AllRunningGameInstances.fetchServers(i, t)
                })
            }
        },
        o = function(n, t) {
            var r = $(f).clone(),
                l = n.Capacity,
                h = n.CurrentPlayers,
                a = h.length,
                o = Math.floor(Math.random() * 1e6),
                v = '<a class="rbx-menu-item" data-toggle="popover-dynamic" data-bind="game-server-context-menu-' + o + '" data-original-title="" title="" data-viewport=".rbx-game-server-item" ><i class="icon-more"></i></a><div class="rbx-popover-content" data-toggle="game-server-context-menu-' + o + '"><ul class="dropdown-menu" role="menu"><!--<li><a href="#">Configure</a></li>--><li><a href="#" class="rbx-game-server-shutdown">Shut Down This Server</a></li></ul>',
                e, c, u, s;
            r.find(".rbx-game-server-title").html("&nbsp;"), r.find(".rbx-game-server-status").text(a + " of " + l + " Players Max"), r.find(".rbx-game-server-join").attr("data-placeid", n.PlaceId), r.find(".rbx-game-server-item").attr("data-gameid", n.Guid).attr("data-show-shutdown-all", n.ShowShutdownAllButton), t && r.find(".rbx-game-server-menu").html(v), r.find(".rbx-menu-item").attr("data-bind", "game-server-context-menu-" + o), r.find(".rbx-popover-content").attr("data-toggle", "game-server-context-menu-" + o), r.find(".rbx-game-server-join").attr("onclick", n.JoinScript), n.ShowSlowGameMessage || r.find(".rbx-game-server-alert").addClass("hidden"), e = "", c = "headshot-thumbnail";
            for (i in h) u = h[i], s = "", u.Id !== 0 ? u.Id > 0 ? (u.Thumbnail.IsFinal || (s = "data-retry-url='/" + c + "/json?userId=" + u.Id + "&width=48&height=48&format=PNG'"), e += "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' href='" + Roblox.Endpoints.getAbsoluteUrl("/users/" + u.Id + "/profile") + "' " + s + " title='" + u.Username + "'><img class='avatar-card-image' src='" + u.Thumbnail.Url + "'></a></span>") : e += "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' " + s + "><img class='avatar-card-image' src='" + u.Thumbnail.Url + "'></a></span>" : e += "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link'><img class='avatar-card-image' src='" + u.Thumbnail.Url + "'></a></span>";
            return r.find(".rbx-game-server-players").html(e), r = r.html()
        },
        s = function() {
            $(n + " .player-avatar a").loadRobloxThumbnails()
        },
        h = function() {
            $(n + ' [data-toggle="popover-dynamic"]').popover({
                html: !0,
                placement: "bottom",
                content: function() {
                    var n = $(this).attr("data-bind");
                    return $('[data-toggle="' + n + '"]').html()
                }
            })
        },
        c = function() {
            t.empty()
        },
        l = function(n, t, i) {
            var r = {
                __RequestVerificationToken: $("[name=__RequestVerificationToken]").val(),
                placeId: n,
                gameId: t
            };
            typeof i != "undefined" && (r.privateServerId = i), $.ajax({
                type: "POST",
                url: "/game-instances/shutdown",
                data: r,
                success: function() {},
                error: function() {}
            })
        },
        a = function(n) {
            var t = Roblox.FriendsRunningGameInstances.friendServerGuids(),
                i;
            if (t == undefined) return !1;
            for (i in t)
                if (t[i] == n) return !0;
            return !1
        },
        v = function(i, r) {
            r = r == undefined ? 0 : r, $.ajax({
                type: "GET",
                url: "/games/getgameinstancesjson",
                data: {
                    placeId: i,
                    startindex: r
                },
                cache: !1,
                contentType: "application/json; charset=utf-8",
                success: function(i) {
                    var f = i.Collection,
                        h = i.ShowShutdownAllButton,
                        u, e, s;
                    $(n).attr("data-showshutdown", h), u = $("<div></div>");
                    for (e in f) a(f[e].Guid) || u.append(o(f[e], h));
                    r == 0 ? t.html(u.html()) : t.append(u.html()), i.Collection.length == 0 ? Roblox.FriendsRunningGameInstances.setGameInstanceContainerEmpty(t) : t.removeClass("section-content-off"), s = $(".rbx-running-games-load-more"), i.Collection.length == 10 ? s.removeClass("hidden") : s.addClass("hidden"), Roblox.AllRunningGameInstances.populateAvatarImages(".player-avatar a"), Roblox.AllRunningGameInstances.bindPopovers()
                },
                error: function() {
                    t.find(".loading").remove().append("<div class='empty'>Sorry, something went wrong loading places.</div>")
                }
            })
        },
        y = function() {
            var t = $(n).attr("data-placeid");
            Roblox.AllRunningGameInstances.fetchServers(t, 0)
        };
    return {
        init: e,
        fetchServers: v,
        fetchFirstServers: y,
        populateAvatarImages: s,
        bindPopovers: h,
        clearInstances: c,
        shutdownInstance: l
    }
}();