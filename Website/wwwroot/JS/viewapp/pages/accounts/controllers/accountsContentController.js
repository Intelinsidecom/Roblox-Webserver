// ~/viewapp/pages/accounts/controllers/accountsContentController.js
"use strict";
accounts.controller("accountsContentController", ["$scope", "$window", "$log", "$filter", "accountsService", "$timeout", "$interval", "$document", "$location", "$state", "$uibModal", "modalService", "notificationConstants", "modalConstants", "accountConstantsResources", function(n, t, i, r, u, f, e, o, s, h, c, l, a, v, y) {
    function w(n, t) {
        return l.open({
            titleText: n,
            bodyText: t,
            neutralButtonText: "OK"
        })
    }
    var et = "The social network link is not valid.",
        k = "Custom",
        it = "Default",
        tt = "Off",
        rt = "AllUsers",
        nt = "Friends",
        b = "NoOne",
        ft;
    n.isNewAccountCountrySettingEnabled = angular.element(document.getElementById("state-properties")).data("is-new-account-country-setting-enabled");
    var d = function() {
            var n = !1,
                r = !1,
                t = !1,
                e = function(i) {
                    t = !1, n = !1, r = !0, Roblox.PushNotificationRegistrar.isPushEnabled(function(u) {
                        r = !1, t = !0, n = u, f(function() {}), i(!0)
                    })
                },
                u = function() {
                    t = !1, n = !1
                },
                i = function() {
                    return Roblox && Roblox.PushNotificationRegistrar && Roblox.PushNotificationRegistrar.isPushSupported() && Roblox.PushNotificationRegistrationUI
                },
                o = function(u) {
                    return i() ? t ? n : (r || e(u), !1) : !1
                },
                s = function(n) {
                    i() && Roblox.PushNotificationRegistrationUI.enable(function() {
                        Roblox.PushNotificationRegistrar.getEventPublisher().Publish(Roblox.PushNotificationEventPublishers.RegistrationEventTypes.settingsPageEnabled), u(), n()
                    })
                },
                h = function(n) {
                    i() && Roblox.PushNotificationRegistrationUI.disable(function() {
                        Roblox.PushNotificationRegistrar.getEventPublisher().Publish(Roblox.PushNotificationEventPublishers.RegistrationEventTypes.settingsPageDisabled), u(), n()
                    })
                },
                c = function() {
                    return !0
                };
            return {
                isAvailable: i,
                isEnabled: o,
                enable: s,
                disable: h,
                isToggleable: c
            }
        }(),
        g = function() {
            var n = !1,
                i = !1,
                t = !1,
                r = function(r) {
                    t = !1, n = !1, i = !0, Roblox.NotificationSettingsAuthority.isPushEnabled(function(u) {
                        i = !1, t = !0, n = u, f(function() {}), r(!0)
                    })
                },
                u = function() {
                    t = !1, n = !1
                },
                e = function(u) {
                    return t ? n : (i || r(u), !1)
                },
                s = function() {
                    return !0
                },
                h = function() {
                    return Roblox.NotificationSettingsAuthority.canToggleMobilePushNotifications()
                },
                c = function() {
                    o.triggerHandler("Roblox.Settings.PushEnabledOnDevice"), n = !0
                },
                l = function(n) {
                    Roblox.NotificationSettingsAuthority.deregisterCurrentDevice(function() {
                        u(), n(!0)
                    })
                };
            return {
                isAvailable: s,
                isToggleable: h,
                enable: c,
                disable: l,
                isEnabled: e
            }
        }(),
        p = 100,
        ot = a.notificationSourceTypeMapping,
        ut = a.receiverDestinationTypeMapping;
    ut.forEach(function(t) {
        t.destinationType === "DesktopPush" && (t.getUnavailableReason = function() {
            var t = "Google Chrome versions " + n.desktopPushSettings.minimumChromeVersion + " and above",
                i = n.desktopPushSettings.enabledOnFirefox ? ", and Firefox versions 46 and above" : "";
            return "Desktop Push is currently only supported on " + t + i + "."
        }, t.areSourcesShown = !0, t.availabilityCheck = d.isAvailable, t.customEnablement = d.enable, t.customDisablement = d.disable, t.customEnabledCheck = d.isEnabled, t.isToggleable = d.isToggleable), t.destinationType === "MobilePush" && (t.availabilityCheck = g.isAvailable, t.customEnablement = g.enable, t.customDisablement = g.disable, t.customEnabledCheck = g.isEnabled, t.isToggleable = g.isToggleable)
    }), n.helpUrl = "https://en.help.roblox.com/entries/22631829", n.cancelRenewalUrl = angular.element(document.getElementById("state-properties")).attr("data-cancelrenewalurl"), n.showBlockedUsers = !1, n.populateAccount = u.populateAccount, n.accountPinContent = {
        isEnabled: !1,
        isUnlocked: !1,
        unlockedUntil: null,
        pinLength: 4
    }, n.parentContent = {
        accountRestrictions: {
            isFeatureEnabled: !1
        }
    }, n.twoStepContent = {
        isEnabled: !1
    }, n.userSocialSignOn = {
        isDisconnectFbEnabled: !1,
        isSocialSignOnBoxShown: !1,
        providerName: "",
        userName: ""
    }, n.processErrorMessage = function(n) {
        var t = n;
        return n && n.errors && n.errors[0] && n.errors[0].message && (t = n.errors[0].message), t
    }, n.gigyaUserInfo = function() {
        gigya.socialize.getUserInfo({
            callback: function(t) {
                n.$apply(function() {
                    n.userSocialSignOn.userName = t.UID !== null ? t.user.nickname : ""
                })
            }
        })
    }, n.xbox = {
        isDisconnectXboxEnabled: !1,
        isDisconnectXboxBoxShown: !1
    }, n.accountContent = {
        userInfo: {},
        bcExpireDate: "",
        hasEmailButNotVerified: !1,
        hasVerfiedEmail: !1,
        hasPassword: !0
    }, n.pinFields = {
        pinOne: "",
        pinTwo: "",
        pinThree: "",
        pinFour: ""
    }, n.passwordInfo = {
        newPassword: "",
        oldPassword: "",
        confirmPassword: ""
    }, n.notificationSettings = {
        notificationBandSettings: [],
        optedOutNotificationSourceTypes: [],
        optedOutReceiverDestinationTypes: [],
        updatedDestinationTypeOptOuts: {}
    }, n.notificationSourceTypeMapping = {}, n.receiverDestinationTypeMapping = [], n.contactSettings = {}, n.joinSettings = {}, n.accountRestrictions = {}, n.socialNetworks = {}, n.tradeSettings = {}, n.accountInfoSettings = {}, n.personal = {
        description: "",
        birthdate: {},
        gender: {},
        country: {}
    }, n.isV2Enabled = {
        socialNetworks: !1
    }, n.allActiveCountryList = {}, n.addOrChangePhone = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.addOrChangePhone, p);
            return
        }
        n.phoneModalData = angular.extend(n.phoneModalData, {
            codeLength: n.accountInfoSettings.verificationCodeLength
        });
        var t = {
            animation: !1,
            templateUrl: y.templates.modal.phoneSet,
            controller: y.controllers.modal.phoneSet,
            scope: n
        };
        c.open(t).result.then(function(t) {
            Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-success"), 100, 3e3, t.message), n.getPhoneNumber()
        })
    }, n.setAccountInfo = function(t) {
        n.accountContent.userInfo = {
            data: t,
            hasError: !1
        }, n.accountContent.bcExpireDate = n.convertDate(n.accountContent.userInfo.data.BcExpireDate), n.accountContent.hasVerifiedEmail = n.accountContent.userInfo.data.IsEmailOnFile && n.accountContent.userInfo.data.IsEmailVerified, n.accountContent.hasEmailButNotVerified = n.accountContent.userInfo.data.IsEmailOnFile && !n.accountContent.userInfo.data.IsEmailVerified, n.setXboxConnectionUrls(n.accountContent.userInfo.data.AuthDomain), n.setNotificationSettingsUrls(n.accountContent.userInfo.data.NotificationSettingsDomain), angular.forEach(n.accountContent.userInfo.data.AllowedNotificationSourceTypes, function(t) {
            n.notificationSourceTypeMapping[t] = ot[t]
        }), n.receiverDestinationTypeMapping = ut.filter(function(t) {
            return n.accountContent.userInfo.data.AllowedReceiverDestinationTypes.indexOf(t.destinationType) !== -1
        }), n.desktopPushSettings = {
            minimumChromeVersion: t.MinimumChromeVersionForPushNotifications,
            enabledOnFirefox: t.PushNotificationsEnabledOnFirefox
        }, n.getNotificationSettings(), n.setApiProxyUrl(n.accountContent.userInfo.data.ApiProxyDomain), n.get2SvSetting(), n.setAuthDomainUrl(n.accountContent.userInfo.data.AuthDomain), n.accountContent.userInfo.data.IsAccountPinEnabled && n.getAccountPinSetting(), n.setupDisconnectFb(n.accountContent.userInfo), n.getXboxConnection(), n.getAccountRestrictions(), n.prepForV2(t)
    }, n.getAccountInfo = function() {
        u.beginGetAccountInfo().then(function(t) {
            n.setAccountInfo(t)
        }, function(t) {
            n.accountContent.userInfo = {
                data: t,
                hasError: !0
            }
        })
    }, n.prepForV2 = function(t) {
        n.isV2Enabled.socialNetworks = t.IsAccountSettingsSocialNetworksV2Enabled, n.accountContent.isPhoneFeatureEnabled = t.IsPhoneFeatureEnabled, n.getAccountInfoSettings(), n.getPersonalSettings(), n.getJoinSettings(), n.getTradeSettings(), n.getContactSettings(), n.isV2Enabled.socialNetworks ? n.getSocialNetworks() : (n.socialNetworks.SocialNetworksVisibilityPrivacy = t.SocialNetworksVisibilityPrivacyValue, n.socialNetworks.FacebookUrl = t.Facebook, n.socialNetworks.TwitterUrl = t.Twitter, n.socialNetworks.GooglePlusUrl = t.GooglePlus, n.socialNetworks.YouTubeUrl = t.YouTube, n.socialNetworks.TwitchUrl = t.Twitch)
    }, n.getAccountInfoSettings = function() {
        n.getPhoneNumber()
    }, n.getPhoneNumber = function() {
        n.accountContent.isPhoneFeatureEnabled && u.beginGetPhone().then(function(t) {
            t.IsPhoneNumberVisible && (n.accountInfoSettings.isPhoneNumberVisible = t.IsPhoneNumberVisible, n.accountInfoSettings.countryCode = t.CountryCode, n.accountInfoSettings.prefix = t.Prefix, n.accountInfoSettings.phone = t.Phone, n.accountInfoSettings.isPhoneVerified = t.IsPhoneVerified, n.accountInfoSettings.verificationCodeLength = t.VerificationCodeLength)
        }, function() {
            i.debug("Error getting phone")
        })
    }, n.getPersonalSettings = function() {
        u.beginGetDescription().then(function(t) {
            n.personal.description = t.Description
        }, function() {
            i.debug("Error getting personal description")
        }), u.beginGetBirthdate().then(function(t) {
            var i = n.personal.birthdate;
            i.birthDay = t.BirthDay, i.birthMonth = t.BirthMonth, i.birthYear = t.BirthYear, n.personal.birthdate.fullBirthdate = function() {
                return new Date(i.birthYear, i.birthMonth - 1, i.birthDay)
            }, n.populateDays(), i.originalBirthdate = i.fullBirthdate()
        }, function() {
            i.debug("Error getting personal birthdate")
        }), n.isNewAccountCountrySettingEnabled === !0 ? u.beginGetCountry().then(function(t) {
            if (t.success) {
                var r = 0;
                t.countryId && (r = t.countryId), n.personal.country.countryId = r
            } else i.debug("Failed to get personal country. Message : " + t.errorMessage)
        }, function() {
            i.debug("Error getting personal country")
        }) : u.beginGetLegacyCountry().then(function(t) {
            n.personal.country.countryId = t.CountryId.toString()
        }, function() {
            i.debug("Error getting personal country")
        }), u.beginGetGender().then(function(t) {
            n.personal.gender = t.Gender.toString()
        }, function() {
            i.debug("Error getting personal country")
        })
    }, n.updateDescription = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateDescription, p);
            return
        }
        u.beginUpdateDescription(n.personal).then(function() {
            w(v.default.success.title, v.default.success.body)
        }, function() {
            w(v.default.error.title, v.default.error.body), i.debug("Error updating personal description")
        })
    }, n.updateBirthdate = function() {
        var e = function() {
                n.personal.birthdate.birthMonth = n.personal.birthdate.originalBirthdate.getMonth() + 1, n.personal.birthdate.birthDay = n.personal.birthdate.originalBirthdate.getDate(), n.personal.birthdate.birthYear = n.personal.birthdate.originalBirthdate.getFullYear(), n.populateDays()
            },
            f, r;
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateBirthdate, p, e);
            return
        }
        f = function(r) {
            u.beginUpdateBirthdate(n.personal).then(function() {
                r && t.location.reload();
                var u = n.personal.birthdate;
                u.fullBirthdate = function() {
                    return new Date(u.birthYear, u.birthMonth - 1, u.birthDay)
                }, u.originalBirthdate = u.fullBirthdate(), n.populateDays()
            }, function() {
                w(v.default.error.title, v.default.error.body), e(), i.debug("Error updating personal birthdate")
            })
        }, r = n.personal.birthdate.originalBirthdate && n.calculateAge(n.personal.birthdate.originalBirthdate) >= 13 && n.calculateAge(n.personal.birthdate.fullBirthdate()) < 13, r ? u.beginGetSocialConnected().then(function(t) {
            var i;
            return i = t && t.providers && t.providers[0] ? n.accountContent.userInfo.data.IsSetPasswordNotificationEnabled ? l.open({
                titleText: v.birthdayChange.needPassword.title,
                bodyText: v.birthdayChange.needPassword.body
            }) : l.open({
                titleText: v.birthdayChange.default.title,
                bodyText: v.birthdayChange.withSocialSignOn.body,
                actionButtonShow: !0,
                actionButtonText: v.birthdayChange.withSocialSignOn.positiveBtnText,
                neutralButtonText: v.birthdayChange.withSocialSignOn.negativeBtnText
            }) : l.open({
                titleText: v.birthdayChange.default.title,
                bodyText: v.birthdayChange.default.body,
                actionButtonShow: !0,
                actionButtonText: v.default.positiveBtnText,
                neutralButtonText: v.default.negativeBtnText
            }), i.result
        }).then(function() {
            f(r)
        }).catch(e) : f(r)
    }, n.updateGender = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateGender, p);
            return
        }
        u.beginUpdateGender(n.personal).then(function() {}, function() {
            i.debug("Error updating gender")
        })
    }, n.updateLegacyCountry = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateLegacyCountry, p);
            return
        }
        u.beginUpdateLegacyCountry(n.personal).then(function() {}, function() {
            i.debug("Error updating country")
        })
    }, n.updateCountry = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateCountry, p);
            return
        }
        n.personal.country.countryId <= 0 || u.beginUpdateCountry(n.personal).then(function(n) {
            n.success || i.debug("Failed to update country. Message: " + n.errorMessage)
        }, function() {
            i.debug("Error updating country")
        })
    }, n.getContactSettings = function() {
        u.beginGetContactSettings().then(function(t) {
            n.contactSettings.AppChatPrivacy = t.appChatPrivacy, n.contactSettings.GameChatPrivacy = t.gameChatPrivacy, n.contactSettings.PrivateMessagePrivacy = t.privateMessagePrivacy, n.setContactSettingsMasterSetting()
        }, function() {
            i.debug("Error getting contact settings")
        })
    }, n.onContactSettingsMasterSettingChange = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.onContactSettingsMasterSettingChange, p);
            return
        }
        var t = n.contactSettings.MasterSetting;
        t === it ? (n.contactSettings.PrivateMessagePrivacy = nt, n.contactSettings.AppChatPrivacy = nt, n.contactSettings.GameChatPrivacy = rt) : t === tt && (n.contactSettings.PrivateMessagePrivacy = b, n.contactSettings.AppChatPrivacy = b, n.contactSettings.GameChatPrivacy = b), t !== k && (u.beginUpdateAppChatPrivacy(n.contactSettings).then(function() {}, function() {
            i.debug("Error updating app chat privacy")
        }), u.beginUpdateGameChatPrivacy(n.contactSettings).then(function() {}, function() {
            i.debug("Error updating game chat privacy")
        }), u.beginUpdatePrivateMessagePrivacy(n.contactSettings).then(function() {}, function() {
            i.debug("Error updating private message privacy")
        }))
    }, n.updateAppChatPrivacy = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateAppChatPrivacy, p);
            return
        }
        n.contactSettings.MasterSetting = k, u.beginUpdateAppChatPrivacy(n.contactSettings).then(function() {}, function() {
            i.debug("Error updating app chat privacy")
        })
    }, n.updateGameChatPrivacy = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateGameChatPrivacy, p);
            return
        }
        n.contactSettings.MasterSetting = k, u.beginUpdateGameChatPrivacy(n.contactSettings).then(function() {}, function() {
            i.debug("Error updating game chat privacy")
        })
    }, n.updatePrivateMessagePrivacy = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updatePrivateMessagePrivacy, p);
            return
        }
        n.contactSettings.MasterSetting = k, u.beginUpdatePrivateMessagePrivacy(n.contactSettings).then(function() {}, function() {
            i.debug("Error updating private message privacy")
        })
    }, n.getJoinSettings = function() {
        u.beginGetPrivateServerInvitePrivacy().then(function(t) {
            n.joinSettings.privateServerInvitePrivacy = t.PrivateServerInvitePrivacy
        }, function() {
            i.debug("Error getting private server invite privacy")
        }), u.beginGetFollowMePrivacy().then(function(t) {
            n.joinSettings.followMePrivacy = t.FollowMePrivacy
        }, function() {
            i.debug("Error getting follow me privacy")
        })
    }, n.updatePrivateServerInvitePrivacy = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updatePrivateServerInvitePrivacy, p);
            return
        }
        u.beginUpdatePrivateServerInvitePrivacy(n.joinSettings).then(function() {}, function() {
            i.debug("Error updating private server invite privacy")
        })
    }, n.updateFollowMePrivacy = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateFollowMePrivacy, p);
            return
        }
        u.beginUpdateFollowMePrivacy(n.joinSettings).then(function() {}, function() {
            i.debug("Error updating follow me privacy")
        })
    }, n.getTradeSettings = function() {
        u.beginGetTradePrivacy().then(function(t) {
            n.tradeSettings.tradePrivacy = t.TradePrivacy
        }, function() {
            i.debug("Error getting trade privacy")
        }), u.beginGetTradeValue().then(function(t) {
            n.tradeSettings.tradeValue = t.TradeValue
        }, function() {
            i.debug("Error getting trade value")
        })
    }, n.updateTradePrivacy = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateTradePrivacy, p);
            return
        }
        u.beginUpdateTradePrivacy(n.tradeSettings).then(function() {}, function() {
            i.debug("Error updating trade privacy")
        })
    }, n.updateTradeValue = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateTradeValue, p);
            return
        }
        u.beginUpdateTradeValue(n.tradeSettings).then(function() {}, function() {
            i.debug("Error updating trade value")
        })
    }, n.getSocialNetworks = function() {
        u.beginGetSocialNetworks().then(function(t) {
            n.socialNetworks = t
        }, function() {
            i.debug("Error getting social networks")
        })
    }, n.updateSocialNetworks = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateSocialNetworks, p);
            return
        }
        u.beginUpdateSocialNetworks(n.socialNetworks).then(function() {
            w(v.default.success.title, v.default.success.body)
        }, function() {
            w(v.default.error.title, v.default.error.body)
        })
    }, n.setupDisconnectFb = function(t) {
        n.userSocialSignOn.isDisconnectFbEnabled = t.data.IsDisconnectFbSocialSignOnEnabled, n.userSocialSignOn.isDisconnectFbEnabled && n.getUserConnectedToSocialMedia()
    }, n.convertDate = function(n) {
        var i = parseInt(n.replace(/(^.*\()|([+-].*$)/g, "")),
            t = new Date(i),
            r = t.getUTCMonth() + 1,
            u = t.getUTCDate(),
            f = t.getUTCFullYear();
        return r + "/" + u + "/" + f
    }, n.currentDate = new Date, n.calculateAge = function(n) {
        var t = Date.now() - n.getTime(),
            i = new Date(t);
        return Math.abs(i.getUTCFullYear() - 1970)
    }, n.updateAccountInfo = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateAccountInfo, p);
            return
        }
        var t = {
            ReceiveNewsletter: n.accountContent.userInfo.data.ReceiveNewsletter
        };
        t.SocialNetworksVisibilityPrivacyValue = n.socialNetworks.SocialNetworksVisibilityPrivacy, t.Facebook = n.socialNetworks.FacebookUrl, t.Twitter = n.socialNetworks.TwitterUrl, t.GooglePlus = n.socialNetworks.GooglePlusUrl, t.YouTube = n.socialNetworks.YouTubeUrl, t.Twitch = n.socialNetworks.TwitchUrl, u.beginUpdateAccountInfo(t).then(function(t) {
            n.refreshPage();
            var i;
            i = t.success ? w(v.default.success.title, v.default.success.body) : w(v.default.error.title, t.error ? t.error : et)
        }, function() {
            i.debug("there was an error updating info")
        })
    }, n.populateYears = function() {
        for (var i = parseInt(r("date")(new Date, "yyyy")), t = 0; t < 100; t++) n.populateAccount.years[t] = i - t
    }, n.populateDays = function() {
        var r = n.personal.birthdate.birthYear,
            u = n.personal.birthdate.birthMonth - 1,
            i = new Date(r, u + 1, 0),
            t;
        for (n.personal.birthdate.birthDay > i.getDate() && (n.personal.birthdate.birthDay = i.getDate()), n.populateAccount.days = [], t = 0; t < i.getDate(); t++) n.populateAccount.days[t] = t + 1
    }, n.signOutFromSessions = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.signOutFromSessions, p);
            return
        }
        u.beginAccountSignOut().then(function() {
            w(v.default.success.title, v.secureSignOut.success.body)
        }, function() {
            w(v.default.error.title, v.secureSignOut.error.body)
        })
    }, n.disconnectFromFb = function() {
        var t, i;
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.disconnectFromFb, p);
            return
        }
        t = function() {
            u.beginUpdateSocialDisconnect(n.userSocialSignOn.providerName).then(function() {
                n.refreshPage()
            }, function(t) {
                console.info(t), w(v.default.error.title, n.processErrorMessage(t))
            })
        }, n.accountContent.userInfo.data.IsSetPasswordNotificationEnabled ? (i = c.open({
            animation: !1,
            templateUrl: y.templates.modal.passwordSet,
            controller: y.controllers.modal.passwordSet,
            resolve: {
                modalData: {
                    facebookDisconnect: !0,
                    changePassword: !n.accountContent.userInfo.data.IsSetPasswordNotificationEnabled
                }
            }
        }), i.result.finally(function() {
            u.beginGetAccountInfo().then(function(i) {
                n.setAccountInfo(i), n.accountContent.userInfo.data.IsSetPasswordNotificationEnabled || t()
            }, function(t) {
                n.accountContent.userInfo = {
                    data: t,
                    hasError: !0
                }
            })
        })) : t()
    }, n.disconnectFromXbox = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.disconnectFromXbox, p);
            return
        }
        u.beginDisconnectXbox().then(function(t) {
            t.success ? n.refreshPage() : w(v.default.success.title, v.disconnectXbox.error.body)
        })
    }, n.chooseGender = function(t) {
        n.personal.gender = t, n.updateGender()
    }, n.privacyModeInfo = function() {
        t.open(n.helpUrl, "_blank")
    }, n.buyRobux = function(n) {
        t.location.href = n
    }, n.refreshPage = function() {
        n.getAccountInfo()
    }, n.setPassword = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.setPassword, p);
            return
        }
        var t = c.open({
            animation: !1,
            templateUrl: y.templates.modal.passwordSet,
            controller: y.controllers.modal.passwordSet,
            resolve: {
                modalData: {
                    facebookDisconnect: !1,
                    changePassword: !n.accountContent.userInfo.data.IsSetPasswordNotificationEnabled
                }
            }
        });
        t.result.finally(function() {
            n.refreshPage()
        })
    }, n.changeUsername = function() {
        var i, t;
        if (n.accountContent.userInfo.data.IsEmailOnFile)
            if (n.accountContent.userInfo.data.IsEmailVerified)
                if (n.accountContent.userInfo.data.HasCurrencyOperationError) i = n.accountContent.userInfo.data.CurrencyOperationErrorMessage, w("Currency Service Error", i ? i : "There was an error with the currency service. Try again later.");
                else if (n.accountContent.userInfo.data.RobuxRemainingForUsernameChange > 0) t = l.open({
            titleText: "Insufficient Funds",
            bodyText: '<p>You need <span class="icon-robux"></span>' + n.accountContent.userInfo.data.RobuxRemainingForUsernameChange + " more to change your username. Would you like to buy more ROBUX? </p>",
            actionButtonText: "Buy",
            actionButtonShow: !0,
            actionButtonClass: "btn-primary-md",
            neutralButtonShow: !1
        }), t.result.then(function() {
            n.buyRobux(n.accountData.stateProperties.buyRobuxUrl)
        });
        else {
            if (n.pinIsEnabledAndLocked()) {
                n.unlockAccountPinSetting(n.changeUsername, p);
                return
            }
            t = c.open({
                animation: !1,
                templateUrl: y.templates.modal.usernameUpdate,
                controller: y.controllers.modal.usernameUpdate,
                resolve: {
                    modalData: {
                        userEmail: n.accountContent.userInfo.data.UserEmail,
                        accountPinLength: n.accountPinContent.pinLength
                    }
                }
            }), t.result.finally(function() {
                n.refreshPage()
            })
        } else n.verifyEmailAddress(!0);
        else n.setEmailAddress("change your username")
    }, n.verifyEmailAddress = function(n) {
        c.open({
            animation: !1,
            templateUrl: y.templates.modal.emailVerify,
            controller: y.controllers.modal.emailVerify,
            resolve: {
                modalData: {
                    verifiedEmailRequired: n
                }
            }
        })
    }, n.setEmailAddress = function(t) {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.setEmailAddress, p);
            return
        }
        var i = c.open({
            animation: !1,
            templateUrl: y.templates.modal.emailSet,
            controller: y.controllers.modal.emailSet,
            resolve: {
                modalData: {
                    emailRequired: !n.accountContent.userInfo.data.IsEmailOnFile && t,
                    action: t,
                    changeEmail: n.accountContent.userInfo.data.IsEmailOnFile,
                    over13: n.accountContent.userInfo.data.UserAbove13
                }
            }
        });
        i.result.finally(function() {
            n.refreshPage()
        })
    }, n.addPhone = function() {
        n.phoneModalData = {
            action: v.phoneSet.addActionLabel,
            isAttemptToChangePhone: !1
        }, n.addOrChangePhone()
    }, n.changePhone = function() {
        n.phoneModalData = {
            action: v.phoneSet.editActionLabel,
            isAttemptToChangePhone: !0,
            countryCode: n.accountInfoSettings.countryCode,
            prefix: n.accountInfoSettings.prefix,
            maskedNumber: n.accountInfoSettings.phone
        }, n.addOrChangePhone()
    }, n.toggleBlockedUsers = function() {
        n.showBlockedUsers = !n.showBlockedUsers
    }, n.unBlock = function(t) {
        u.beginAccountUnblockUser(t).then(function(i) {
            var r, u;
            if (i.success === !0)
                for (r = n.accountContent.userInfo.data.BlockedUsersModel, r.Total = r.Total - 1, u = 0; u < r.BlockedUsers.length; u++) r.BlockedUsers[u].uid === t && (r.BlockedUsers[u].hidden = !0)
        }, function() {
            i.debug("There was an error.")
        })
    }, n.getNotificationSettings = function() {
        n.receiverDestinationTypeMapping.length < 1 || angular.equals(n.notificationSourceTypeMapping, {}) || u.beginGetNotificationSettings().then(function(t) {
            n.notificationSettings = t, n.notificationSettings.updatedDestinationTypeOptOuts = {}
        }, function(n) {
            i.debug(n)
        })
    }, n.destinationNotificationBands = function(t) {
        return n.notificationSettings.notificationBandSettings.filter(function(n) {
            return n.receiverDestinationType === t
        })
    }, n.isNotificationBandBlacklisted = function(t, i) {
        if (i === "MobilePush") {
            var r = n.accountContent.userInfo.data.BlacklistedNotificationSourceTypesForMobilePush;
            return r.indexOf(t) >= 0
        }
        return !1
    }, n.setNotificationSettingsUrls = function(n) {
        u.setNotificationSettingsUrls(n)
    }, n.setXboxConnectionUrls = function(n) {
        u.setXboxConnectionUrls(n)
    }, n.setAuthDomainUrl = function(n) {
        u.setAuthDomainUrl(n)
    }, n.setApiProxyUrl = function(n) {
        u.setApiProxyUrl(n)
    }, n.isAvailable = function(n) {
        return n.availabilityCheck ? n.availabilityCheck() : !0
    }, n.isToggleable = function(n) {
        return typeof n.isToggleable != "undefined" ? n.isToggleable() : !0
    }, n.isDestinationTypeOptedOut = function(t) {
        var i, r;
        return t.customEnabledCheck ? (i = t.customEnabledCheck(function() {
            n.$apply()
        }), !i) : (r = t.destinationType, n.notificationSettings.optedOutReceiverDestinationTypes.filter(function(n) {
            return n === r
        }).length > 0)
    }, n.toggleDestinationTypeOptOut = function(t) {
        var r = t.destinationType,
            i;
        if (n.isDestinationTypeOptedOut(t)) {
            if (t.customEnablement) {
                t.customEnablement(function() {
                    n.$apply()
                });
                return
            }
            for (n.notificationSettings.updatedDestinationTypeOptOuts[r] = t, i = 0; i < n.notificationSettings.optedOutReceiverDestinationTypes.length; i++)
                if (n.notificationSettings.optedOutReceiverDestinationTypes[i] === r) {
                    n.notificationSettings.optedOutReceiverDestinationTypes.splice(i, 1);
                    return
                }
        } else {
            if (t.customDisablement) {
                t.customDisablement(function() {
                    n.$apply()
                });
                return
            }
            n.notificationSettings.updatedDestinationTypeOptOuts[r] = t, n.notificationSettings.optedOutReceiverDestinationTypes.push(r)
        }
    }, n.toggleNotificationBand = function(n) {
        n.isOverridable && (n.isSetByReceiver = !0, n.isEnabled = !n.isEnabled)
    }, n.updateNotificationSettings = function() {
        var t;
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateNotificationSettings, p);
            return
        }
        var f = n.notificationSettings.notificationBandSettings.filter(function(n) {
                return n.isSetByReceiver
            }),
            i = [],
            r = [];
        for (t in n.notificationSettings.updatedDestinationTypeOptOuts) n.isDestinationTypeOptedOut(n.notificationSettings.updatedDestinationTypeOptOuts[t]) ? r.push(t) : i.push(t);
        u.beginUpdateNotificationSettings(f, r, i).then(function() {
            n.refreshPage(), w(a.modalText.success.title, a.modalText.success.body)
        }, function() {
            w(a.modalText.error.title, a.modalText.error.body)
        })
    }, n.updateNotificationSettingsAndAccounts = function() {
        var t;
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.updateNotificationSettingsAndAccounts, p);
            return
        }
        var f = n.notificationSettings.notificationBandSettings.filter(function(n) {
                return n.isSetByReceiver
            }),
            i = [],
            r = [];
        for (t in n.notificationSettings.updatedDestinationTypeOptOuts) n.isDestinationTypeOptedOut(n.notificationSettings.updatedDestinationTypeOptOuts[t]) ? r.push(t) : i.push(t);
        u.beginUpdateNotificationSettings(f, r, i).then(function() {
            n.updateAccountInfo()
        }, function() {
            w(v.default.error.title, v.updateNotificationSettings.error.body)
        })
    }, n.setAccountPinSetting = function(t) {
        n.accountPinContent.isEnabled = t.isEnabled, n.accountPinContent.unlockedUntil = t.unlockedUntil, n.accountPinContent.isUnlocked = !1, t.isEnabled && t.unlockedUntil && (n.accountPinContent.isUnlocked = !0, n.startTimer(t.unlockedUntil))
    }, n.pinIsEnabledAndLocked = function() {
        return n.accountPinContent.isEnabled && !n.accountPinContent.isUnlocked && n.accountContent.userInfo.data.IsAccountPinEnabled
    }, n.getAccountPinSetting = function() {
        u.beginGetAccountPinSetting().then(function(t) {
            t && n.setAccountPinSetting(t)
        }, function(n) {
            i.debug(n)
        })
    }, n.lockAccountPinSetting = function() {
        u.beginLockAccountPinSetting().then(function(t) {
            t && (n.accountPinContent.isUnlocked = !1, n.accountPinContent.unlockedUntil = null, n.clearTimer())
        }, function(n) {
            i.debug(n)
        })
    }, n.unlockAccountPinSetting = function(t, i, r) {
        c.open({
            animation: !1,
            templateUrl: y.templates.modal.pinUnlock,
            controller: y.controllers.modal.pinUnlock,
            resolve: {
                modalData: {
                    accountPinLength: n.accountPinContent.pinLength
                }
            }
        }).result.then(function(i) {
            i.unlockedUntil != null && (n.accountPinContent.isEnabled = !0, n.accountPinContent.isUnlocked = !0, n.accountPinContent.unlockedUntil = i.unlockedUntil, n.startTimer(Math.floor(i.unlockedUntil)), t && t.apply())
        }, function() {
            r && r.apply()
        })
    }, n.startTimer = function(t) {
        n.clearTimer(), n.accountPinContent.timeRemaining = t, n.accountPinContent.timeRemainingMin = "--", n.accountPinContent.timeRemainingSec = "--", ft = e(function() {
            n.accountPinContent.timeRemaining = n.accountPinContent.timeRemaining - 1, n.accountPinContent.timeRemainingMin = Math.floor(n.accountPinContent.timeRemaining / 60), n.accountPinContent.timeRemainingSec = Math.floor(n.accountPinContent.timeRemaining - n.accountPinContent.timeRemainingMin * 60), n.accountPinContent.timeRemaining <= 0 && (n.accountPinContent.isUnlocked = !1, n.accountPinContent.unlockedUntil = null, n.clearTimer())
        }, 1e3)
    }, n.clearTimer = function() {
        e.cancel(ft)
    }, n.toggleAccountPinLockSetting = function() {
        n.accountPinContent.isUnlocked ? n.lockAccountPinSetting() : n.unlockAccountPinSetting()
    }, n.toggleAccountPinEnabledSetting = function() {
        if (n.accountContent.userInfo.data.IsEmailOnFile)
            if (n.accountContent.hasVerifiedEmail) {
                if (n.pinIsEnabledAndLocked()) {
                    n.unlockAccountPinSetting(n.toggleAccountPinEnabledSetting, p);
                    return
                }
                n.accountPinContent.isEnabled ? u.beginDeleteAccountPinSetting().then(function(t) {
                    t && (n.accountPinContent.isEnabled = !1, n.accountPinContent.isUnlocked = !0, n.accountPinContent.unlockedUntil = null)
                }, function(t) {
                    w("Error", n.processErrorMessage(t)), i.debug(t)
                }) : c.open({
                    animation: !1,
                    templateUrl: y.templates.modal.pinCreate,
                    controller: y.controllers.modal.pinCreate,
                    resolve: {
                        modalData: {
                            userEmail: n.accountContent.userInfo.data.UserEmail,
                            accountPinLength: n.accountPinContent.pinLength
                        }
                    }
                }).result.then(function() {
                    n.getAccountPinSetting()
                }, function() {}).finally(function() {
                    n.getAccountPinSetting()
                })
            } else n.verifyEmailAddress(!0);
        else n.setEmailAddress("add a Account PIN")
    }, n.getAccountRestrictions = function() {
        n.parentContent.accountRestrictions.isFeatureEnabled = n.accountContent.userInfo.data.IsAccountRestrictionsFeatureEnabled, u.beginGetAccountRestrictions().then(function(t) {
            n.accountRestrictions = t
        }, function() {
            i.debug("Error getting account restrictions settings")
        })
    }, n.toggleAccountRestrictionsSetting = function() {
        if (n.pinIsEnabledAndLocked()) {
            n.unlockAccountPinSetting(n.toggleAccountRestrictionsSetting, p);
            return
        }
        n.parentContent.accountRestrictions.isFeatureEnabled && (n.accountRestrictions.IsEnabled || (n.contactSettings.MasterSetting = tt, n.onContactSettingsMasterSettingChange()), u.beginUpdateAccountRestrictions(!n.accountRestrictions.IsEnabled).then(function() {
            n.accountRestrictions.IsEnabled = !n.accountRestrictions.IsEnabled
        }, function(n) {
            w(v.default.error.title, u.beginProcessErrorMessage(n))
        }))
    }, n.update2svSetting = function() {
        if (!n.accountContent.userInfo.data.IsAdmin) {
            if (n.pinIsEnabledAndLocked()) {
                n.unlockAccountPinSetting(n.update2svSetting, p);
                return
            }
            if (n.twoStepContent.isEnabled) {
                var t = l.open({
                    titleText: v.twoStepVerification.disabling.title,
                    titleIcon: "icon-warning",
                    bodyText: v.twoStepVerification.disabling.body,
                    actionButtonShow: !0,
                    actionButtonText: v.default.positiveBtnText,
                    actionButtonClass: "btn-control-md",
                    neutralButtonText: v.default.negativeBtnText,
                    neutralButtonClass: "btn-secondary-md"
                });
                t.result.then(function() {
                    u.beginUpdate2svSetting(!1).then(function() {
                        n.get2SvSetting()
                    }, function(t) {
                        w(v.default.error.title, n.processErrorMessage(t))
                    })
                })
            } else n.accountContent.userInfo.data.IsEmailOnFile ? n.accountContent.hasVerifiedEmail ? u.beginUpdate2svSetting(!0).then(function() {
                n.get2SvSetting(), l.open({
                    titleText: v.twoStepVerification.success.title,
                    titleIcon: v.twoStepVerification.success.titleIcon,
                    bodyText: v.twoStepVerification.success.body
                })
            }, function(t) {
                displyInfoModal(v.default.error.title, n.processErrorMessage(t))
            }) : n.verifyEmailAddress(!0) : n.setEmailAddress("enable 2 Step Verification")
        }
    }, n.doToggle2SvSetting = function(t) {
        u.beginUpdate2svSetting(t).then(function() {
            n.get2SvSetting()
        })
    }, n.get2SvSetting = function() {
        u.beginGet2svSeting().then(function(t) {
            t && (n.twoStepContent.isEnabled = t.IsTwoStepEnabled)
        }, function(n) {
            w(v.default.error.title, u.beginProcessErrorMessage(n))
        })
    }, n.getUserConnectedToSocialMedia = function() {
        u.beginGetSocialConnected().then(function(t) {
            t && (t.providers && t.providers[0] ? (n.gigyaUserInfo(), n.userSocialSignOn.isSocialSignOnBoxShown = !0, n.userSocialSignOn.providerName = t.providers[0].provider) : (n.userSocialSignOn.isSocialSignOnBoxShown = !1, n.userSocialSignOn.providerName = ""))
        }, function(n) {
            w(v.default.error.title, u.beginProcessErrorMessage(n))
        })
    }, n.getXboxConnection = function() {
        n.xbox.isDisconnectXboxEnabled = n.accountContent.userInfo.data.IsDisconnectXboxEnabled, n.xbox.isDisconnectXboxEnabled && u.beginGetXboxConnection().then(function(t) {
            n.xbox.isDisconnectXboxBoxShown = t && t.hasConnectedXboxAccount ? !0 : !1
        })
    }, n.setContactSettingsMasterSetting = function() {
        var t = k;
        n.contactSettings.PrivateMessagePrivacy === nt && n.contactSettings.AppChatPrivacy === nt && n.contactSettings.GameChatPrivacy === rt ? t = it : n.contactSettings.PrivateMessagePrivacy === b && n.contactSettings.AppChatPrivacy === b && n.contactSettings.GameChatPrivacy === b && (t = tt), n.contactSettings.MasterSetting = t
    }, n.getCountryList = function() {
        n.isNewAccountCountrySettingEnabled === !0 && u.beginGetCountryList().then(function(t) {
            if (t.success) {
                n.allActiveCountryList = t.countryList;
                var r = {
                    countryId: 0,
                    countryName: "Choose a Country/Region"
                };
                n.allActiveCountryList.unshift(r)
            } else i.debug("Failed to get list of countries.")
        }, function(n) {
            i.debug(n)
        })
    }, n.populateYears(), n.getCountryList(), n.getAccountInfo(), t && t.location && t.location.search && t.location.search.indexOf("changepassword=true") > -1 && n.changePassword()
}]);