// ~/viewapp/pages/messages/messages.js
"use strict";
var messages = angular.module("messages", ["ui.router"]).config(["$stateProvider", "$urlRouterProvider", "$locationProvider", function(n, t, i) {
    var r = window.location.href;
    r.indexOf("#") != -1 && r.indexOf("#!") == -1 && (window.location.href = r.replace("#", "#!")), i.html5Mode(!1), i.hashPrefix("!"), t.otherwise("/inbox"), n.state("inbox", {
        url: "/inbox?page&messageIdx&conversationId",
        templateUrl: Roblox.websiteTemplates.messageTemplate,
        controller: "messagesContentController"
    }).state("sent", {
        url: "/sent?page&messageIdx",
        templateUrl: Roblox.websiteTemplates.messageTemplate,
        controller: "messagesContentController"
    }).state("notifications", {
        url: "/notifications",
        templateUrl: Roblox.websiteTemplates.notificationTemplate,
        controller: "messagesContentController"
    }).state("archive", {
        url: "/archive?page&messageIdx",
        templateUrl: Roblox.websiteTemplates.messageTemplate,
        controller: "messagesContentController"
    })
}]);