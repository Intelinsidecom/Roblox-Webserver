// ~/viewapp/common/directives/hoverPopoverDirective.js
"use strict";
robloxApp.directive("hoverPopover", ["$log", "$document", function(n, t) {
    return {
        restrict: "A",
        replace: !0,
        scope: {
            hoverPopoverParams: "="
        },
        link: function(n, i) {
            function o() {
                angular.element(u).on("mouseleave", function(t) {
                    if (n.hoverPopoverParams.isOpen) {
                        var i = angular.element(t.relatedTarget);
                        e(i) || n.$apply(function() {
                            n.hoverPopoverParams.isOpen = !1
                        })
                    }
                })
            }

            function e(n) {
                var t = angular.element(f);
                return t.find(n) && t.find(n).length > 0
            }

            function s(n) {
                var t = !1,
                    i = angular.element(u);
                return i.find(n) && i.find(n).length > 0 && (t = !0), t
            }
            var u = n.hoverPopoverParams.hoverPopoverSelector,
                f = n.hoverPopoverParams.triggerSelector;
            if (!n.hoverPopoverParams.isDisabled) {
                i.on("hover", function() {
                    n.$apply(function() {
                        n.hoverPopoverParams.isOpen = !0, o()
                    })
                });
                angular.element(f).on("mouseleave", function(t) {
                    if (n.hoverPopoverParams.isOpen) {
                        var i = angular.element(t.relatedTarget);
                        s(i) || e(i) || n.$apply(function() {
                            n.hoverPopoverParams.isOpen = !1
                        })
                    }
                });
                t.on("HoverPopover.EnableClose", function() {
                    n.hoverPopoverParams.isOpen && n.$apply(function() {
                        n.hoverPopoverParams.isOpen = !1
                    })
                })
            }
        }
    }
}]);