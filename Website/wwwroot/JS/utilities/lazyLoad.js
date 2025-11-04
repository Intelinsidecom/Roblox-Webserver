// utilities/lazyLoad.js
"use strict";
Roblox = Roblox || {}, Roblox.LazyLoad = function() {
    function n(n) {
        if (n) {
            var t = n.attr("data-delaysrc");
            typeof t != "undefined" && n.attr("src", t).addClass("src-replaced")
        }
    }

    function u() {
        window.addEventListener("load", function() {
            $(t + ", " + i).each(function() {
                n($(this))
            })
        }, !1)
    }

    function f() {
        $(r).one("click touchstart", function() {
            var t = $("#iframe-login:not('.src-replaced')");
            n(t)
        })
    }

    function e() {
        u(), f()
    }
    var t = "img[data-delaysrc]",
        i = "iframe[data-delaysrc]:not('.src-replaced')",
        r = "#head-login, #header-login";
    $(document).ready(function() {
        e()
    })
}();