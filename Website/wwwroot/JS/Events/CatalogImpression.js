// Events/CatalogImpression.js
var Roblox = Roblox || {};
Roblox.Catalog = Roblox.Catalog || {}, Roblox.Catalog.ImpressionCounter = function() {
    return fireImpression = function(n, t) {
        $.ajax({
            url: t,
            cache: !1,
            type: "post",
            data: {
                assetIds: n.split(",")
            },
            traditional: !0,
            xhrFields: {
                withCredentials: !0
            }
        })
    }, {
        fireImpression: fireImpression
    }
}();