// widgets/tabs.js
$(function() {
    function i(n) {
        var t = $(n),
            i = t.parent(),
            r = i.next().children().eq(t.index());
        $.each([t, r], function(n, t) {
            t.addClass("tab-active").siblings().removeClass("tab-active")
        })
    }
    var t = $(".tab-container"),
        n = t.filter(".tab-history-hash"),
        f;
    t.children("div").on("click", function() {
        var t = $(this);
        i(this), t.parent().trigger("tabsactivate", {
            newTab: t
        })
    });
    if (n.length) {
        function r() {
            return "#" + n.next().children(".tab-active").attr("id")
        }

        function u(t) {
            var r = $(t).index();
            r !== -1 && i(n.children().eq(r))
        }
        window.location.hash ? u(window.location.hash) : f = r();
        n.on("tabsactivate", function() {
            history.pushState({}, "", r())
        });
        $(window).on("popstate", function() {
            u(window.location.hash ? window.location.hash : f)
        })
    }
});