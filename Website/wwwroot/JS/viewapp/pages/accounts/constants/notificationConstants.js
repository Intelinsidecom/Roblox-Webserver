// ~/viewapp/pages/accounts/constants/notificationConstants.js
"use strict";
accounts.constant("notificationConstants", {
    notificationSourceTypeMapping: {
        FriendRequestReceived: "I receive a friend request",
        FriendRequestAccepted: "Someone accepts my friend request",
        PartyInviteReceived: "Someone invites me to a party",
        PartyMemberJoined: "Someone joins the party I'm in",
        ChatNewMessage: "Someone chats with me",
        PrivateMessageReceived: "I receive a private message",
        UserAddedToPrivateServerWhiteList: "I am invited to a VIP server"
    },
    destinationTypes: ["NotificationStream", "DesktopPush", "MobilePush"],
    modalText: {
        success: {
            title: "Success",
            body: "Saved Successfully!"
        },
        error: {
            title: "Error",
            body: "Sorry, there was an error updating your notifications settings. Try again later."
        }
    },
    receiverDestinationTypeMapping: [{
        destinationType: "NotificationStream",
        friendlyTitle: "Notification Stream",
        blurbText: "See notifications in my stream. Click the notifications icon in the top bar to view these notifications.",
        actionWhenText: "Notify me when",
        actionDescriptionText: "After you turn off a notification type, we won't send you any new notifications of that type.",
        areSourcesShown: !0,
        isToggleable: function() {
            return !1
        }
    }, {
        destinationType: "DesktopPush",
        friendlyTitle: "Desktop Push",
        blurbText: "See notifications on this computer even when Roblox is closed.",
        secondaryBlurbText: "To see notifications, you may be prompted to turn on push notifications on your browser.",
        actionWhenText: "Notify me when",
        actionDescriptionText: "Desktop notifications for this device."
    }, {
        destinationType: "MobilePush",
        friendlyTitle: "Mobile Push",
        blurbText: "See notifications on your devices' home screens. You can turn them on or off from the Roblox app.",
        actionWhenText: "",
        areSourcesShown: !0,
        actionDescriptionText: "Mobile push notifications for this device."
    }]
});