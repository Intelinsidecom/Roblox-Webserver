// GameInstances/FriendsRunningGameInstances.js
var Roblox = Roblox || {};
Roblox.FriendsRunningGameInstances = function() {
    var n = "#rbx-friends-running-games",
        r = n + " .rbx-friends-game-server-item-container",
        f = n + " .rbx-friends-game-server-template",
        u = 0,
        t = $(r);
    $(".rbx-tab a[href='#game-instances']").on("shown.bs.tab", function() {
        Roblox.FriendsRunningGameInstances.init()
    });
    var e = function() {
            if (!(u > 0)) {
                u += 1, Roblox.FriendsRunningGameInstances.fetchFirstServers();
                $("body").on("click", function(n) {
                    $('[data-toggle="popover-dynamic"]').each(function() {
                        $(this).is(n.target) || $(this).has(n.target).length !== 0 || $(this).siblings(".popover").has(n.target).length !== 0 || $(this).popover("hide")
                    })
                });
                t.on("click", ".rbx-friends-game-server-shutdown", function(t) {
                    t.preventDefault();
                    var i = $(n).attr("data-showshutdown"),
                        r = $(n).attr("data-placeid"),
                        u = $(t.target).parents(".rbx-friends-game-server-item").attr("data-gameid");
                    i && Roblox.FriendsRunningGameInstances.shutdownInstance(r, u), $(this).parents(".rbx-friends-game-server-menu").find(".rbx-menu-item").popover("hide")
                });
                $(".rbx-friends-running-games-load-more").on("click", function() {
                    var t = $(r + " .rbx-friends-game-server-item").length,
                        i = $(n).attr("data-placeId");
                    Roblox.FriendsRunningGameInstances.fetchServers(i, t)
                })
            }
        },
        o = function(n, t) {
            var r = $(f).clone(),
                l = n.Capacity,
                s = n.CurrentPlayers,
                a = s.length,
                v = '<a class="rbx-menu-item" data-toggle="popover-dynamic" data-bind="game-server-context-menu-' + e + '" data-original-title="" title="" data-viewport=".rbx-friends-game-server-item" ><i class="icon-more"></i></a><div class="rbx-popover-content" data-toggle="game-server-context-menu-' + e + '"><ul class="dropdown-menu" role="menu"><!--<li><a href="#">Configure</a></li>--><li><a href="#" class="rbx-friends-game-server-shutdown">Shut Down This Server</a></li></ul>',
                e = Math.floor(Math.random() * 1e6),
                o, c;
            r.find(".rbx-game-server-title").html("&nbsp;"), r.find(".rbx-friends-game-server-status").text(a + " of " + l + " Players Max"), n.FriendsMouseover != "" && r.find(".rbx-friends-game-server-status").append("<div title='" + n.FriendsMouseover + "'>" + n.FriendsDescription + "</div>"), r.find(".rbx-friends-game-server-join").attr("data-placeid", n.PlaceId), r.find(".rbx-friends-game-server-item").attr("data-gameid", n.Guid).attr("data-show-shutdown-all", n.ShowShutdownAllButton), t && r.find(".rbx-friends-game-server-menu").html(v), r.find(".rbx-menu-item").attr("data-bind", "game-server-context-menu-" + e), r.find(".rbx-popover-content").attr("data-toggle", "game-server-context-menu-" + e), r.find(".rbx-friends-game-server-join").attr("onclick", n.JoinScript), n.ShowSlowGameMessage || r.find(".rbx-friends-game-server-alert").addClass("hidden"), o = "", c = "headshot-thumbnail";
            for (i in s) {
                var u = s[i],
                    y = u.Thumbnail.IsFinal,
                    h = "";
                y || (h = "data-retry-url='/" + c + "/json?userId=" + u.Id + "&width=48&height=48&format=PNG'"), o += u.Id > 0 ? "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' href='" + Roblox.Endpoints.getAbsoluteUrl("/users/" + u.Id + "/profile") + "' " + h + " title='" + u.Username + "'><img class='avatar-card-image' src='" + u.Thumbnail.Url + "'></a></span>" : "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' " + h + " title='" + u.Username + "'><img class='avatar-card-image' src='" + u.Thumbnail.Url + "'></a></span>"
            }
            return r.find(".rbx-friends-game-server-players").html(o), r = r.html()
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
        a = function(i, u) {
            u = u == undefined ? 0 : u, $.ajax({
                type: "GET",
                url: "/Games/GetFriendsGameInstances",
                data: {
                    placeId: i,
                    startindex: u
                },
                cache: !1,
                contentType: "application/json; charset=utf-8",
                success: function(i) {
                    var e = i.Collection,
                        s = i.ShowShutdownAllButton,
                        f, h;
                    $(n).attr("data-showshutdown", s), f = $("<div></div>");
                    for (h in e) f.append(o(e[h], s));
                    u == 0 ? t.html(f.html()) : t.append(f.html()), e.length == 0 ? (t.addClass("section-content-off"), t.append($("<p> No Servers Found. </p>"))) : t.removeClass("section-content-off");
                    var l = $(r + " .rbx-friends-game-server-item").length,
                        a = i.Collection.length,
                        v = i.TotalCollectionSize,
                        c = $(".rbx-friends-running-games-load-more");
                    a > 0 && l < v ? c.removeClass("hidden") : c.addClass("hidden"), Roblox.FriendsRunningGameInstances.populateAvatarImages(".player-avatar a"), Roblox.FriendsRunningGameInstances.bindPopovers()
                },
                error: function() {
                    t.find(".loading").remove().append("<div class='empty'>Sorry, something went wrong loading places.</div>")
                }
            })
        },
        v = function() {
            var t = $(n).attr("data-placeid");
            Roblox.FriendsRunningGameInstances.fetchServers(t, 0)
        };
    return {
        init: e,
        fetchServers: a,
        fetchFirstServers: v,
        populateAvatarImages: s,
        bindPopovers: h,
        clearInstances: c,
        shutdownInstance: l
    }
}();