// /js/Build/UniverseLoader.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.BuildPage == "undefined" && (Roblox.BuildPage = {}), Roblox.BuildPage.UniverseLoader = function () {
    function n(container, clearExisting) {
        console.log('[UniverseLoader.js] loadUniverses called; clearExisting:', !!clearExisting, 'container:', container && container.get && container.get(0));
        clearExisting && (console.log('[UniverseLoader.js] Clearing existing item-table children before load'), container.html(""));
        var loadMoreLink = container.find(".load-more-universes").hide(),
            buildPageContent = container.closest(".BuildPageContent");
        // Spinner disabled - f = buildPageContent.find(".build-loading-container").show();
        var startRow = container.find(">.item-table").length,
            activeOnly = buildPageContent.find(t).is(":checked"),
            groupId = buildPageContent.data("groupid");
        console.log('[UniverseLoader.js] Issuing GET /build/universes with params:', {
            startRow: startRow,
            activeOnly: activeOnly,
            groupId: groupId
        });
        $.ajax({
                type: "GET",
                url: "/build/universes",
                data: {
                    startRow: startRow,
                    activeOnly: activeOnly,
                    groupId: groupId
                },
                cache: !1,
                dataType: "html",
                success: function (html) {
                    console.log('[UniverseLoader.js] /build/universes SUCCESS; html length:', html && html.length);
                    // loadMoreLink.remove(), f.hide();
                    var newContent = $(html).hide();
                    container.append(newContent);
                    newContent.fadeIn();
                    console.log('[UniverseLoader.js] Appended universes; new item-table count:', container.find('>.item-table').length);
                    newContent.find("a[data-retry-url]").loadRobloxThumbnails()
                },
                fail: function () {
                    console.log('[UniverseLoader.js] /build/universes FAIL; re-showing load-more-universes link');
                    // loadMoreLink.show(), f.hide();
                }
            })
    }

    function i() {
        console.log('[UniverseLoader.js] init() wiring events for active-only checkbox & load-more-universes');
        $("body").on("change", t, function () {
            console.log('[UniverseLoader.js] Active-only checkbox changed; reloading universes for container around this checkbox');
            n($(this).closest(".content-area").find(">.items-container"), !0)
        });
        $("body").on("click", ".load-more-universes", function () {
            console.log('[UniverseLoader.js] .load-more-universes clicked');
            return n($(this).parent()), !1
        })
    }
    var t = ".active-only-checkbox > input[type='checkbox']";
    console.log('[UniverseLoader.js] Defining Roblox.BuildPage.UniverseLoader; binding init on document ready');
    return $(i), {
        init: i,
        loadUniverses: n
    }
}();