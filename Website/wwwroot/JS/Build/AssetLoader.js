// /js/Build/AssetLoader.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.BuildPage == "undefined" && (Roblox.BuildPage = {}), Roblox.BuildPage.AssetLoader = Roblox.BuildPage.AssetLoader || function() {
    function n(n, t) {
        var u = Number(t.val()),
            f = Number(n.data("groupid")),
            r = {
                assetTypeId: i
            };
        return u && (r.targetPlaceId = u), f && (r.groupId = f), r
    }

    function f(t, i, r) {
        var u = n(t, r);
        return u.startRow = i ? t.find(".items-container > .item-table").length : 0, "/build/assets?" + $.param(u)
    }

    function r(t, i) {
        var r = n(t, i);
        return "/build/upload?" + $.param(r)
    }

    function e(t, i) {
        var r = n(t, i);
        return "/build/game-passes?" + $.param(r)
    }

    function t(n, t) {
        var i = n.find(".asset-place-creationcontext-drop-down"),
            r = n.find(".load-more-items"),
            e = f(n, !t, i);
        u(n, t, i, r, e)
    }

    function u(n, t, i, r, u) {
        var f = n.find(".items-container"),
            o, e;
        t && f.html(""), r.hide(), o = f.closest(".BuildPageContent"), e = o.find(".build-loading-container").show(), $.ajax({
            type: "GET",
            url: u,
            cache: !1,
            dataType: "html",
            success: function(n) {
                r.remove(), e.hide();
                var t = $(n).hide();
                f.append(t), t.fadeIn(), t.find("a[data-retry-url]").loadRobloxThumbnails()
            },
            fail: function() {
                r.show(), e.hide()
            }
        })
    }

    function o() {
        i = Number($("#assetTypeId").val());
        $("body").on("click", ".load-more-items", function() {
            return t($(this).closest(".BuildPageContent"), !1), !1
        });
        $("body").on("change", ".asset-place-creationcontext-drop-down", function() {
            var n = $(this).closest(".BuildPageContent"),
                i = n.find(".asset-place-creationcontext-drop-down");
            t(n, !0), n.find("#upload-iframe").attr("src", r(n, i))
        });
        $("body").on("change", ".game-pass-place-creationcontext-drop-down", function() {
            var n = $(this).closest(".BuildPageContent"),
                t = n.find(".game-pass-place-creationcontext-drop-down"),
                i = n.find(".load-more-game-passes"),
                f = e(n, t);
            u(n, !0, t, i, f), n.find("#upload-iframe").attr("src", r(n, t))
        })
    }
    var i;
    return $(o), {
        loadAssets: t
    }
}();