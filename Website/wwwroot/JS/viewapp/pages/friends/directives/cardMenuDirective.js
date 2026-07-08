// ~/viewapp/pages/friends/directives/cardMenuDirective.js
"use strict";
friends.directive("cardMenu", ["$log", function() {
    return {
        restrict: "A",
        scope: !0,
        templateUrl: Roblox.FriendsTemplates.CardMenuTemplateLink,
        link: function(n, t) {
            var r, u;
            Roblox.BootstrapWidgets.SetupPopover();
            var f = ".popover-content #menu-" + n.friend.UserId + " .friend-unfollow",
                e = ".popover-content #menu-" + n.friend.UserId + " .friend-follow",
                o = ".popover-content #menu-" + n.friend.UserId + " .friend-unfriend",
                s = function() {
                    n.$apply(n.unFollow(n.friend))
                };
            t.on("click touchstart", f, s);
            r = function() {
                n.$apply(n.follow(n.friend))
            };
            t.on("click touchstart", e, r);
            u = function() {
                n.$apply(n.unFriend(n.friend))
            };
            t.on("click touchstart", o, u)
        }
    }
}]);