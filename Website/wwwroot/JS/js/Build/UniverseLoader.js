// /js/Build/UniverseLoader.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.BuildPage == "undefined" && (Roblox.BuildPage = {}), Roblox.BuildPage.UniverseLoader = function() {
    function n(n, i) {
        i && n.html("");
        var u = n.find(".load-more-universes").hide(),
            r = n.closest(".BuildPageContent"),
            f = r.find(".build-loading-container").show();
        $.ajax({
            type: "GET",
            url: "/build/universes",
            data: {
                startRow: n.find(">.item-table").length,
                activeOnly: r.find(t).is(":checked"),
                groupId: r.data("groupid")
            },
            cache: !1,
            dataType: "html",
            success: function(t) {
                u.remove(), f.hide();
                var i = $(t).hide();
                n.append(i), i.fadeIn(), i.find("a[data-retry-url]").loadRobloxThumbnails()
            },
            fail: function() {
                u.show(), f.hide()
            }
        })
    }

    function i() {
        $("body").on("change", t, function() {
            n($(this).closest(".content-area").find(">.items-container"), !0)
        });
        $("body").on("click", ".load-more-universes", function() {
            return n($(this).parent()), !1
        })
    }
    var t = ".active-only-checkbox > input[type='checkbox']";
    return $(i), {
        init: i,
        loadUniverses: n
    }
}();