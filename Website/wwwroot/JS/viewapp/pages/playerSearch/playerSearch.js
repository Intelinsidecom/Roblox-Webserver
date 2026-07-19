// ~/viewapp/pages/playerSearch/playerSearch.js
"use strict";
var playerSearch = angular.module("playerSearch", ["robloxApp.helpers"]).config(["$stateProvider", "$urlRouterProvider", "$locationProvider", function(n, t, i) {
    i.html5Mode({
        enabled: !0,
        requireBase: !1
    })
}]);