// My/Avatar.js
Roblox = Roblox || {}, Roblox.Avatar = function() {
    function f() {
        (i = $("#header"), r = 10, t = $("#wrap"), n = $(".right-wrapper-placeholder"), n.length !== 0) && ($(window).scroll(u), u())
    }

    function u() {
        var u = n[0].getBoundingClientRect().top,
            f = n.is(":visible"),
            e = i.height(),
            o = f && u - e - r < 0;
        o ? t.addClass("pinned") : t.removeClass("pinned")
    }
    var i, r, t, n;
    $(function() {
        f()
    })
}();