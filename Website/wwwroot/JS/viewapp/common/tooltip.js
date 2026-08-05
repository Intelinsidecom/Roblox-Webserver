// ~/viewapp/common/tooltip.js
angular.module("ui.bootstrap").config(["$provide", function(n) {
    n.decorator("tooltipPopupDirective", ["$delegate", function(n) {
        return n[0].templateUrl = Roblox.uiBootstrap.tooltipPopupTemplateLink, n
    }]), n.decorator("tooltipHtmlUnsafeDirective", ["$delegate", function(n) {
        return n[0].templateUrl = Roblox.uiBootstrap.tooltipHtmlUnsafePopupTemplateLink, n
    }])
}]);