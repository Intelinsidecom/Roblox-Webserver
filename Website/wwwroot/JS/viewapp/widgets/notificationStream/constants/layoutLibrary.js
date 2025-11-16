notificationStream.constant("layoutLibrary", {
    links: {
        profileLinkName:"goToProfilePage", profileLink:"/users/{id}/profile", friendRequestLinkName:"viewAllFriendRequests", friendRequestTabName:"Friends", friendRequestLink:"/users/friends#!/friend-requests", settingLinkName:"goToSettingPage", settingTabName:"Settings", settingLink:"/my/account#!/notifications", friendsTabName:"Friends", friendsLink:"/users/friends", inboxTabName:"Messages", inboxLink:"/my/messages/#!/inbox", inboxMessageDetailQuery:"?conversationId="
    }

    , stringTemplates: {
        boldLink:"<a class='font-bold'>{username}</a>", userLink:"<a class='text-name small' type='goToProfilePage' user_id='{userid}' href='{profilelink}'>{username}</a>"
    }

    , friendRequestReceivedLayout: {
        defaultPrefixText:"You have ", defaultPostfixTextBySingle:" new friend request.", defaultPostfixTextByMulti:" new friend requests.", friendRequestTextBySingle:" sent you a friend request.", friendRequestTextByMulti:" sent you friend requests.", requestConfirmedTextBySingle:" is now your friend!", requestConfirmedTextByMulti:" are now your friends!"
    }

    , friendRequestAcceptedLayout: {
        defaultPrefixText:"You have ", defaultPostfixTextBySingle:" new friend.", defaultPostfixTextByMulti:" new friends.", friendRequestAcceptedText:" accepted your friend request.", requestConfirmedText:""
    }

    , textTemplate: {
        newNotificationPostfix:" New Notification", noNetworkConnectionText:"Connecting..."
    }

    , friendRequestActionType: {
        acceptIgnoreBtns:"AcceptIgnoreBtns", chatBtn:"chatBtn", viewAllBtn:"ViewAllBtn"
    }

    , directiveTemplatesName: {
        notificationIndicatorTemplate:"notification-indicator", notificationContentTemplate:"notification-content", friendRequestReceivedTemplate:"friend-request-received", friendRequestAcceptedTemplate:"friend-request-accepted", friendRequestTemplate:"friend-request", privateMessageTemplate:"private-message", testTemplate:"test"
    }

    , notificationSourceType: {
        test:"Test", friendRequestReceived:"FriendRequestReceived", friendRequestAccepted:"FriendRequestAccepted", privateMessageReceived:"PrivateMessageReceived"
    }
});