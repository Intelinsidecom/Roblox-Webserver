// ~/viewapp/pages/friends/friends.js
"use strict";
var friends = angular.module("friends", ["ui.router", "ui.bootstrap", "captcha"]).config(["$stateProvider", "$urlRouterProvider", "$locationProvider", function(n, t, i) {
    var r = window.location.href;
    r.indexOf("#") != -1 && r.indexOf("#!") == -1 && (window.location.href = r.replace("#", "#!")), i.html5Mode(!1), i.hashPrefix("!"), t.otherwise("/friends"), n.state("friends", {
        url: "/friends",
        label: "Friends",
        authenticate: !1
    }).state("following", {
        url: "/following",
        label: "Following",
        authenticate: !1
    }).state("followers", {
        url: "/followers",
        label: "Followers",
        authenticate: !1
    }).state("friend-requests", {
        url: "/friend-requests",
        label: "Friend Requests",
        authenticate: !0
    })
}]);