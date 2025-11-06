// DropDownNav.js
(function() {
    function r(t) {
        var r = $(t.target),
            f, u;
        r.attr("drop-down-nav-button") || (r = r.parents("[drop-down-nav-button]")), r.addClass("active"), f = r.attr("drop-down-nav-button"), u = n.filter('[data-drop-down-nav-container="' + f + '"]'), u.show(), n.not(u).hide(), i.not(r).removeClass("active"), t.stopPropagation(), r.trigger("showDropDown")
    }

    function u(n) {
        $("[drop-down-nav-button]").unbind("click", e), r(n), $("[drop-down-nav-button]").bind("mouseleave", f)
    }

    function f() {
        t(), $("[drop-down-nav-button]").unbind("mouseleave", f)
    }

    function e(n) {
        $("[drop-down-nav-button]").unbind("mouseenter", u), r(n), $(document).bind("click", function() {
            t()
        }), $("[drop-down-nav-button]").bind("click", o)
    }

    function o() {
        $(document).unbind("click", function() {
            t()
        }), t(), $("[drop-down-nav-button]").bind("click", r)
    }

    function t() {
        n.hide(), i.removeClass("active")
    }
    var n, i;
    $(function() {
        n = $("[data-drop-down-nav-container]"), i = $("[drop-down-nav-button]"), $("[drop-down-nav-button]").bind("click", e), $("[drop-down-nav-button]").bind("mouseenter", u)
    })
})();