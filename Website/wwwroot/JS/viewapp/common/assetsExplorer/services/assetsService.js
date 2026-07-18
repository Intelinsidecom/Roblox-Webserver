// ~/viewapp/common/assetsExplorer/services/assetsService.js
"use strict";
assetsExplorer.factory("assetsService", ["httpService", "$log", function(n, t) {
    return {
        currentPage: 0,
        itemsPerPage: 30,
        userId: Roblox.AssetsExplorerModel.userId,
        assetTypeId: 0,
        getItemSection: function(n) {
            switch (n.name) {
                case "Decals":
                case "Models":
                case "Audio":
                case "Plugins":
                case "Animations":
                case "Meshes":
                    return "library"
            }
            switch (n.name) {
                case "Places":
                case "Badges":
                case "Game Passes":
                    return null
            }
            return "catalog"
        },
        setCategory: function(n) {
            this.currentCategory = n;
            var t = n.items[0];
            return this.setSubcategory(t), !0
        },
        setSubcategory: function(n) {
            return n !== null && typeof n != "undefined" && (this.currentSubcategory = n, this.assetTypeId = this.currentSubcategory.id), !0
        },
        setPage: function(n) {
            return n < 0 ? (t.debug("Invalid attempt to set page to page " + n), !1) : (this.currentPage = n, !0)
        },
        beginUpdateAssetsItems: function(t, i) {
            var e = "/users/" + i + "/list-json",
                o = this.itemsPerPage,
                s = this.currentPage,
                r = {
                    userId: this.userId,
                    assetTypeId: this.assetTypeId,
                    pageNumber: s,
                    thumbWidth: t,
                    thumbHeight: t,
                    itemsPerPage: o
                },
                u, f;
            return this.currentSubcategory != null && this.currentSubcategory.filter != null && (u = this.currentSubcategory.filter, r.placeTab = u), f = {
                url: e,
                noCache: !0
            }, n.httpGet(f, r)
        }
    }
}]);