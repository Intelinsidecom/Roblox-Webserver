// IDE/PublishAs.js
var Roblox = Roblox || {};
Roblox.PublishAs = function() {
    var n = function() {
            return $("div.asset:not(#newasset):visible").length
        },
        i = function(t, i) {
            var r = "/ide/placelist" + (i ? "/" + i : ""),
                u;
            return t && (u = "?startRow=" + n(), r += u), r
        },
        r = function(t, i) {
            var r = "/ide/publish/listmodels" + (i ? "/" + i : ""),
                u;
            return t && (u = "?startRow=" + n(), r += u), r
        },
        u = function(t, i) {
            var r = "/studio/animations" + (i ? "/" + i : ""),
                u;
            return t && (u = "?startRow=" + n(), r += u), r
        },
        f = function(t, i) {
            var r = "/studio/plugins" + (i ? "/" + i : ""),
                u;
            return t && (u = "?startRow=" + n(), r += u), r
        },
        t = function(n, t) {
            $.ajax({
                url: t,
                cache: !1,
                dataType: "html",
                success: function(t) {
                    var i = $(t),
                        r;
                    n.parent().append(i), n.remove(), i.animate({
                        opacity: 1
                    }, "fast"), r = i.find("a[data-retry-url]"), r.loadRobloxThumbnails()
                }
            })
        },
        e = function() {
            $("#closeButton").click(function() {
                return window.close(), !1
            });
            $("#assetList,#groupAssetList").on("click", "#load-more-assets", function() {
                var n = $(this),
                    e = Number($("#groupAssetList .group-select select:visible").val()) || 0,
                    o = n.data("asset-type");
                return o === "model" ? t(n.parent(), r(!0, e)) : o === "animation" ? t(n.parent(), u(!0, e)) : o === "plugin" ? t(n.parent(), f(!0, e)) : t(n.parent(), i(!0, e)), !1
            });
            $(".group-select").on("change", "select", function() {
                var t = $(this),
                    i = t.find(":selected").val(),
                    r = t.data("url"),
                    n = $("#groupAssetList .content");
                n.find(".loading").length || n.append("<div class='loading'></div>"), $.ajax({
                    url: r + "/" + i,
                    cache: !1,
                    dataType: "html",
                    success: function(t) {
                        n.find(".gameplace, .list-item, #MoreGames, .loading").remove(), n.append($(t)), $(t).animate({
                            opacity: 1
                        }, "fast");
                        var i = $(t).find("a[data-retry-url]");
                        i.loadRobloxThumbnails()
                    }
                })
            });
            $("#groupAssetList").on("click", "#newasset", function() {
                var n = $("#groupAssetList select").find(":selected").val(),
                    t = $(this).data("url"),
                    i = $("#UniverseID").val();
                window.location.href = t + "/" + n + "?universeId=" + i
            });
            $("#gameAssetList").on("click", "#newasset", function() {
                var n = $(this).data("url");
                window.location.href = n
            })
        };
    return {
        Initialize: e
    }
}();