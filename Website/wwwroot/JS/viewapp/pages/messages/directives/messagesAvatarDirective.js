// ~/viewapp/pages/messages/directives/messagesAvatarDirective.js
"use strict";
messages.directive("rbxAvatar", ["robloxImagesService", "$log", function(n, t) {
    return {
        restrict: "A",
        scope: {
            thumbnail: "="
        },
        replace: !1,
        templateUrl: Roblox.websiteTemplates.avatarTemplate,
        link: function(i) {
            var f = i.$watch(function() {
                return i.thumbnail
            }, function(r) {
                r && (t.debug("---get thumbnail ! --" + r.Url), i.thumbnailUrl = r.Url, r.Final || n.getImageUrl(r, function(n) {
                    i.thumbnailUrl = n
                }, 0))
            }, !0);
            i.$on("$destroy", function() {
                f && f()
            })
        }
    }
}]);