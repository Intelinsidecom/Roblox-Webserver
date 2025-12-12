// ~/viewapp/widgets/emailAndPhoneUpsell/controllers/emailAndPhoneUpsellController.js
"use strict";
emailAndPhoneUpsell.controller("emailAndPhoneUpsellController", ["$scope", "$window", "$uibModal", "modalService", "eventStreamService", "emailAndPhoneUpsellService", "emailAndPhoneUpsellConstants", function(n, t, i, r, u, f, e) {
    n.init = function() {
        Roblox.EmailAndPhoneUpsellMeta && (f.setAccountSettingsDomain(Roblox.EmailAndPhoneUpsellMeta.accountSettingsApiDomain), f.getScreen().then(function(t) {
            t && t.upsellScreenType && t.upsellScreenType !== e.screenTypes.none && n.displayModal(t)
        }))
    }, n.displayModal = function(t) {
        n.modalData = {
            upsellScreenType: t.upsellScreenType
        }, n.sendModalEvent(e.modalActions.shown), n.launchModal()
    }, n.launchModal = function() {
        var t = {
            animation: !1,
            backdrop: "static",
            templateUrl: "email-and-phone-upsell",
            scope: n,
            controller: "emailAndPhoneUpsellModalController"
        };
        i.open(t).result.then(function(n) {
            r.open({
                titleText: e.success,
                bodyText: n
            })
        }, function() {
            n.sendModalEvent(e.modalActions.dismissed)
        })
    }, n.sendModalEvent = function(t) {
        var i = e.modalEventContextPrefix + n.modalData.upsellScreenType;
        u.sendEventWithTarget(e.events.modalAction, i, {
            aType: t
        })
    }, n.init()
}]);