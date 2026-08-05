// ~/viewapp/pages/accounts/services/accountsService.js
"use strict";
accounts.factory("accountsService", ["$q", "$log", "httpService", function(n, t, i) {
    var r = {
            getAccountInfoUrl: "/my/settings/json",
            updateAccountInfoUrl: "/my/account/update-json",
            accountSignoutUrl: "/authentication/signoutfromallsessionsandreauthenticate",
            accountChangePasswordUrl: "/account/changepassword",
            accountAddEmailAddressUrl: "/account/changeemail",
            accountVerifyEmailAddressUrl: "/my/account/sendverifyemail",
            accountUnblockUserUrl: "/userblock/unblockuser",
            accountAskParentToVerifyAge: "/my/account/AskParentVerifyAge",
            accountChangeUsername: "/account/username/update",
            getAllowedDestinationTypes: "/account/settings/allowed-notification-destinations",
            accountAppChatPrivacy: "/account/settings/app-chat-privacy",
            accountGameChatPrivacy: "/account/settings/game-chat-privacy",
            accountPrivateMessagePrivacy: "/account/settings/private-message-privacy",
            accountPrivateServerInvitePrivacy: "/account/settings/private-server-invite-privacy",
            accountFollowMePrivacy: "/account/settings/follow-me-privacy",
            accountTradePrivacy: "/account/settings/trade-privacy",
            accountTradeValue: "/account/settings/trade-value",
            accountAccountRestrictions: "/account/settings/account-restrictions",
            accountSocialNetworks: "/account/settings/social-networks",
            accountDescription: "/account/settings/description",
            accountBirthdate: "/account/settings/birthdate",
            accountCountryLegacy: "/account/settings/country",
            accountCountry: "/account/settings/account-country",
            accountGender: "/account/settings/gender",
            accountPhone: "/account/settings/phone",
            accountPhoneDelete: "/account/settings/phone/delete",
            accountResendPhoneCode: "/account/settings/phone/resend",
            accountVerifyPhone: "/account/settings/phone/verify",
            getCountryListUrl: "/account/settings/countries"
        },
        u = function(n, t, r, u) {
            var f = {
                    url: n,
                    withCredentials: !0
                },
                e;
            switch (r) {
                case i.methods.get:
                    return f.noCache = !u, i.httpGet(f, t);
                case i.methods.post:
                    return f.headers = {
                        "Content-Type": "application/x-www-form-urlencoded"
                    }, t != null && (e = $.param(t)), i.httpPost(f, e);
                case i.methods.delete:
                    return i.httpDelete(f, t)
            }
        };
    return {
        setNotificationSettingsUrls: function(n) {
            r.getNotificationSettings = n + "/v2/notifications/get-settings", r.updateNotificationBandSettings = n + "/v2/notifications/update-notification-settings", r.removeDestinationTypeOptOut = n + "/v2/notifications/receiver-destination-types/allow", r.addDestinationTypeOptOut = n + "/v2/notifications/receiver-destination-types/opt-out"
        },
        setXboxConnectionUrls: function(n) {
            r.xboxConnection = n + "/v1/xbox/connection", r.disconnectXbox = n + "/v1/xbox/disconnect"
        },
        setApiProxyUrl: function(n) {
            r.enable2svUrl = n + "/account/two-step-enabled"
        },
        setAuthDomainUrl: function(n) {
            r.accountPinUrl = n + "/v1/account/pin", r.accountPinLockUrl = n + "/v1/account/pin/lock", r.accountPinUnlockUrl = n + "/v1/account/pin/unlock", r.connectedSocialAuthUrl = n + "/v1/social/connected-providers", r.socialAuthDomainUrl = n + "/v1/social"
        },
        templateUrls: {
            passwordChangedUrl: "user-account-password-changed.html",
            passwordErrorUrl: "user-account-password-error.html",
            accountPhoneChangedUrl: "user-account-phone-changed.html",
            accountVerifySmsSentUrl: "user-account-verify-sms-sent.html",
            accountEmailChangedUrl: "user-account-email-changed.html",
            accountVerifySentUrl: "user-account-verify-sent.html",
            accountParentNeedNeedEmailUrl: "user-account-parent-need-email.html",
            accountAgeChangeConfirmationUrl: "user-account-age-change-confirmation.html",
            account2SvEnabledUrl: "user-account-2sv-enabled.html",
            account2SvDisableConfirmationUrl: "user-account-2sv-disable-confirmation.html"
        },
        populateAccount: {
            years: [],
            days: [],
            months: [{
                name: "Jan",
                value: 1
            }, {
                name: "Feb",
                value: 2
            }, {
                name: "Mar",
                value: 3
            }, {
                name: "Apr",
                value: 4
            }, {
                name: "May",
                value: 5
            }, {
                name: "Jun",
                value: 6
            }, {
                name: "Jul",
                value: 7
            }, {
                name: "Aug",
                value: 8
            }, {
                name: "Sep",
                value: 9
            }, {
                name: "Oct",
                value: 10
            }, {
                name: "Nov",
                value: 11
            }, {
                name: "Dec",
                value: 12
            }],
            privacy: [{
                name: "Everyone",
                value: "All"
            }, {
                name: "Friends, Users I Follow, and Followers",
                value: "Followers"
            }, {
                name: "Friends and Users I Follow",
                value: "Following"
            }, {
                name: "Friends",
                value: "Friends"
            }, {
                name: "No one",
                value: "NoOne"
            }],
            visibility: [{
                name: "Everyone",
                value: "AllUsers"
            }, {
                name: "Friends, Users I Follow, and Followers",
                value: "FriendsFollowingAndFollowers"
            }, {
                name: "Friends and Users I Follow",
                value: "FriendsAndFollowing"
            }, {
                name: "Friends",
                value: "Friends"
            }, {
                name: "No one",
                value: "NoOne"
            }],
            under13Privacy: [{
                name: "Friends",
                value: "Friends"
            }, {
                name: "No one",
                value: "NoOne"
            }],
            contactSettingsConfiguration: [{
                name: "Default",
                value: "Default"
            }, {
                name: "Custom",
                value: "Custom"
            }, {
                name: "Off",
                value: "Off"
            }],
            messaging: [{
                name: "Everyone",
                value: "All"
            }, {
                name: "Friends, Users I Follow, and Followers",
                value: "Followers"
            }, {
                name: "Friends and Users I Follow",
                value: "Following"
            }, {
                name: "Friends",
                value: "Friends"
            }, {
                name: "No one",
                value: "NoOne"
            }],
            appChat: [{
                name: "Friends",
                value: "Friends"
            }, {
                name: "No one",
                value: "NoOne"
            }],
            gameChat: [{
                name: "Everyone",
                value: "AllUsers"
            }, {
                name: "No one",
                value: "NoOne"
            }],
            tradeValue: [{
                name: "None",
                value: 0
            }, {
                name: "Low",
                value: 1
            }, {
                name: "Medium",
                value: 2
            }, {
                name: "High",
                value: 3
            }],
            gender: {
                MALE: "2",
                FEMALE: "3"
            }
        },
        beginGetCountryList: function() {
            var t, n;
            return n = r.getCountryListUrl, u(n, t, "GET")
        },
        beginGetAccountInfo: function() {
            var n, t;
            return n = r.getAccountInfoUrl, u(n, t, "GET")
        },
        beginGetAllowedNotificationDestinationTypes: function() {
            var n, t;
            return n = r.getAllowedDestinationTypes, u(n, t, "GET")
        },
        beginUpdateAccountInfo: function(n) {
            var t, i;
            return t = r.updateAccountInfoUrl, i = {
                __RequestVerificationToken: angular.element("[name=__RequestVerificationToken]").val(),
                ReceiveNewsletter: n.ReceiveNewsletter,
                ThemeId: 47,
                Facebook: n.Facebook,
                Twitter: n.Twitter,
                GooglePlus: n.GooglePlus,
                YouTube: n.YouTube,
                Twitch: n.Twitch,
                SocialNetworksVisibilityPrivacy: n.SocialNetworksVisibilityPrivacyValue
            }, u(t, i, "POST")
        },
        beginAccountSignOut: function() {
            var n, t;
            return n = r.accountSignoutUrl, t = {
                __RequestVerificationToken: angular.element("[name=__RequestVerificationToken]").val()
            }, u(n, t, "POST")
        },
        beginAccountChangePassword: function(n) {
            var t, i;
            return t = r.accountChangePasswordUrl, i = {
                oldPassword: n.oldPassword,
                newPassword: n.newPassword,
                confirmNewPassword: n.confirmPassword
            }, u(t, i, "POST")
        },
        beginAccountAddEmailAddress: function(n) {
            var t, i;
            return t = r.accountAddEmailAddressUrl, i = {
                emailAddress: n.emailAddress,
                password: n.password
            }, u(t, i, "POST")
        },
        beginAccountVerifyEmailAddress: function() {
            var n;
            return n = r.accountVerifyEmailAddressUrl, u(n, null, "POST")
        },
        beginGetPhone: function() {
            var n = r.accountPhone;
            return u(n, null, "GET")
        },
        beginAccountUpdatePhone: function(n) {
            var t = r.accountPhone,
                i = {
                    countryCode: n.countryCode,
                    prefix: n.prefix,
                    phone: n.phone,
                    password: n.password
                };
            return u(t, i, "POST")
        },
        beginAccountDeletePhone: function(n) {
            var t = r.accountPhoneDelete,
                i = {
                    password: n.password
                };
            return u(t, i, "POST")
        },
        beginAccountResendPhoneCode: function() {
            var n = r.accountResendPhoneCode;
            return u(n, null, "POST")
        },
        beginAccountVerifyPhone: function(n) {
            var t = r.accountVerifyPhone,
                i = {
                    code: n.code
                };
            return u(t, i, "POST")
        },
        beginAccountUnblockUser: function(n) {
            var t, i;
            return t = r.accountUnblockUserUrl, i = {
                blockeeId: n
            }, u(t, i, "POST")
        },
        beginAccountAskParentToVerifyAge: function() {
            var n;
            return n = r.accountAskParentToVerifyAge, u(n, null, "POST")
        },
        beginAccountChangeUsername: function(n) {
            var t, i;
            return t = r.accountChangeUsername, i = {
                __RequestVerificationToken: angular.element("[name=__RequestVerificationToken]").val(),
                username: n.username,
                password: n.password
            }, u(t, i, "POST")
        },
        beginGetNotificationSettings: function() {
            var n = r.getNotificationSettings;
            return u(n, null, "GET", !0)
        },
        beginGet2svSeting: function() {
            var n = r.enable2svUrl;
            return u(n, null, "GET", !0)
        },
        beginUpdate2svSetting: function(n) {
            var t = r.enable2svUrl,
                i = {
                    isEnabled: n
                };
            return u(t, i, "POST", !0)
        },
        beginUpdateNotificationSettings: function(t, i, f) {
            var e = [],
                h = r.updateNotificationBandSettings,
                o, s;
            return t.length > 0 && e.push(u(h, {
                updatedSettings: t
            }, "POST", !0)), o = r.removeDestinationTypeOptOut, angular.forEach(f, function(n) {
                e.push(u(o, {
                    destinationType: n
                }, "POST", !0))
            }), s = r.addDestinationTypeOptOut, angular.forEach(i, function(n) {
                e.push(u(s, {
                    destinationType: n
                }, "POST", !0))
            }), n.all(e)
        },
        beginGetSocialConnected: function() {
            var n = r.connectedSocialAuthUrl;
            return u(n, null, "GET", !0)
        },
        beginUpdateSocialDisconnect: function(n) {
            r.socialAuthDomainUrl = r.socialAuthDomainUrl + "/" + n + "/disconnect";
            var t = r.socialAuthDomainUrl;
            return u(t, null, "POST")
        },
        beginGetAccountPinSetting: function() {
            var n = r.accountPinUrl;
            return u(n, null, "GET", !0)
        },
        beginCreateAccountPinSetting: function(n) {
            var t = r.accountPinUrl,
                i = {
                    pin: n
                };
            return u(t, i, "POST", !0)
        },
        beginDeleteAccountPinSetting: function() {
            var n = r.accountPinUrl;
            return u(n, null, "DELETE", !0)
        },
        beginUnlockAccountPinSetting: function(n) {
            var t = r.accountPinUnlockUrl,
                i = {
                    pin: n
                };
            return u(t, i, "POST", !0)
        },
        beginLockAccountPinSetting: function() {
            var t = r.accountPinLockUrl;
            return u(t, null, "POST", !0)
        },
        beginGetXboxConnection: function() {
            var n = r.xboxConnection;
            return u(n, null, "GET", !0)
        },
        beginDisconnectXbox: function() {
            var n = r.disconnectXbox;
            return u(n, null, "POST")
        },
        beginProcessErrorMessage: function(n) {
            var t = n ? n : "Something went wrong, please try again later.";
            return n && n.errors && n.errors[0] && n.errors[0].message && (t = n.errors[0].message), t
        },
        beginGetDescription: function() {
            var n = r.accountDescription;
            return u(n, null, "GET", !0)
        },
        beginUpdateDescription: function(n) {
            var t = r.accountDescription,
                i = {
                    Description: n.description
                };
            return u(t, i, "POST")
        },
        beginGetBirthdate: function() {
            var n = r.accountBirthdate;
            return u(n, null, "GET", !0)
        },
        beginUpdateBirthdate: function(n) {
            var t = r.accountBirthdate,
                i = {
                    BirthDay: n.birthdate.birthDay,
                    BirthMonth: n.birthdate.birthMonth,
                    BirthYear: n.birthdate.birthYear
                };
            return u(t, i, "POST")
        },
        beginGetGender: function() {
            var n = r.accountGender;
            return u(n, null, "GET", !0)
        },
        beginUpdateGender: function(n) {
            var t = r.accountGender,
                i = {
                    Gender: n.gender
                };
            return u(t, i, "POST")
        },
        beginGetLegacyCountry: function() {
            var n = r.accountCountryLegacy;
            return u(n, null, "GET", !0)
        },
        beginGetCountry: function() {
            var n = r.accountCountry;
            return u(n, null, "GET", !0)
        },
        beginUpdateLegacyCountry: function(n) {
            var t = r.accountCountryLegacy,
                i = {
                    CountryId: n.country.countryId
                };
            return u(t, i, "POST")
        },
        beginUpdateCountry: function(n) {
            var t = r.accountCountry,
                i = {
                    countryId: n.country.countryId
                };
            return u(t, i, "POST")
        },
        beginGetSocialNetworks: function() {
            var n = r.accountSocialNetworks;
            return u(n, null, "GET", !0)
        },
        beginUpdateSocialNetworks: function(n) {
            var t = r.accountSocialNetworks,
                i = {
                    SocialNetworksVisibilityPrivacy: n.SocialNetworksVisibilityPrivacy,
                    FacebookUrl: n.FacebookUrl,
                    TwitterUrl: n.TwitterUrl,
                    GooglePlusUrl: n.GooglePlusUrl,
                    YouTubeUrl: n.YouTubeUrl,
                    TwitchUrl: n.TwitchUrl
                };
            return u(t, i, "POST", !0)
        },
        beginGetAccountRestrictions: function() {
            var n = r.accountAccountRestrictions;
            return u(n, null, "GET", !0)
        },
        beginUpdateAccountRestrictions: function(n) {
            var t = r.accountAccountRestrictions,
                i = {
                    isEnabled: n
                };
            return u(t, i, "POST", !0)
        },
        beginGetContactSettings: function() {
            var t = r.accountAppChatPrivacy,
                i = r.accountGameChatPrivacy,
                f = r.accountPrivateMessagePrivacy,
                e = {
                    appChatPrivacy: u(t, null, "GET", !0),
                    gameChatPrivacy: u(i, null, "GET", !0),
                    privateMessagePrivacy: u(f, null, "GET", !0)
                };
            return n.all(e).then(function(n) {
                var t = {};
                return t.appChatPrivacy = n.appChatPrivacy.AppChatPrivacy, t.gameChatPrivacy = n.gameChatPrivacy.GameChatPrivacy, t.privateMessagePrivacy = n.privateMessagePrivacy.PrivateMessagePrivacy, t
            })
        },
        beginGetAppChatPrivacy: function() {
            var n = r.accountAppChatPrivacy;
            return u(n, null, "GET", !0)
        },
        beginUpdateAppChatPrivacy: function(n) {
            var t = r.accountAppChatPrivacy,
                i = {
                    AppChatPrivacy: n.AppChatPrivacy
                };
            return u(t, i, "POST")
        },
        beginGetGameChatPrivacy: function() {
            var n = r.accountGameChatPrivacy;
            return u(n, null, "GET", !0)
        },
        beginUpdateGameChatPrivacy: function(n) {
            var t = r.accountGameChatPrivacy,
                i = {
                    GameChatPrivacy: n.GameChatPrivacy
                };
            return u(t, i, "POST")
        },
        beginGetPrivateMessagePrivacy: function() {
            var n = r.accountPrivateMessagePrivacy;
            return u(n, null, "GET", !0)
        },
        beginUpdatePrivateMessagePrivacy: function(n) {
            var t = r.accountPrivateMessagePrivacy,
                i = {
                    PrivateMessagePrivacy: n.PrivateMessagePrivacy
                };
            return u(t, i, "POST")
        },
        beginGetPrivateServerInvitePrivacy: function() {
            var n = r.accountPrivateServerInvitePrivacy;
            return u(n, null, "GET", !0)
        },
        beginUpdatePrivateServerInvitePrivacy: function(n) {
            var t = r.accountPrivateServerInvitePrivacy,
                i = {
                    PrivateServerInvitePrivacy: n.privateServerInvitePrivacy
                };
            return u(t, i, "POST")
        },
        beginGetFollowMePrivacy: function() {
            var n = r.accountFollowMePrivacy;
            return u(n, null, "GET", !0)
        },
        beginUpdateFollowMePrivacy: function(n) {
            var t = r.accountFollowMePrivacy,
                i = {
                    FollowMePrivacy: n.followMePrivacy
                };
            return u(t, i, "POST")
        },
        beginGetTradePrivacy: function() {
            var n = r.accountTradePrivacy;
            return u(n, null, "GET", !0)
        },
        beginUpdateTradePrivacy: function(n) {
            var t = r.accountTradePrivacy,
                i = {
                    TradePrivacy: n.tradePrivacy
                };
            return u(t, i, "POST")
        },
        beginGetTradeValue: function() {
            var n = r.accountTradeValue;
            return u(n, null, "GET", !0)
        },
        beginUpdateTradeValue: function(n) {
            var t = r.accountTradeValue,
                i = {
                    TradeValue: n.tradeValue
                };
            return u(t, i, "POST")
        }
    }
}]);