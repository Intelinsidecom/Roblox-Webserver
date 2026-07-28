// Chat-specific popover module for Angular UI Bootstrap 2.5.0
// Includes old 0.11.2 directives for rbxBootstrap.js compatibility
angular.module("ui.bootstrap.popover", ["ui.bootstrap.tooltip", "ui.bootstrap.tooltip.v2"])
// Old 0.11.2 directives (needed by rbxBootstrap.js decorator on ui.bootstrap)
.directive("popoverPopup", function() {
    return {
        restrict: "EA",
        replace: true,
        scope: { title: "@", content: "@", placement: "@", animation: "&", isOpen: "&" },
        templateUrl: "template/popover/popover.html"
    }
}).directive("popover", ["$tooltip", function(n) {
    return n("popover", "popover", "click")
}])
// New 2.5.0 directives (needed by chat bundle templates)
.directive("uibPopoverTemplatePopup", function() {
    return {
        restrict: "A",
        scope: { uibTitle: "@", contentExp: "&", originScope: "&" },
        templateUrl: "uib/template/popover/popover-template.html"
    }
}).directive("uibPopoverTemplate", ["$uibTooltip", function(n) {
    return n("uibPopoverTemplate", "popover", "click", { useContentExp: !0 })
}]).directive("uibPopoverHtmlPopup", function() {
    return {
        restrict: "A",
        scope: { contentExp: "&", uibTitle: "@" },
        templateUrl: "uib/template/popover/popover-html.html"
    }
}).directive("uibPopoverHtml", ["$uibTooltip", function(n) {
    return n("uibPopoverHtml", "popover", "click", { useContentExp: !0 })
}]).directive("uibPopoverPopup", function() {
    return {
        restrict: "A",
        scope: { uibTitle: "@", content: "@" },
        templateUrl: "uib/template/popover/popover.html"
    }
}).directive("uibPopover", ["$uibTooltip", function(n) {
    return n("uibPopover", "popover", "click")
}]);

angular.module("uib/template/popover/popover-html.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/popover/popover-html.html", '<div class="arrow"></div>\n\n<div class="popover-inner">\n    <h3 class="popover-title" ng-bind="uibTitle" ng-if="uibTitle"></h3>\n    <div class="popover-content" ng-bind-html="contentExp()"></div>\n</div>\n')
}]);
angular.module("uib/template/popover/popover-template.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/popover/popover-template.html", '<div class="arrow"></div>\n\n<div class="popover-inner">\n    <h3 class="popover-title" ng-bind="uibTitle" ng-if="uibTitle"></h3>\n    <div class="popover-content"\n      uib-tooltip-template-transclude="contentExp()"\n      tooltip-template-transclude-scope="originScope()"></div>\n</div>\n')
}]);
angular.module("uib/template/popover/popover.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/popover/popover.html", '<div class="arrow"></div>\n\n<div class="popover-inner">\n    <h3 class="popover-title" ng-bind="uibTitle" ng-if="uibTitle"></h3>\n    <div class="popover-content" ng-bind="content"></div>\n</div>\n')
}]);
