typeof Roblox=="undefined" &&(Roblox= {}),
$(function() {
        if(localStorage) {
            var u="Roblox.CookieNoticeUi.CookieLawNoticeShown", t=$(".alert-info"), i=$(".alert-container"), n=$(".alert-cookie-notice"), r=n.data("cookie-notice-timeout"); function e() {
                var n=localStorage.getItem(u); return n===null? !1:n
            }

            function o(n) {
                localStorage.setItem(u, n|| !1)
            }

            function f(r, u) {
                n.is(":visible")&&(r?n.slideUp(function() {
                            t.slideDown()
                        }):(u?i.css("min-height", ""):i.css("min-height", n.outerHeight()), n.slideUp()))
            }

            function s() {
                if( !e()) {
                    var u=t.is(":visible"); u&&(i.css("min-height", t.outerHeight()), t.hide()), n.slideDown(), o( !0), $(".cookie-law-notice-dismiss").click(function() {
                            f(u, !0)

                        }), r||(r=2e4), setTimeout(function() {
                            f(u, !1)
                        }

                        , r)
                }
            }

            s()
        }
    });