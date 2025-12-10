// widgets/Favorites.js
$(function() {
    Roblox = Roblox || {};
    var i = $("#favorite-icon"),
        y = $("#toggle-favorite"),
        u = $(".favorite-button-container .tooltip-container"),
        f = i.length,
        e = y.data("signin-url"),
        v = "Login Required",
        s = "Remove from Favorites",
        h = "Add to Favorites",
        l = "<div>You must be logged in to favorite this game.</div><div>Please <a href='" + e + "'>Login or Register</a> to continue</div>",
        r = $(".favoriteCount"),
        n = 0,
        p = $(".favorite-button, #toggle-favorite"),
        t = i.hasClass("favorited"),
        o = t ? s : h,
        c = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
        a = 200,
        w = 2e3;
    f ? u.attr("data-original-title", o) : u.attr("title", o), p.click(function() {
        var u = $(this);
        u.find(i).hasClass("icon-favorite") && u.attr("data-isguest").toLowerCase() === "true" ? Roblox.GenericConfirmation.open({
            titleText: v,
            bodyContent: l,
            onAccept: function() {
                window.location.href = e
            },
            acceptColor: Roblox.GenericConfirmation.blue,
            acceptText: "Login",
            declineText: "Cancel",
            allowHtmlContentInBody: !0
        }) : $.post(u.data("toggle-url"), {
            assetID: u.data("assetid")
        }, function(e) {
            var v, l, y, b, p, o, k;
            if (e.success) {
                for (t = !t, u.find(i).toggleClass("favorited"), v = !0, l = r.text(), y = 0; y < l.length && v; y++) b = l[y], b !== "," && $.inArray(b, c) < 0 && (v = !1);
                v && (l = l.replace(",", ""), n = parseInt(l, 10)), p = "", o = "", p = t ? s : h, v && !isNaN(n) && (t ? n++ : n--, o = n.toString(10), n >= 1e4 ? o = "10K+" : n >= 1e3 && (o = [o.slice(0, 1), ",", o.slice(1)].join("")), n > 0 ? r.text(o) : r.text("0")), f ? u.closest(".tooltip-container").attr("data-original-title", p) : u.closest("a").attr("title", p)
            } else k = $(".content .alert-warning"), Roblox.BootstrapWidgets && Roblox.BootstrapWidgets.ToggleSystemMessage && k.length && Roblox.BootstrapWidgets.ToggleSystemMessage(k, a, w, e.message)
        })
    })
});