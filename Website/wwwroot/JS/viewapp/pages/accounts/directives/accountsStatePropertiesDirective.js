// ~/viewapp/pages/accounts/directives/accountsStatePropertiesDirective.js
"use strict";
accounts.directive("accountStateProperties", function() {
    return {
        restrict: "A",
        scope: {
            accountData: "="
        },
        link: function(n, t, i) {
            n.accountData.stateProperties = {
                countries: JSON.parse(i.countries),
                cancelRenewalUrl: i.cancelrenewalurl,
                upgradeMemberShipUrl: i.upgrademembershipurl,
                buyRobuxUrl: i.buyrobuxurl,
                isTwoStepToggleEnabled: i.isTwoStepToggleEnabled,
                checkIfInvalidUsernameForSignup: i.checkifinvalidusernameforsignup,
                inApp: i.inApp === "true"
            }
        }
    }
});