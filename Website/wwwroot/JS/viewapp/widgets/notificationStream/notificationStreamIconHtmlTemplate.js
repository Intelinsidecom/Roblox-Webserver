"use strict";
var notificationStreamIconHtmlTemplate = angular.module("notificationStreamIconHtmlTemplate", []).run(["$templateCache", function($templateCache) {
    $templateCache.put("notification-indicator", '<a id="nav-ns-icon" class="roblox-popover rbx-menu-item notification-stream-icon" data-bind="notification-stream-base" data-container="notification-stream-container"> <span class="icon-nav-notification-stream" id="nav-notifications"></span> <span class="notification-red notification" ng-show="layout.unreadNotifications > 0 &amp;&amp; (!layout.isNotificationContentOpen)"> {{layout.unreadNotifications | abbreivateCount}} </span> </a>');
    $templateCache.put("notification-stream", '<div class="notification-stream" ng-class="{\'inApp\': library.inApp}"> <div notification-indicator></div> </div>');
}]);
