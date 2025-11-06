// Thumbnails/ThumbnailView.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.ThumbnailView = function() {
    function h() {
        try {
            var n = document.createElement("canvas");
            return !!window.WebGLRenderingContext && (n.getContext("webgl") || n.getContext("experimental-webgl"))
        } catch (t) {
            return !1
        }
    }

    function c() {
        return $(i).data("3d-thumbs-enabled") !== undefined ? !0 : !1
    }

    function l() {
        return g(u) === !0
    }

    function tt() {
        if (typeof localStorage != "undefined" && localStorage != null) {
            var n = localStorage.getItem(u);
            return n == "false" || !1
        }
        return !1
    }

    function s(n) {
        nt(u, n)
    }

    function nt(n, t) {
        typeof localStorage != "undefined" && localStorage != null && localStorage.setItem(n, t)
    }

    function g(n) {
        if (typeof localStorage != "undefined" && localStorage != null) {
            var t = localStorage.getItem(n);
            return t == "true" || !1
        }
        return !1
    }

    function d() {
        var n = $(".enable-three-dee");
        n.addClass("disabled")
    }

    function v() {
        if (c() && h()) {
            function i(n) {
                n === !0 ? t.text("2D") : t.text("3D")
            }
            var n = l(),
                t = $(".enable-three-dee");
            i(n), t.css("visibility", "visible");
            t.on("click", function() {
                t.hasClass("disabled") || (n == !1 ? GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["3D Thumbnails", "Enable 3D Button Clicked"]) : GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["3D Thumbnails", "Disable 3D Button Clicked"]), n = !n, s(n), n === !1 ? f() : y(), i(n))
            })
        }
    }

    function it() {
        if (r) {
            var f = n.find(i),
                u = f.data("url");
            u = u + "&_=" + $.now(), n.load(u, function() {
                t = n.find(o), p()
            })
        }
    }

    function p() {
        h() && l() && c() ? (y(), v()) : (f(), v())
    }

    function y() {
        w(), e = t.load3DThumbnail(function(t) {
            GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["3D Thumbnail Loading", "Load succeeded"]), n.find("canvas").not(t).remove()
        }, function() {
            GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["3D Thumbnail Loading", "Load failed"]), d(), f()
        })
    }

    function k() {
        e !== undefined && e.cancel()
    }

    function b() {
        k(), n.find("canvas").remove(), n.find(".thumbnail-spinner").remove()
    }

    function w() {
        var t = n.find(o + " > img");
        t.hide()
    }

    function a() {
        r && t.find(" > img").attr("src", rt)
    }

    function f() {
        b(), t.find(" > img").show();
        var i = n.find("span[data-retry-url]");
        i.length > 0 && (a(), i.loadRobloxThumbnails())
    }
    var r = !1,
        n, t, e, i = ".thumbnail-holder",
        o = ".thumbnail-span",
        rt = "/images/Spinners/ajax_loader_blue_300.gif",
        u = "RobloxUse3DThumbnailsV2";
    return $(function() {
        $(i).length > 0 && ($(i).data("reset-enabled-every-page") !== undefined && (tt() || s(!0)), n = $(i).parent(), t = n.find(o), p(), r = !0)
    }), {
        showSpinner: a,
        reloadThumbnail: it
    }
}();