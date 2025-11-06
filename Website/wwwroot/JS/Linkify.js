// Linkify.js
Roblox = Roblox || {}, Roblox.Linkify = function() {
    "use strict";

    function u() {
        var n = $("#roblox-linkify");
        if (n.length) {
            var r = n.data("regex"),
                u = n.data("regex-flags"),
                f = {
                    enabled: n.data("enabled"),
                    regex: new RegExp(r, u)
                };
            i(f)
        }
        t = !0
    }

    function i(t) {
        $.extend(n, t)
    }

    function f(n) {
        return $("<div>" + n + "</div>").find("a[href]").length > 0
    }

    function e(n) {
        return n.replace(/\&amp;/g, "&")
    }

    function o(n) {
        return n.clone().wrap("<div>").parent().html()
    }

    function s(t) {
        return t.match(n.hasProtocolRegex) || (t = n.defaultProtocol + t), t
    }

    function h(t) {
        t = e(t);
        var u = t,
            f = s(t),
            i = $("<a></a>");
        return i.addClass(n.cssClass), i.attr("href", f), i.text(u), r || i.attr("target", "_blank"), o(i)
    }

    function c(i) {
        return t || u(), n.enabled && !f(i) && (i = i.replace(n.regex, h)), i
    }
    var n = {
            enabled: !1,
            hasProtocolRegex: /(https?:\/\/)/,
            defaultProtocol: "http://",
            cssClass: "text-link"
        },
        t = !1,
        r = Roblox.FixedUI && Roblox.FixedUI.isMobile;
    return {
        String: c,
        SetOptions: i
    }
}(), $.fn.linkify = function() {
    return this.each(function() {
        var t = $(this),
            n = t.html(),
            i;
        typeof n != "undefined" && n !== null && (i = Roblox.Linkify.String(n), t.html(i))
    })
}, $(function() {
    $(".linkify").linkify()
});