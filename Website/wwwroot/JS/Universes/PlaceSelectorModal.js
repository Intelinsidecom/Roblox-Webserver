// Universes/PlaceSelectorModal.js
var Roblox = Roblox || {};
Roblox.PlaceSelector = function() {
    function i(n) {
        var u = n.name.escapeHTML(),
            r = n.gameName,
            t, i;
        return r == "" && (r = "None"), t = $(".place-selector.template").clone().removeClass("template"), t.show(), i = t.find(".place-image").data("retry-url-template"), i += i.indexOf("?") !== -1 ? "&" : "?", i += "assetId=" + n.placeId, t.find(".place-image").attr("data-retry-url", i), t.attr("title", u), t.attr("data-placeId", n.placeId), t.attr("data-notSelectable", n.ignoreRootPlace && n.isRootPlace), n.ignoreRootPlace && n.isRootPlace && (t.removeClass("selectable"), t.addClass("not-selectable"), t.find(".root-place").show()), t.find(".place-name").text(n.name).attr("title", u), t.find(".game-name-text").text(r).attr("title", r.escapeHTML()), t
    }

    function r(n, i) {
        var r = $(".place-selector-modal").data("place-loader-url");
        r = r + "&startIndex=" + n + "&maxRows=" + i, t !== undefined && (r = t(n, i)), $.ajax({
            type: "GET",
            url: r,
            contentType: "application/json; charset=utf-8",
            cache: !1,
            success: function(n) {
                Roblox.PlaceSelectorPager.update(n)
            },
            error: function() {
                $("#PlaceSelectorItemContainer").addClass("empty").text(anErrorOccurred)
            }
        })
    }

    function u() {
        $("[data-retry-url]").loadRobloxThumbnails()
    }

    function f(i, r) {
        t = r, Roblox.PlaceSelectorPager = new DataPager(0, 5, "PlaceSelectorItemContainer", "PlaceSelectorPagerContainer", Roblox.PlaceSelector.GetPlaceSelector, Roblox.PlaceSelector.FormatPlaceSelectorHTML, Roblox.PlaceSelector.FormatPlaceSelectorCallback, {
            Paging_PageNumbers_AreLinks: !1
        }), n = i, $(".PlaceSelectorModal").modal({
            overlayClose: !0,
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            }
        })
    }

    function e() {
        $.modal.close()
    }

    function o() {
        $(document).on("click", ".place-selector-modal .place-selector", function() {
            var i = $(this).data("notselectable"),
                t;
            i || (t = $(this).data("placeid"), n !== undefined && n(t), Roblox.PlaceSelector.Close())
        })
    }
    var n, t;
    return {
        Init: o,
        GetPlaceSelector: r,
        FormatPlaceSelectorHTML: i,
        FormatPlaceSelectorCallback: u,
        Open: f,
        Close: e
    }
}();