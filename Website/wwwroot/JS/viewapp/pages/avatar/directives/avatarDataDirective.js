// ~/viewapp/pages/avatar/directives/avatarDataDirective.js
"use strict";
avatar.directive("avatarData", ["$log", function(n) {
    return {
        restrict: "A",
        scope: {
            avatarViewModel: "="
        },
        link: function(t, i, r) {
            var u;
            n.debug("avatarDataDirective is linking");
            var f = parseInt(r.userId),
                e = r.avatarDomain,
                o = parseFloat(r.scaleHeightIncrement),
                s = parseFloat(r.scaleWidthIncrement),
                h = parseFloat(r.scaleHeadIncrement),
                c = parseInt(r.hatAssetTypeId),
                l = r.loadingThumbnailUrl,
                a = angular.fromJson(r.assetTypeToCatalogUrlMap),
                v = r.scaleHeadEnabled === "true",
                y = r.enableDefaultClothingMessage === "true",
                p = r.showDefaultClothingMessageOnPageLoad === "true";
            n.debug(r), u = {
                loadingThumbnailUrl: l,
                hatAssetTypeId: c,
                userId: f,
                avatarDomain: e,
                scaleHeightIncrement: o,
                scaleWidthIncrement: s,
                scaleHeadIncrement: h,
                assetTypeToCatalogUrlMap: a,
                scaleHeadEnabled: v,
                enableDefaultClothingMessage: y,
                showDefaultClothingMessageOnPageLoad: p
            }, t.$parent.metaDataDeferred.resolve(u)
        }
    }
}]);