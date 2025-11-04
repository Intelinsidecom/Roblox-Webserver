// ~/viewapp/common/helpers.js
"use strict";
angular.module("robloxApp.helpers", []).directive("focusMe", ["$timeout", function(n) {
    return {
        scope: {
            trigger: "@focusMe"
        },
        link: function(t, i, r) {
            t.$watch(function() {
                return r.focusMe
            }, function(u) {
                u && n(function() {
                    t.$eval(r.focusMe) && i[0].focus()
                }, 0, !1)
            })
        }
    }
}]).directive("numbersOnly", function() {
    return {
        require: "ngModel",
        link: function(n, t, i, r) {
            function u(n) {
                if (n == undefined) return "";
                var t = n.replace(/[^0-9]/g, "");
                return t !== n && (r.$setViewValue(t), r.$render()), t
            }
            r.$parsers.push(u)
        }
    }
}).directive("phone", function() {
    return {
        require: "ngModel",
        link: function(n, t, i, r) {
            function u(n) {
                if (n == undefined) return "";
                var t = n.replace(/[^0-9-+()]/g, "");
                return t !== n && (r.$setViewValue(t), r.$render()), t
            }
            r.$parsers.push(u)
        }
    }
}).directive("keyPress", function() {
    return {
        scope: {
            selection: "="
        },
        controller: ["$anchorScroll", "$location", "$scope", function(n, t, i) {
            i.gotoAnchor = function(r) {
                var u = "anchor-" + r;
                t.hash() !== u ? (t.hash("anchor-" + r), i.selection.selectedIndex = angular.element(document.querySelector("#" + u)).attr("data-index")) : n()
            }
        }],
        link: function(n, t) {
            t.bind("keydown", function(t) {
                t.stopPropagation(), t.which === 38 && (n.selection.selectedIndex = n.selection.selectedIndex == 0 ? n.selection.listLength : n.selection.selectedIndex - 1), t.which === 40 && (n.selection.selectedIndex = n.selection.selectedIndex == n.selection.listLength ? 0 : +n.selection.selectedIndex + 1), n.$apply()
            }), t.bind("keypress", function(t) {
                if (t.stopPropagation(), t.which >= 65 && t.which <= 122) var i = String.fromCharCode(t.which);
                n.$apply()
            })
        }
    }
}).directive("imageRetry", ["robloxImagesService", "$log", function(n) {
    return {
        restrict: "A",
        scope: {
            thumbnail: "="
        },
        link: function(t, i, r) {
            var u = function(t) {
                t && !t.Final && n.getImageUrl(t, function(n) {
                    r.$set("src", n)
                }, 0)
            };
            u(t.thumbnail), t.$watch(function() {
                return t.thumbnail
            }, function(n) {
                n && u(n)
            })
        }
    }
}]).directive("switcher", ["$log", function() {
    return {
        restrict: "A",
        controller: ["$scope", function(n) {
            n.switcher = {}, n.switcher.games = {
                currPage: 0,
                itemsCount: 0
            }, n.switcher.groups = {
                currPage: 0,
                itemsCount: 0
            }
        }],
        scope: {
            currpage: "=",
            itemscount: "="
        },
        link: function(n, t) {
            var r = null;
            n.currpage = 0, t.find(".switcher-item") && t.find(".switcher-item").length > 0 && (n.itemscount = t.find(".switcher-item").length);
            t.find(".carousel-control").on("click", function(i) {
                n.$apply(function() {
                    angular.element(i.currentTarget).attr("data-switch") == "next" ? n.currpage + 1 <= n.itemscount - 1 ? n.currpage += 1 : n.currpage = 0 : n.currpage - 1 >= 0 ? n.currpage -= 1 : n.currpage = n.itemscount - 1
                }), r = t.find(".switcher-item[data-index=" + n.currpage + "] img"), r.attr("src") || r.attr("src", r.attr("data-src"))
            })
        }
    }
}]).directive("keyPressEnter", function() {
    return function(n, t, i) {
        t.bind("keydown keypress", function(t) {
            t.which === 13 && (n.$apply(function() {
                n.$eval(i.keyPressEnter)
            }), t.preventDefault())
        })
    }
}).directive("keyPressEscape", function() {
    return function(n, t, i) {
        t.bind("keydown keypress", function(t) {
            t.which === 27 && (n.$apply(function() {
                n.$eval(i.keyPressEscape)
            }), t.preventDefault())
        })
    }
}).directive("formValidation", function() {
    return {
        require: ["^form", "ngModel"],
        restrict: "A",
        link: function(n, t, i, r) {
            n.$watch(function() {
                var n = r[1];
                return n.$viewValue
            }, function(n) {
                var u, t, i, f;
                typeof Roblox.FormEvents != "undefined" && (u = r[0], t = r[1], t.$dirty && t.$invalid && (i = [], angular.forEach(t.$error, function(n, t) {
                    n === !0 && i.push(t)
                }), f = t.redactedInput ? "[Redacted]" : n, Roblox.FormEvents.SendValidationFailed(u.$name, t.$name, f, i.join(","))))
            })
        }
    }
}).directive("onFinishRender", ["$timeout", function(n) {
    return {
        restrict: "A",
        link: function(t, i, r) {
            t.$last === !0 && n(function() {
                t.$emit(r.onFinishRender)
            })
        }
    }
}]);