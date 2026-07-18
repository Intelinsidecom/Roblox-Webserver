// ~/viewapp/pages/inventory/controllers/inventoryContentController.js
"use strict";
assetsExplorer.controller("inventoryContentController", ["$scope", "assetsService", "cursorPaginationService", "$document", "$location", "$log", "$q", "$timeout", function(n, t, i, r) {
    n.pageType = "inventory", n.assets = [], n.assetsPager = i.createPager({
        limitName: "itemsPerPage",
        sortOrder: i.sortOrder.Desc,
        pageSize: 30,
        loadPageSize: 50,
        getDataListFromResponse: function(n) {
            return n.Data ? n.Data.Items : []
        },
        getNextPageCursorFromResponse: function(n) {
            return n.Data ? n.Data.nextPageCursor : null
        },
        getErrorsFromResponse: function(n) {
            return n.isValid ? [] : [{
                code: 0,
                message: n.Data || n.error || ""
            }]
        },
        getCacheKeyParameters: function(n) {
            return {
                userId: n.userId,
                assetTypeId: n.assetTypeId,
                placeTab: n.placeTab
            }
        },
        getRequestUrl: function() {
            return "/users/inventory/list-json"
        },
        beforeLoad: function(t, i) {
            n.assetsPager.setPagingParameter("pageNumber", Number(i.cursor) || 1)
        },
        loadSuccess: function(t) {
            angular.forEach(t, function(n) {
                n.HasPrice = n.Product && n.Product.PriceInRobux
            }), n.assets = t, n.currentData.templateVisible = !0
        }
    }), n.assetsPager.setPagingParameter("userId", t.userId), n.$on("$stateChangeSuccess", function(i, u) {
        var s, h, c;
        n.currentData.templateVisible = !1, s = u.category, h = u.subcategory, n.currentData.category = s, n.currentData.subcategory = h, n.currentData.AssetTypeId = h.id, n.assetsPager.setPagingParameter("assetTypeId", h.id), n.assetsPager.setPagingParameter("placeTab", h.filter), s.name === "Places" ? (n.currentData.thumbSize.thumbWidth = n.currentData.thumbSize.thumbLarge, n.currentData.thumbSize.thumbHeight = n.currentData.thumbSize.thumbLarge) : (n.currentData.thumbSize.thumbWidth = n.currentData.thumbSize.thumbDefault, n.currentData.thumbSize.thumbHeight = n.currentData.thumbSize.thumbDefault), c = t.getItemSection(s), c === "library" ? (n.currentData.assetTypeUrl = n.staticData.absoluteLibraryUrl, n.currentData.itemSection = "library") : c === "catalog" ? (n.currentData.assetTypeUrl = n.staticData.absoluteCatalogUrl, n.currentData.itemSection = "catalog") : n.currentData.itemSection = null, r.triggerHandler("Roblox.Recommendations.GetItems", [n.currentData.AssetTypeId, s.name]), n.assetsPager.loadFirstPage()
    })
}]);