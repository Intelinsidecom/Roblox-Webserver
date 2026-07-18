// ~/viewapp/common/assetsExplorer/controllers/assetsExplorerController.js
"use strict";
assetsExplorer.controller("assetsExplorerController", ["$scope", function(n) {
    n.staticData = Roblox.AssetsExplorerModel, n.currentData = n.currentData ? n.currentData : {
        currentPage: 1,
        totalPages: 1,
        category: null,
        subcategory: null,
        nextPageCursor: null,
        previousPageCursor: null,
        thumbSize: {
            thumbDefault: 110,
            thumbLarge: 140,
            thumbWidth: 0,
            thumbHeight: 0
        },
        assetTypeUrl: n.staticData.absoluteCatalogUrl,
        AssetTypeId: 0,
        itemSection: "catalog",
        templateVisible: !0
    }
}]);