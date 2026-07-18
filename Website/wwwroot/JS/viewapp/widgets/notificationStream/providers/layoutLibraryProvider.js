// notificationStream/providers/layoutLibraryProvider.js
"use strict";

if (typeof robloxApp !== 'undefined') {
    robloxApp.provider("layoutLibraryProvider", function() {
        this.$get = ['$window', '$rootScope', function($window, $rootScope) {
            return {
                inApp: false,
                isPhone: false,
                isTablet: false,
                isDesktop: true,
                
                getDeviceType: function() {
                    if (this.isPhone) return 'phone';
                    if (this.isTablet) return 'tablet';
                    if (this.inApp) return 'app';
                    return 'desktop';
                },
                
                isNotificationContentOpen: false,
                
                setNotificationContentOpen: function(isOpen) {
                    this.isNotificationContentOpen = isOpen;
                    $rootScope.$broadcast('notificationContentStateChanged', { isOpen: isOpen });
                },
                
                getLayoutConfig: function() {
                    return {
                        deviceType: this.getDeviceType(),
                        inApp: this.inApp,
                        isPhone: this.isPhone,
                        isTablet: this.isTablet,
                        isDesktop: this.isDesktop,
                        isNotificationContentOpen: this.isNotificationContentOpen
                    };
                },
                
                initialize: function() {
                    var userAgent = navigator.userAgent.toLowerCase();
                    var screenWidth = $window.innerWidth || $window.screen.width;
                    
                    this.isPhone = screenWidth <= 767;
                    this.isTablet = screenWidth > 767 && screenWidth <= 991;
                    this.isDesktop = screenWidth > 991;
                    this.inApp = userAgent.indexOf('robloxapp') > -1 || 
                               userAgent.indexOf('robloxmobile') > -1;
                }
            };
        }]
    });
}

if (typeof notificationStream !== 'undefined') {
    notificationStream.provider("layoutLibraryProvider", function() {
        this.$get = function() {
            return {
                links: {
                    profileLinkName: "goToProfilePage",
                    profileLink: "/users/{id}/profile",
                    friendRequestLinkName: "viewAllFriendRequests",
                    friendRequestTabName: "Friends",
                    friendRequestLink: "/users/friends#!/friend-requests",
                    settingLinkName: "goToSettingPage",
                    settingTabName: "Settings",
                    settingLink: "/my/account#!/notifications",
                    friendsTabName: "Friends",
                    friendsLink: "/users/friends",
                    inboxTabName: "Messages",
                    inboxLink: "/my/messages/#!/inbox",
                    inboxMessageDetailQuery: "?conversationId="
                },
                stringTemplates: {
                    boldLink: "<a class='font-bold'>{username}</a>",
                    userLink: "<a class='text-name small' type='goToProfilePage' user_id='{userid}' href='{profilelink}'>{username}</a>"
                },
                friendRequestReceivedLayout: {
                    defaultPrefixText: "You have ",
                    defaultPostfixTextBySingle: " new friend request.",
                    defaultPostfixTextByMulti: " new friend requests.",
                    friendRequestTextBySingle: " sent you a friend request.",
                    friendRequestTextByMulti: " sent you friend requests.",
                    requestConfirmedTextBySingle: " is now your friend!",
                    requestConfirmedTextByMulti: " are now your friends!"
                },
                friendRequestAcceptedLayout: {
                    defaultPrefixText: "You have ",
                    defaultPostfixTextBySingle: " new friend.",
                    defaultPostfixTextByMulti: " new friends.",
                    friendRequestAcceptedText: " accepted your friend request.",
                    requestConfirmedText: ""
                },
                textTemplate: {
                    newNotificationPostfix: " New Notification",
                    noNetworkConnectionText: "Connecting..."
                },
                friendRequestActionType: {
                    acceptIgnoreBtns: "AcceptIgnoreBtns",
                    chatBtn: "chatBtn",
                    viewAllBtn: "ViewAllBtn"
                },
                directiveTemplatesName: {
                    notificationIndicatorTemplate: "notification-indicator",
                    notificationContentTemplate: "notification-content",
                    friendRequestReceivedTemplate: "friend-request-received",
                    friendRequestAcceptedTemplate: "friend-request-accepted",
                    friendRequestTemplate: "friend-request",
                    privateMessageTemplate: "private-message",
                    testTemplate: "test"
                },
                notificationSourceType: {
                    test: "Test",
                    friendRequestReceived: "FriendRequestReceived",
                    friendRequestAccepted: "FriendRequestAccepted",
                    privateMessageReceived: "PrivateMessageReceived",
                    commentOnAsset: "CommentOnAsset",
                    assetPurchased: "AssetPurchased",
                    assetFavorited: "AssetFavorited"
                }
            };
        };
    });
}

