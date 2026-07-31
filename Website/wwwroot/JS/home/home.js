// home/home.js
var Roblox = Roblox || {};
Roblox.Home = function() {
    function i() {
        var i = $("#statusForm"),
            o = $("#loadingImage"),
            u = ".form-group",
            f = ".form-control-label",
            e = "form-has-feedback form-has-error";
        if (n.prop("disabled")) return !1;
        var s = r.data("update-status-url"),
            h = n.val(),
            c = {
                status: h
            };
        return t.hide(), o.show(), $.ajax({
            cache: !1,
            url: s,
            type: "POST",
            data: c,
            success: function(t) {
                t.success ? (n.val(t.message), i.find(u).removeClass(e), i.find(f).hide()) : (n.val(""), i.find(u).addClass(e), i.find(f).text(t.message).show())
            },
            error: function() {
                n.val(""), i.find(u).addClass(e), i.find(f).show()
            },
            complete: function() {
                n.blur(), t.show(), o.hide()
            }
        }), !0
    }

    function u() {
        var r = 13;
        t.click(function() {
            i()
        }), n.keypress(function(n) {
            n.which === r && i()
        })
    }

    function f() {
        $("#HomeContainer *[data-retry-url]").loadRobloxThumbnails()
    }

    function e(n) {
        var c = ".home-friends #friend_",
            l = ".friend-link",
            a = ".friend-status",
            o = Roblox.Constants.presenceTypes,
            v = n.UserId,
            r = $(c + v),
            u, e, i, s, h;
        if (r && r.length > 0) {
            u = r.find(a), u && u.length > 0 && u.remove();
            var f = r.find(l),
                y = n.LastLocation,
                t = $("<span />").addClass("avatar-status");
            t.attr("title", y);
            switch (n.UserPresenceType) {
                case o.inGame:
                    e = n.AbsolutePlaceUrl, i = "", e && e.length > 0 ? (i = t.addClass("icon-game"), f.append("<a class='friend-status place-link' href='" + e + "'>" + i + "</a>")) : (i = t.addClass("friend-status icon-game"), f.append(i));
                    break;
                case o.inStudio:
                    s = t.addClass("friend-status icon-studio"), f.append(s);
                    break;
                case o.online:
                    h = t.addClass("friend-status icon-online"), f.append(h)
            }
        }
    }

    function o() {
        $(document).on("Roblox.Presence.Update", function(n, t) {
            for (var i = 0; i < t.length; i++) e(t[i])
        })
    }

    function s() {
        $(document).on("GuttersHidden", function() {
            $("#LeftGutterAdContainer").hide(), $("#RightGutterAdContainer").hide()
        })
    }

    function h() {
        u(), f(), s(), o()
    }

    function c(n, t, i, r) {
        r && Roblox.Hashcash.setWorkerFile(r), Roblox.Hashcash.setRegex(t), Roblox.Hashcash.getValueToHash(n, function(n) {
            if (n) {
                var r = "/game/report-stats",
                    u = "HashcashDuration",
                    t = n.timeDiff;
                !t || t < 0 || $.ajax({
                    cache: !1,
                    url: r,
                    type: "POST",
                    data: {
                        name: u + "_" + i,
                        value: t
                    }
                })
            }
        })
    }
    var r = $("#HomeContainer"),
        n = $("#txtStatusMessage"),
        t = $("#shareButton");
    return $(function() {
        h()
    }), {
        doProofOfWork: c
    }
}();