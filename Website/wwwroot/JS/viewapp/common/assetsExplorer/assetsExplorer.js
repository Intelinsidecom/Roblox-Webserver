// ~/viewapp/common/assetsExplorer/assetsExplorer.js
"use strict";
var assetsExplorer = angular.module("assetsExplorer", ["ui.router", "robloxApp.helpers"]).config(["$stateProvider", "$urlRouterProvider", "$locationProvider", function(n, t, i) {
    var o = function(n) {
            return n.split(" ").join("-").toLowerCase()
        },
        c = function(n) {
            return "/" + o(n.name)
        },
        a = function(n, t) {
            return "/" + o(n.name) + "/" + o(t.name)
        },
        u, r, h, f, e;
    if (typeof Roblox == "undefined" || typeof Roblox.AssetsExplorerModel == "undefined") throw new Error("Roblox.AssetsExplorerModel should be defined");
    var l = Roblox.AssetsExplorerModel.assetCategories,
        v = Roblox.AssetsExplorerModel.defaultCategory,
        s = window.location.href;
    for (s.indexOf("#") !== -1 && s.indexOf("#!") === -1 && (window.location.href = s.replace("#", "#!")), i.html5Mode(!1), i.hashPrefix("!"), t.otherwise(c(v)), u = 0; u < l.length; u++)
        for (r = l[u], n.state(r.name, {
                url: c(r),
                category: r,
                subcategory: r.items[0]
            }), h = r.items, f = 0; f < h.length; f++) e = h[f], n.state(r.name + "/" + e.name, {
            url: a(r, e),
            category: r,
            subcategory: e
        })
}]);