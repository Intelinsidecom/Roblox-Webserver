// Leaderboards/Leaderboard.js
"use strict";
var Roblox = Roblox || {};
Roblox.Leaderboard = function() {
    function s(n) {
        var i = $(n.find(".rbx-leaderboard-filter")),
            t;
        i.on("click", ".dropdown-menu a", {
            leaderboard: n
        }, function(t) {
            var i = $(this).attr("data-time-filter"),
                r = t.data.leaderboard;
            n.find(".rbx-leaderboard-data").attr("data-time-filter", i), n.find(".rbx-leaderboard-filtername").text($(this).text()), $('[data-toggle="popover"]').popover("hide"), u(n), e(n)
        });
        t = $("." + n.attr("data-associated-leaderboard-more")).find(".rbx-leaderboard-see-more");
        t.on("click", function() {
            Roblox.Leaderboard.getMoreItems(n)
        })
    }

    function t(n, t) {
        var i = n.find(".rbx-leaderboard-item-template").clone(),
            s = t.DisplayRank,
            r = t.ProfileUri,
            u = t.UserImageUri,
            f = t.Name,
            e = t.ClanName != null ? t.ClanName : "",
            h = t.DisplayPoints,
            o = !1;
        return n.hasClass("rbx-leaderboard-clan") && (f = t.ClanName, e = t.Name, u = t.ClanImageUri, r = t.ClanUri, o = !0), i.find(".rank").text(s), o ? i.find(".avatar").html("<a href='" + r + "'><img src='" + u + "'/></a>") : i.find(".avatar").html("<a class='avatar-card-link' href='" + r + "'><img class='avatar-card-image' src='" + u + "'/></a>"), i.find(".name-and-group").html("<a class='text-name' href='" + r + "'><span class='name text-overflow' title='" + f + "'>" + f + "</span></a><span class='group text-overflow' title='" + e + "'>" + e + "</span>"), i.find(".points").text(h).attr("title", t.FullPoints), i = i.html()
    }

    function i(n, t) {
        return $("<div class='rbx-leaderboard-notification'><p>" + t + "</p></div>")
    }

    function e(n) {
        var u = n.find(".rbx-leaderboard-data"),
            f = n.find(".rbx-leaderboard-my"),
            h = u.attr("data-distributor-target-id"),
            e = u.attr("data-target-type"),
            c = u.attr("data-time-filter"),
            l = u.attr("data-rank-max"),
            o = u.attr("data-player-id"),
            s = u.attr("data-clan-id"),
            a = 48,
            v = 48,
            y = "PNG";
        $.ajax({
            type: "GET",
            url: "/leaderboards/rank/json",
            data: {
                targetType: e,
                distributorTargetId: h,
                timeFilter: c,
                startIndex: 0,
                currentRank: 1,
                previousPoints: -1,
                max: l,
                imgWidth: a,
                imgHeight: v,
                imgFormat: y
            },
            contentType: "application/json; charset=utf-8",
            success: function(i) {
                var r, u;
                if (f.find(".rbx-leaderboard-item, .rbx-leaderboard-notification").remove(), i.length > 0) {
                    if (e === 0 && o !== -1)
                        for (r = 0; r < i.length; r++)
                            if (i[r].UserId === o) {
                                f.html(t(n, i[r])), n.addClass("rbx-has-rank");
                                break
                            } if (e === 1 && s != -1)
                        for (u = 0; u < i.length; u++)
                            if (i[u].TargetId === s) {
                                f.html(t(n, i[u])), n.addClass("rbx-has-rank");
                                break
                            }
                }
            },
            error: function() {
                f.find(".rbx-leaderboard-item").remove(), f.append(i(n, r))
            }
        })
    }

    function u(u) {
        var f = u.find(".rbx-leaderboard-data"),
            e = u.find(".rbx-leaderboard-items"),
            l = e.find(".rbx-leaderboard-more-container"),
            s = u.find(".spinner"),
            a = f.attr("data-distributor-target-id"),
            h = f.attr("data-target-type"),
            v = f.attr("data-time-filter"),
            c = f.attr("data-max"),
            y = 48,
            p = 48,
            w = "PNG";
        e.find(".rbx-leaderboard-item, .rbx-leaderboard-notification").remove(), Roblox.Leaderboard.toggleMore(u, "off"), s.show(), $.ajax({
            type: "GET",
            url: "/leaderboards/game/json",
            data: {
                targetType: h,
                distributorTargetId: a,
                timeFilter: v,
                startIndex: 0,
                currentRank: 1,
                previousPoints: n[h],
                max: c,
                imgWidth: y,
                imgHeight: p,
                imgFormat: w
            },
            contentType: "application/json; charset=utf-8",
            success: function(r) {
                s.hide();
                for (var f in r) l.before(t(u, r[f]));
                r.length === 0 ? e.append(i(u, o)) : (n[h] = r[r.length - 1].Points, r.length == c && Roblox.Leaderboard.toggleMore(u, "on"))
            },
            error: function() {
                s.hide(), e.append(i(u, r))
            }
        })
    }

    function h(n, t) {
        t == "on" ? $("." + n.attr("data-associated-leaderboard-more")).find(".rbx-leaderboard-see-more").removeClass("hidden") : $("." + n.attr("data-associated-leaderboard-more")).find(".rbx-leaderboard-see-more").addClass("hidden")
    }

    function c(u) {
        var f = u.find(".rbx-leaderboard-data"),
            e = u.find(".rbx-leaderboard-items"),
            l = u.find(".rbx-leaderboard-item"),
            y = e.find(".rbx-leaderboard-more-container"),
            o = u.find(".spinner"),
            p = f.attr("data-distributor-target-id"),
            s = f.attr("data-target-type"),
            w = f.attr("data-time-filter"),
            a = f.attr("data-max"),
            b = 48,
            k = 48,
            d = "PNG",
            v, h, c;
        Roblox.Leaderboard.toggleMore(u, "off"), o.show(), v = u.find(".rbx-leaderboard-items .rbx-leaderboard-item").length, h = 1, l && (c = l.last().find(".rank"), c && (h = parseInt(c.text(), 10))), $.ajax({
            type: "GET",
            url: "/leaderboards/game/json",
            data: {
                targetType: s,
                distributorTargetId: p,
                timeFilter: w,
                startIndex: v,
                currentRank: h,
                previousPoints: n[s],
                max: a,
                imgWidth: b,
                imgHeight: k,
                imgFormat: d
            },
            contentType: "application/json; charset=utf-8",
            success: function(i) {
                o.hide();
                for (var r in i) y.before(t(u, i[r]));
                i.length > 0 && (n[s] = i[i.length - 1].Points, i.length == a && Roblox.Leaderboard.toggleMore(u, "on"))
            },
            error: function() {
                o.hide(), e.find(".rbx-leaderboard-item").remove(), e.append(i(u, r))
            }
        })
    }

    function l() {
        if (!(f > 0)) {
            f += 1;
            var n = [];
            $("#rbx-leaderboard-container-player, #rbx-leaderboard-container-clan").each(function(t, i) {
                n[t] = $(i), u(n[t]), e(n[t]), s(n[t])
            })
        }
    }
    var a = {
            Users: 0,
            Clans: 1
        },
        v = {
            TopLeaders: 0,
            MyRank: 1
        },
        y = {
            AllTime: 3,
            LastDay: 0,
            LastWeek: 1,
            LastMonth: 2
        },
        r = "Error loading rows.",
        p = "You are not yet ranked for this time period. Go earn some Points!",
        o = "No results found.",
        f = 0,
        n = [-1, -1];
    return {
        init: l,
        getItems: u,
        getMoreItems: c,
        toggleMore: h
    }
}();