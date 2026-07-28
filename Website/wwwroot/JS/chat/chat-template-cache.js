// Pre-populate $templateCache with tooltip/popover/modal templates
// from ui-bootstrap-custom-tpls-2.5.0.min.js.
// These templates are registered via standalone module .run() blocks
// that never execute because the modules aren't in the bootstrap dependency tree.
// Without this, Angular fetches them via HTTP, gets 404s, and the resulting
// $apply/$digest cycles cause an infinite loop that freezes the page.
(function() {
    angular.module("robloxApp").run(["$templateCache", function($templateCache) {
        $templateCache.put("uib/template/tooltip/tooltip-popup.html",
            '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner" ng-bind="content"></div>\n');
        $templateCache.put("uib/template/tooltip/tooltip-html-popup.html",
            '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner" ng-bind-html="contentExp()"></div>\n');
        $templateCache.put("uib/template/tooltip/tooltip-template-popup.html",
            '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner"\n  uib-tooltip-template-transclude="contentExp()"\n  tooltip-template-transclude-scope="originScope()"></div>\n');
        $templateCache.put("uib/template/modal/window.html",
            '<div class="modal-dialog {{size ? \'modal-\' + size : \'\'}}"><div class="modal-content" uib-modal-transclude></div></div>\n');
    }]);
})();
