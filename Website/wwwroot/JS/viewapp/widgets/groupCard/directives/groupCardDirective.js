// ~/viewapp/widgets/groupCard/directives/groupCardDirective.js
"use strict";
groupCard.directive("groupCard", ["layout", function(n) {
    return {
        restrict: "A",
        scope: {
            groupItem: "=item"
        },
        templateUrl: n.templateLinks.groupCard
    }
}]);