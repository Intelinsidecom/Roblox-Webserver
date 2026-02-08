// Universes/PlaceSelectorModal.js

var Roblox = Roblox || {};

Roblox.PlaceSelector = function() {

    function i(n) {
        if (!n) {
            return null;
        }
        var u = n.name ? n.name.escapeHTML() : "Unnamed Place",
            r = n.gameName || "None",
            t, i;
        return r == "" && (r = "None"), t = $(".place-selector.template").clone().removeClass("template"), t.show(), i = t.find(".place-image").data("retry-url-template"), 
        i = i ? i.replace("PLACE_ID", n.placeId) : "", 
        t.find(".place-image").attr("data-retry-url", i), t.attr("title", u), t.attr("data-placeId", n.placeId), t.attr("data-notSelectable", (n.ignoreRootPlace && n.isRootPlace) || n.isInUniverse), (n.ignoreRootPlace && n.isRootPlace) && (t.removeClass("selectable"), t.addClass("not-selectable"), t.find(".root-place").show()), t.find(".place-name").text(n.name || "Unnamed Place").attr("title", u), t.find(".game-name-text").text(r).attr("title", r.escapeHTML()), t
    }



    function r(n, i) {
        var r = $("#universe-configure").data("place-loader-url");
        
        if (!r) {
            $("#PlaceSelectorItemContainer").addClass("empty").html('<div style="text-align: center; padding: 20px;">How did we got there</div>');
            return;
        }
        
        r = r + "&startIndex=" + n + "&maxRows=" + i;
        $.ajax({
            type: "GET",
            url: r,
            contentType: "application/json; charset=utf-8",
            cache: !1,
            success: function(response) {
                Roblox.PlaceSelectorPager.update(response)
            },
            error: function(xhr, status, error) {
                $("#PlaceSelectorItemContainer").addClass("empty").html('<div style="text-align: center; padding: 20px;">Failed to load places. Please try again later.</div>')
            }
        })
    }



    function u() {
        var placeElements = $("#PlaceSelectorItemContainer .place-image[data-retry-url]");
        if (placeElements.length > 0) {
            placeElements.loadRobloxThumbnails();
        }
    }



    function f(i, r) {
        var modalElement = $(".PlaceSelectorModal").first();
        
        if (modalElement.length === 0) {
            // Fallback: create the modal dynamically if it's not found
            var modalHtml = '<div class="PlaceSelectorModal modalPopup unifiedModal smallModal GenericModal" style="display:none;">' +
                '<div class="Title">Select Place</div>' +
                '<div class="GenericModalBody text">' +
                '<div class="place-selector-content" data-place-loader-url="' + $("#universe-configure").data("place-loader-url") + '">' +
                '<div class="place-selector-container">' +
                '<div id="PlaceSelectorItemContainer" class="place-selector-item-container"></div>' +
                '<div id="PlaceSelectorPagerContainer" class="place-selector-pager-container"></div>' +
                '</div>' +
                '<div class="place-selector selectable template" title="Place" style="display: none">' +
                '<div class="place-image" data-retry-url-template="/game-thumbnails/json?assetId=PLACE_ID">' +
                '<img alt="Place" class="item-image" src="/images/ec5c01d220bf1b73403fa51519267742.gif" />' +
                '</div>' +
                '<div class="place-info">' +
                '<div class="place-name"></div>' +
                '<div class="game-name"><span class="form-label">Game:</span><span class="game-name-text"></span></div>' +
                '<div class="root-place" style="display: none"><span>Cannot choose start places</span></div>' +
                '</div>' +
                '<div style="clear:both;"></div>' +
                '</div>' +
                '</div>' +
                '</div>';
            
            $('body').append(modalHtml);
            modalElement = $(".PlaceSelectorModal");
        }
        
        if (modalElement.length === 0) {
            $("#PlaceSelectorItemContainer").addClass("empty").html('<div style="text-align: center; padding: 20px;">How did we got there</div>');
            return;
        }
        
        t = r, Roblox.PlaceSelectorPager = new DataPager(0, 5, "PlaceSelectorItemContainer", "PlaceSelectorPagerContainer", Roblox.PlaceSelector.GetPlaceSelector, Roblox.PlaceSelector.FormatPlaceSelectorHTML, Roblox.PlaceSelector.FormatPlaceSelectorCallback, {
            Paging_PageNumbers_AreLinks: !1
        }), n = i;
        
        modalElement.modal({
            overlayClose: !0,
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            maxWidth: 800,
        });
    }



    function e() {

        $.modal.close()

    }



    function o() {

        $(document).on("click", ".PlaceSelectorModal .place-selector", function() {

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