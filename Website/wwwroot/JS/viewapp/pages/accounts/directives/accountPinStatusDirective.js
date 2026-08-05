// ~/viewapp/pages/accounts/directives/accountPinStatusDirective.js
accounts.directive("accountPinStatus", ["accountConstantsResources", function(n) {
    return {
        restrict: "A",
        templateUrl: n.templates.accountPinStatus,
        link: function() {}
    }
}]);