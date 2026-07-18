// ~/viewapp/widgets/slideSwitcher/directives/slideSwitcherDirective.js
"use strict";
slideSwitcher.directive("slideSwitcher", ["$rootScope", "$timeout", function(n, t) {
    return {
        restrict: "A",
        scope: {
            collection: "="
        },
        templateUrl: "slide-switcher",
        link: function(i) {
            function f() {
                t(function() {
                    n.$emit("lazyImg:refresh")
                }, 0)
            }
            i.curIdx = 0, i.slideNext = function() {
                i.curIdx + 1 <= i.collection.length - 1 ? i.curIdx++ : i.curIdx = 0, f()
            }, i.slidePrev = function() {
                i.curIdx - 1 >= 0 ? i.curIdx-- : i.curIdx = i.collection.length - 1, f()
            }, i.shouldPreLoad = function(n) {
                var t = i.collection.length - 1;
                return i.curIdx === n || (i.curIdx - 1 >= 0 ? n === i.curIdx - 1 : n === t) || (i.curIdx + 1 <= t ? n === i.curIdx + 1 : n === 0)
            }
        }
    }
}]);