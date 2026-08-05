// ~/viewapp/pages/accounts/accounts.js
"use strict";
var accounts = angular.module("accounts", ["ui.router", "ui.bootstrap", "ui.bootstrap.v2", "modal", "notificationStream"]).config(["$stateProvider", "$urlRouterProvider", "$locationProvider", "accountConstantsResources", function(n, t, i, r) {
    var u = window.location.href;
    u.indexOf("#") != -1 && u.indexOf("#!") == -1 && (window.location.href = u.replace("#", "#!")), i.html5Mode(!1), i.hashPrefix("!"), t.otherwise("/info"), n.state("info", {
        url: "/info",
        label: "Account Info",
        templateUrl: r.templates.accountInfo
    }).state("social", {
        url: "/social",
        label: "Social",
        templateUrl: r.templates.accountSocial
    }).state("security", {
        url: "/security",
        label: "Security",
        templateUrl: r.templates.accountSecurity
    }).state("privacy", {
        url: "/privacy",
        label: "Privacy",
        templateUrl: r.templates.accountPrivacy
    }).state("billing", {
        url: "/billing",
        label: "Billing",
        templateUrl: r.templates.accountBilling
    }).state("notifications", {
        url: "/notifications",
        label: "Notifications",
        templateUrl: r.templates.accountNotifications
    })
}]);