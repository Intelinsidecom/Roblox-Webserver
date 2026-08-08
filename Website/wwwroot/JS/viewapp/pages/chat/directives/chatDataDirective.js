// ~/viewapp/pages/chat/directives/chatDataDirective.js
"use strict";
chat.directive("chatData", ["chatService", "partyService", "ngAudio", "chatUtility", "chatHybridService", "googleAnalyticsEventsService", "$log", function(n, t, i, r, u, f) {
    return {
        restrict: "A",
        scope: {
            chatViewModel: "=",
            chatLibrary: "="
        },
        link: function(e, o, s) {
            var h = "https://chat." + s.domain,
                c = "https://notifications." + s.domain;
            e.chatViewModel = {
                chatDomain: h,
                friendsDict: {},
                friendsHasConversation: [],
                chatUserDict: {},
                signalRDomain: c,
                signalRHubName: "userNotificationHub",
                convIdsForInvitedParties: [],
                invitedParties: {}
            }, e.chatLibrary.domain = s.domain, e.chatLibrary.gamesPageLink = s.gamespagelink, r.party.gamesPageLink = s.gamespagelink, e.chatLibrary.userId = parseInt(s.userid), e.chatLibrary.spinner = s.spinner, e.chatLibrary.deviceType = s.devicetype, e.chatLibrary.inApp = s.inapp === "true", e.chatLibrary.togglechatbarenabled = s.togglechatbarenabled === "true", e.chatLibrary.tabletInApp = s.devicetype === r.deviceType.TABLET && s.inapp === "true", e.chatLibrary.chatInPhone = s.devicetype === r.deviceType.PHONE && s.inapp === "true", e.chatLibrary.maxNumberOfPartyMembers = s.numberofmembersforpartychrome, e.chatLibrary.quotaOfPartyMembers = s.numberofmembersforpartychrome - 1, e.chatLibrary.intervalOfChangeTitleForPartyChrome = s.intervalofchangetitleforpartychrome, e.chatLibrary.audio = i.load(Roblox.Chat.SoundFile), e.chatLibrary.cookieOption = {
                domain: s.domain,
                path: "/",
                expires: null
            }, e.chatLibrary.dialogLocalStorageName = "dialogLibrary_" + s.domain, e.chatLibrary.cleanPartyFromConversationEnabled = s.cleanpartyfromconversationenabled === "true", e.chatLibrary.googleAnalyticsEvent = {
                category: f.eventCategories.JSErrors,
                action: e.chatLibrary.inApp ? f.eventActions.ChatEmbedded : f.eventActions.Chat
            }, n.setParams(h), n.setAvatarHeadshotsMultigetLimit(s.avatarheadshotsmultigetlimit), n.setUserPresenceMultigetLimit(s.userpresencemultigetlimit), t.setParams(h), r.setInApp(e.chatLibrary.inApp), e.chatLibrary.layout = {
                inputHeight: 32,
                topBarHeight: 32,
                dialogHeight: 320,
                bannerHeight: 40,
                searchHeight: 34
            }, e.chatLibrary.inApp && (e.chatLibrary.inAppLayout = {}, u.getTopBarHeight(function(n, t) {
                e.chatLibrary.inAppLayout = {
                    inputHeight: e.chatLibrary.tabletInApp ? r.chatLayout.heightOfTabletInput : r.chatLayout.heightOfMobileInput,
                    topBarHeight: parseInt(t.topBarHeight),
                    dialogHeight: "100%",
                    bannerHeight: 40,
                    searchHeight: 42
                }, r.setInAppLayout(e.chatLibrary)
            })), e.chatLibrary.tabletInApp && (e.chatLibrary.chatPlaceholderEnabled = !0)
        }
    }
}]);