// Games/GamesPageInteraction.js
"use strict";
if (typeof Roblox === 'undefined') {
    var Roblox = {};
}
if (typeof Roblox.GamesPageInteraction === 'undefined') {
    Roblox.GamesPageInteraction = function() {
    function n(n, t) {
        Roblox.EventStream && Roblox.EventStream.SendEvent("gamesPageInteraction", n, t)
    }

    function t(n) {
        switch (n.attr("id")) {
            case "SortFilter":
                return "SFMenu";
            case "TimeFilter":
                return "TFMenu";
            case "GenreFilter":
                return "GFMenu";
        }
        return "";
    }

    function r(i) {
        n(t(i), {
            aType: "focus"
        })
    }

    function u(i) {
        n(t(i), {
            aType: "offFocus"
        })
    }

    function f(i, r) {
        var u = $("ul.dropdown-menu li", i).index(r) + 1,
            f = r.data("value");
        n(t(i), {
            pos: u,
            aValue: f,
            aType: "click"
        })
    }

    function i(t, i) {
        var r = t.closest(".games-list-container"),
            u = $(".games-list-container").index(r) + 1,
            f = r.data("sortfilter");
        n(i, {
            pos: u,
            aValue: f
        })
    }

    function e(n) {
        var t = n.hasClass("next") ? "PageRight" : "PageLeft";
        i(n, t)
    }

    function o(n) {
        i(n, "SeeAll")
    }
    return {
        SendMenuFocus: r,
        SendMenuOffFocus: u,
        SendMenuClick: f,
        SendPagerClick: e,
        SendSeeAllClick: o
    }
}();
}
$(function() {
    $("div#FiltersAndSort").on("mousedown", ".input-group-btn", function(n) {
        if (n.which == 1)
            if ($(this).hasClass("open")) {
                var i = $(n.target),
                    t = i.parent();
                t.prop("tagName") === "LI" && Roblox.GamesPageInteraction.SendMenuClick($(this), t)
            } else Roblox.GamesPageInteraction.SendMenuFocus($(this))
    });
    $("div#FiltersAndSort").on("blur", ".input-group-btn", function(n) {
        var t = $(n.relatedTarget),
            i = t.parent();
        !$(n.target).hasClass("input-dropdown-btn") || t.prop("tagName") === "A" && i.prop("tagName") === "LI" && typeof i.data("value") != "undefined" || Roblox.GamesPageInteraction.SendMenuOffFocus($(this))
    });
    $("#GamesListsContainer").on("mousedown", "div.scroller", function(n) {
        n.which == 1 && Roblox.GamesPageInteraction.SendPagerClick($(this))
    });
    $("#GamesListsContainer").on("mousedown", "div.see-all-button", function(n) {
        n.which == 1 && Roblox.GamesPageInteraction.SendSeeAllClick($(this))
    })
});