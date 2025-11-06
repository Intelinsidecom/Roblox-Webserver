// MasterPageUI.js
$(function() {
    try {
        $(".tooltip").tipsy(), $(".tooltip-top").tipsy({
            gravity: "s"
        }), $(".tooltip-right").tipsy({
            gravity: "w"
        }), $(".tooltip-left").tipsy({
            gravity: "e"
        }), $(".tooltip-bottom").tipsy({
            gravity: "n"
        })
    } catch (n) {}
    $("a.btn-disabled-primary[disabled]").prop("disabled", !0)
}), typeof Roblox == "undefined" && (Roblox = {}), Roblox.FixedUI = function() {
    function t() {
        var n = 1024;
        return document.body && document.body.offsetWidth && (n = document.body.offsetWidth), window.innerWidth && window.innerHeight && (n = window.innerWidth), n
    }

    function i() {
        return !$(".nav-container").hasClass("no-gutter-ads")
    }

    function e() {
        return t() > u
    }
    var u = 700,
        n = navigator.userAgent.toLowerCase(),
        f = /mobile/i.test(n) || /ipad/i.test(n) || /iphone/i.test(n) || /android/i.test(n) || /playbook/i.test(n) || /blackberry/i.test(n),
        r;
    return $(function() {
        var n, t;
        i() && (n = $("#LeftGutterAdContainer iframe"), n.length > 0 && (t = $(".ad-annotations", n.contents()), t.addClass("left-gutter-ad")))
    }), r = {
        isMobile: f,
        gutterAdsEnabled: i,
        isHeaderFixed: e,
        getWindowWidth: t
    }
}();