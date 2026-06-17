// Thumbnails/ThumbnailView.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.ThumbnailView = function() {
    function f(n) {
        var t = n.find("canvas");
        t.remove()
    }

    function l(n) {
        n.forEach(function(n) {
            n.reload()
        })
    }

    function a(n) {
        n.forEach(function(n) {
            n.showSpinner()
        })
    }

    function h() {
        if (n === null) try {
            var t = document.createElement("canvas");
            n = !!window.WebGLRenderingContext && (t.getContext("webgl") || t.getContext("experimental-webgl")) ? !0 : !1
        } catch (i) {
            n = !1
        }
        return n
    }

    function c() {
        return window.localStorage && localStorage.setItem && localStorage.getItem
    }

    function v() {
        c() && (localStorage.getItem(r) === "true" ? i = !0 : (i = !1, localStorage.setItem(r, "false")))
    }

    function t(n) {
        return h() ? o(n) ? i : !1 : !1
    }

    function y(n, t) {
        (h() || (i = !1), o(t) || n !== !0) && (i = n, c() && localStorage.setItem(r, n.toString()))
    }

    function o(n) {
        return typeof n.data("3d-thumbs-enabled") == "string"
    }

    function p(i) {
        function r() {
            return i.find(".thumbnail-span")
        }

        function a() {
            return r().find("img")
        }

        function h() {
            return i.find(".thumbnail-holder")
        }

        function l() {
            return i.find(s)
        }

        function p() {
            return h().data("3dtype") || "static"
        }

        function et() {
            return p() === "static" && "2D" || ut
        }

        function tt() {
            return p() === "static" && "3D" || ft
        }

        function g() {
            f(i), Roblox.ThumbnailSpinner.show(r()), a().hide();
            r().one("thumbnailLoaded", function() {
                a().show(), Roblox.ThumbnailSpinner.hide(r())
            })
        }

        function nt() {
            l().html(t(h()) ? et() : tt()), t(h()) || (o(h()) ? n ? l().css("visibility", "visible") : l().addClass("disabled") : l().css("visibility", "hidden"))
        }

        function k(n) {
            c = undefined, i.find("canvas").not(n).remove()
        }

        function b() {
            c = undefined, l().addClass("disabled")
        }

        function rt() {
            c && c.cancel();
            var n = t(h());
            y(!n, h()), n !== t(h()) && v()
        }

        function w() {
            v()
        }

        function v() {
            var o = t(h()),
                n, u;
            o ? (e && (e = !1, n = r().data("orig-retry-url"), n && (r().data("retry-url", n + (n.indexOf("?") > -1 ? "&" : "?") + "_=" + Date.now()), r().loadRobloxThumbnails())), f(i), a().hide(), c = p() === "static" ? r().load3DThumbnail(k, b) : r().loadAnimatedThumbnail(k, b), a().hide()) : (e && (e = !1, n = r().data("orig-retry-url"), n && r().data("retry-url", n + (n.indexOf("?") > -1 ? "&" : "?") + "_=" + Date.now())), u = r().data("retry-url"), u ? (g(), r().loadRobloxThumbnails()) : (f(i), Roblox.ThumbnailSpinner.hide(r()), a().show())), nt()
        }

        function it() {
            var f, n, u;
            if (e = !0, f = t(h()), f) {
                v();
                return
            }
            n = r().data("orig-retry-url"), typeof n != "undefined" ? (r().data("retry-url", n + (n.indexOf("?") > -1 ? "&" : "?") + "_=" + Date.now()), v()) : (u = h().data("url"), u && i.load(u + "&_=" + $.now(), function() {
                w()
            }))
        }
        var c, ft = "<span class='icon-bigplay'></span>",
            ut = "<span class='icon-bigstop'></span>",
            d = {
                container: i,
                holder: h,
                span: r,
                button: l,
                showSpinner: g,
                load: w,
                reload: it
            };
        i.on("click", s, function() {
            $(this).hasClass("disabled") || rt()
        });
        return w(), u.push(d), d
    }
    var r = "RobloxUse3DThumbnailsV2",
        u = [],
        s = ".enable-three-dee",
        n = null,
        i = !1,
        e = !1;
    return v(), $(function() {
        $(".thumbnail-holder").each(function() {
            var n = $(this).parent();
            p(n)
        })
    }), {
        showSpinner: function() {
            return a(u)
        },
        reloadThumbnail: function() {
            return l(u)
        }
    }
}();
