// Reference/themeUpdate.js
"use strict";
var Roblox = Roblox || {};
Roblox.ThemeUpdate = function() {
    function f(t) {
        r !== t.themeType && (h(), s(n[t.themeType]), r = t.themeType)
    }

    function u() {
        var n = Roblox.EnvironmentUrls.accountSettingsApi + "/v1/themes/user";
        $.ajax({
            type: "GET",
            url: n,
            contentType: "application/json; charset=utf-8",
            success: f
        })
    }

    function e() {
        if (Roblox && Roblox.RealTime) {
            var n = Roblox.RealTime.Factory.GetClient();
            n.Subscribe("UserThemeTypeChangeNotification", u)
        }
        $(document).on("Roblox.ThemeUpdate", u)
    }

    function o() {
        var n = $(t);
        return n && n.length > 0
    }

    function s(n) {
        o() ? $(t).addClass(n) : i.forEach(function(t) {
            $(t) && $(t).addClass(n)
        })
    }

    function h() {
        for (var r in n) $(t).removeClass(n[r]), i.forEach(function(t) {
            $(t) && $(t).removeClass(n[r])
        })
    }

    function c() {
        e()
    }
    var n = {
            Classic: "classic",
            Dark: "dark-theme",
            Light: "light-theme"
        },
        t = ".rbx-body",
        i = ["#header", "#navigation", ".container-main", "#chat-container", ".notification-stream-base"],
        r = "";
    return {
        init: c
    }
}(), $(function() {
    Roblox.ThemeUpdate.init()
});