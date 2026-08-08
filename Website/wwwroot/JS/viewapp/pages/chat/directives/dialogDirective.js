// ~/viewapp/pages/chat/directives/dialogDirective.js
"use strict";
chat.directive("dialog", ["$window", "$compile", "$templateCache", "$interval", "$timeout", "chatService", "chatUtility", "partyService", "cookieService", "ngAudio", "localStorageService", "messageService", "$log", function(n, t, i, r, u, f, e, o, s, h, c, l, a) {
    return {
        restrict: "A",
        scope: {
            dialogData: "=",
            chatLibrary: "=",
            closeDialog: "&",
            sendInvite: "&"
        },
        link: function(r, f) {
            var vt = angular.element(document.querySelector("#chat-main")),
                v = angular.copy(e.dialogLayout.limitMemberDisplay),
                k = null,
                p = 0,
                ct = function() {
                    var n;
                    e.updateDialogStyle(r.dialogData, r.dialogLayout, r.chatLibrary), r.chatLibrary.inApp ? (n = r.dialogLayout.inAppStyle, n && n.inputStyle && e.setResizeInputLayout(r.chatLibrary, n.inputStyle.height, r.dialogData, r.dialogLayout)) : (n = r.dialogLayout.defaultStyle, n && n.inputStyle && e.setResizeInputLayout(r.chatLibrary, n.inputStyle.height, r.dialogData, r.dialogLayout))
                },
                w = function() {
                    r.dialogData.party ? (r.dialogData.isPartyExisted = !0, r.dialogData.partyInGame = r.dialogData.party.GamePlaceId ? !0 : !1) : (r.dialogData.isPartyExisted = !1, r.dialogData.partyInGame = !1)
                },
                d = function() {
                    r.isOverLoaded(), r.dialogData.currentUserId = r.currentUserId;
                    switch (r.dialogData.dialogType) {
                        case e.dialogType.CHAT:
                            var n;
                            angular.forEach(r.dialogData.userIds, function(t) {
                                t !== r.chatLibrary.userId && (n = t)
                            }), r.dialogLayout.title = r.chatLibrary.friendsDict[n] ? r.chatLibrary.friendsDict[n].Username : r.dialogData.Username, r.dialogLayout.templateUrl = Roblox.ChatTemplates.DialogTemplate, r.dialogLayout.scrollbarElm = e.getScrollBarSelector(r.dialogData, e.scrollBarType.MESSAGE), r.dialogData.name = r.chatLibrary.friendsDict[n] ? r.chatLibrary.friendsDict[n].Username : r.dialogData.Username, r.dialogData.nameLink = r.chatLibrary.friendsDict[n].UserProfileLink, w();
                            break;
                        case e.dialogType.PENDINGPARTY:
                            r.dialogLayout.templateUrl = r.dialogData.IsGroupChat ? Roblox.ChatTemplates.GroupDialogTemplate : Roblox.ChatTemplates.DialogTemplate, r.dialogLayout.limitMemberDisplay = v / 2, r.dialogLayout.scrollbarElm = e.getScrollBarSelector(r.dialogData, e.scrollBarType.MESSAGE), r.dialogData.name = r.dialogData.groupName, w();
                            break;
                        case e.dialogType.GROUPCHAT:
                            r.dialogLayout.templateUrl = Roblox.ChatTemplates.GroupDialogTemplate, r.dialogLayout.limitMemberDisplay = r.dialogData.party ? v / 2 : v, r.dialogLayout.scrollbarElm = e.getScrollBarSelector(r.dialogData, e.scrollBarType.MESSAGE), r.dialogData.name = r.dialogData.groupName, w();
                            break;
                        case e.dialogType.PARTY:
                            r.dialogData.partyName = r.dialogData.partyName ? r.dialogData.partyName : e.party.partyName, r.dialogLayout.templateUrl = r.dialogData.IsGroupChat ? Roblox.ChatTemplates.GroupDialogTemplate : Roblox.ChatTemplates.DialogTemplate, r.dialogLayout.limitMemberDisplay = r.dialogData.party.LeaderUser.Id === r.chatLibrary.userId ? v / 2 : v, r.dialogLayout.scrollbarElm = e.getScrollBarSelector(r.dialogData, e.scrollBarType.MESSAGE), r.dialogData.party.GamePlaceId && !r.dialogData.placeThumbnail && o.getPlace(r.dialogData.party.GamePlaceId).then(function(n) {
                                r.dialogData.placeThumbnail = n
                            }), r.dialogData.name = r.dialogData.partyName, w();
                            break;
                        case e.dialogType.NEWGROUPCHAT:
                            r.dialogLayout.title = r.dialogData.title, r.dialogLayout.templateUrl = Roblox.ChatTemplates.NewGroupTemplate, r.dialogData.name = r.dialogData.title;
                            break;
                        case e.dialogType.NEWPARTY:
                            r.dialogLayout.title = r.dialogData.title, r.dialogLayout.templateUrl = Roblox.ChatTemplates.NewGroupTemplate
                    }
                    ct()
                },
                ht = function() {
                    (r.dialogLayout.IsdialogContainerVisible || f.find(".dialog-container")) && (r.dialogLayout.IsdialogContainerVisible = !1, f.empty());
                    var n = angular.element(i.get(r.dialogLayout.templateUrl)),
                        u = t(n);
                    f.append(n), u(r)
                },
                st = function() {
                    var n = "";
                    return angular.isDefined(window.getSelection) ? n = window.getSelection().toString() : angular.isDefined(document.selection) && document.selection.type === "Text" && (n = document.selection.createRange().text), n.length > 0
                },
                ft = function(t) {
                    var e = r.dialogData.layoutId,
                        l = "#" + e,
                        i = r.chatLibrary.chatLayout,
                        u = angular.element(document.querySelector(l)).find(".dialog-container"),
                        a = i.widthOfChat,
                        o = i.widthOfDialog + i.spaceOfDialog,
                        v = t.indexOf(e),
                        f, s, h, c;
                    r.chatLibrary.inApp ? u.addClass("dialog-visible") : (f = +a + v * o + i.spaceOfDialog, s = n.innerWidth, s < f + o ? (h = +i.defaultChatZIndex + 1, u.css("z-index", h)) : u.css("right", f), u.addClass("dialog-visible")), c = function() {
                        r.$digest(r.toggleDialogFocusStatus(!0))
                    };
                    u.on("mouseup", c)
                },
                et = function() {
                    r.chatLibrary.inApp || (s.updateCookie(e.cookies.dialogIdList, r.chatLibrary.dialogIdList, r.chatLibrary.cookieOption), s.updateCookie(e.cookies.dialogDict, r.chatLibrary.dialogDict, r.chatLibrary.cookieOption))
                },
                lt = function() {
                    r.chatLibrary.inApp || (r.chatLibrary.dialogsLayout[r.dialogData.layoutId] = r.dialogLayout, s.updateCookie(e.cookies.dialogsLayout, r.chatLibrary.dialogsLayout, r.chatLibrary.cookieOption))
                },
                nt = function(n) {
                    var t = angular.isDefined(r.dialogData.userIds) ? r.dialogData.userIds.indexOf(n) : -1,
                        i = angular.isDefined(r.dialogData.selectedUserIds) ? r.dialogData.selectedUserIds.indexOf(n) : -1;
                    return t < 0 && i < 0
                },
                y = function() {
                    r.dialogLibrary = c.getLocalStorage(r.chatLibrary.dialogLocalStorageName) ? c.getLocalStorage(r.chatLibrary.dialogLocalStorageName) : {}
                },
                ut = function() {
                    return r.dialogLibrary && r.dialogLibrary[r.dialogData.layoutId] && r.dialogLibrary[r.dialogData.layoutId].active
                },
                rt = function() {
                    return r.dialogLibrary && r.dialogLibrary[r.dialogData.layoutId] && !r.dialogLibrary[r.dialogData.layoutId].active
                },
                g = function() {
                    return r.dialogLibrary && r.dialogLibrary[r.dialogData.layoutId] && r.dialogLibrary[r.dialogData.layoutId].played
                },
                it = Math.floor(Math.random() * 100 + 1),
                tt = 1500 + it,
                b, ot = function() {
                    b = u(function() {
                        y(), r.dialogLibrary && !r.dialogLibrary[r.dialogData.layoutId].played ? (r.chatLibrary.audio.play(), r.dialogLibrary[r.dialogData.layoutId].played = !0, c.setLocalStorage(r.chatLibrary.dialogLocalStorageName, r.dialogLibrary)) : u.cancel(b)
                    }, tt)
                },
                at = function() {
                    r.chatLibrary.inApp && (r.dialogLayout.focusMeEnabled = !1)
                };
            r.dialogData.friendIds = angular.copy(r.chatLibrary.friendIds), r.dialogMessages = [], r.dialogType = angular.copy(e.dialogType), r.deviceType = angular.copy(e.deviceType), r.memberStatus = angular.copy(e.memberStatus), r.dialogLayout = angular.isDefined(r.chatLibrary.dialogsLayout[r.dialogData.layoutId]) ? r.chatLibrary.dialogsLayout[r.dialogData.layoutId] : angular.copy(e.dialogLayout), r.chatLibrary.inApp ? r.dialogLayout.inAppStyle = {} : r.dialogLayout.defaultStyle = {}, r.updateFriends = function(n) {
                var t = [],
                    i, u;
                n ? (u = e.sortFriendList(r.chatLibrary, n), u.forEach(function(n) {
                    nt(n.Id) && t.push(n.Id), r.chatLibrary.friendsDict[n.Id] || (r.chatLibrary.friendsDict[n.Id] = n)
                }), r.dialogData.friendIds = t) : (i = angular.copy(r.chatLibrary.friendIds), angular.forEach(i, function(n) {
                    nt(n) && t.push(n)
                }), r.dialogData.friendIds = t)
            }, r.isOverLoaded = function() {
                angular.isUndefined(r.dialogData.selectedUserIds) && (r.dialogData.selectedUserIds = [], r.dialogData.selectedUsersDict = {}), r.dialogData.dialogType !== e.dialogType.FRIEND && (r.dialogData.numberOfSelected = r.dialogData.dialogType === e.dialogType.NEWGROUPCHAT ? r.dialogData.selectedUserIds.length : r.dialogData.userIds.length + r.dialogData.selectedUserIds.length - 1, r.dialogLayout.isMembersOverloaded = r.dialogData.numberOfSelected >= r.chatLibrary.quotaOfPartyMembers ? !0 : !1)
            }, r.dialogData.selectedUserIds = [], r.dialogData.selectedUsersDict = {}, r.selectFriends = function(n) {
                var t = r.dialogData.selectedUserIds.indexOf(n);
                t < 0 && !r.dialogLayout.isMembersOverloaded ? (r.dialogData.selectedUserIds.push(n), r.dialogData.selectedUsersDict[n] = r.chatLibrary.friendsDict[n]) : t > -1 && (r.dialogData.selectedUserIds.splice(t, 1), delete r.dialogData.selectedUsersDict[n]), r.dialogData.searchTerm = "", r.isOverLoaded()
            }, r.toggleDialogContainer = function() {
                r.chatLibrary.deviceType !== e.deviceType.PHONE && (r.dialogLayout.collapsed = !r.dialogLayout.collapsed), r.chatLibrary.deviceType === e.deviceType.COMPUTER && lt()
            }, r.toggleDialogFocusStatus = function(n) {
                r.chatLibrary.inApp || (r.dialogLayout.hasFocus = n, n && r.dialogLayout.active && r.markInactive()), n && l.markMessagesAsRead(r.dialogData);
                var t = n && !st() && !r.chatLibrary.inApp;
                r.dialogLayout.focusMeEnabled = t
            }, r.getTitle = function(n) {
                var u, t, i, f;
                r.dialogData.dialogType === e.dialogType.PENDINGPARTY && n === e.activeType.PARTYINVITE ? (t = r.dialogData.party ? r.dialogData.party.CreatorUser.Name : r.dialogData.InitiatorUser.Username, u = e.chatLayout.defaultTitleForPartyInvite + t) : (i = r.dialogData.ChatMessages, i && i.length > 0 ? (f = i[0].SenderUserId, t = r.chatLibrary.friendsDict[f].Username) : t = r.dialogData.InitiatorUser.Username, u = t + e.chatLayout.defaultTitleForMessage), r.title = u
            }, r.changeTitle = function() {
                n.document.title = p % 2 == 0 ? r.title : r.chatLibrary.currentTabTitle, p++
            }, r.markInactive = function() {
                r.dialogLayout.active && (r.dialogLayout.active = !1, clearInterval(k), p = 0, n.document.title = r.chatLibrary.currentTabTitle, y(), ut() && (a.debug(" --------------- markInactive -------------- set into local storage"), angular.isUndefined(r.dialogLibrary[r.dialogData.layoutId]) && (r.dialogLibrary[r.dialogData.layoutId] = {}), r.dialogLibrary[r.dialogData.layoutId].active = !1, r.dialogLibrary[r.dialogData.layoutId].played = !1, c.setLocalStorage(r.chatLibrary.dialogLocalStorageName, r.dialogLibrary)))
            }, r.markActive = function(n) {
                y(), angular.isUndefined(r.dialogLibrary[r.dialogData.layoutId]) && (r.dialogLibrary[r.dialogData.layoutId] = {}), r.dialogLibrary[r.dialogData.layoutId].active = !0, r.dialogLibrary[r.dialogData.layoutId].played = !1, c.setLocalStorage(r.chatLibrary.dialogLocalStorageName, r.dialogLibrary), r.dialogLayout.hasFocus ? l.markMessagesAsRead(r.dialogData) : (clearInterval(k), p = 0, y(), g() || r.chatLibrary.inApp || ot(), r.getTitle(n), k = setInterval(r.changeTitle, r.chatLibrary.intervalOfChangeTitleForPartyChrome)), r.chatLibrary.inApp || (r.dialogLayout.active = !0), r.dialogLayout.focusMeEnabled && (r.dialogLayout.focusMeEnabled = !1)
            }, r.handleLocalStorage = function(n) {
                n.key === r.chatLibrary.dialogLocalStorageName && (y(), r.dialogLayout.active && rt() && r.markInactive(), g() && u.cancel(b))
            }, at(), r.isOverLoaded(), r.$watch(function() {
                return r.chatLibrary.dialogDict
            }, function(n, t) {
                if (angular.isDefined(n) && angular.isDefined(n[r.dialogData.layoutId])) {
                    var u = r.dialogData.layoutId,
                        o = r.chatLibrary,
                        s = o.dialogIdList.indexOf(u),
                        i = n[u],
                        h = t[u];
                    if (!h || i.isUpdated) {
                        angular.isDefined(r.chatLibrary.dialogsLayout[r.dialogData.layoutId]) ? r.dialogLayout = r.chatLibrary.dialogsLayout[r.dialogData.layoutId] : angular.isUndefined(r.dialogLayout) && (r.dialogLayout = angular.copy(e.dialogLayout)), i.isUpdated = !1;
                        switch (i.updateStatus) {
                            case e.dialogStatus.INIT:
                            case e.dialogStatus.REPLACE:
                                s > -1 && (r.dialogLayout.focusMeEnabled === i.autoOpen && (r.chatLibrary.inApp || (r.dialogLayout.focusMeEnabled = !i.autoOpen), i.autoOpen || l.markMessagesAsRead(r.dialogData)), d(), ht(), Roblox.BootstrapWidgets.SetupTooltip(), Roblox.BootstrapWidgets.SetupPopover());
                                break;
                            case e.dialogStatus.MINIMIZE:
                                d(), r.chatLibrary.minimizedDialogIdList.indexOf(u) < 0 && (r.chatLibrary.minimizedDialogIdList.push(u), r.chatLibrary.minimizedDialogData[u] = r.dialogData), f.empty();
                                break;
                            case e.dialogStatus.REFRESH:
                                d(), i.updateStatus = e.dialogStatus.INIT
                        }
                    }
                    r.chatLibrary.deviceType !== e.deviceType.PHONE && et(), s > -1 && (ft(o.dialogIdList), i.markAsActive && (r.markActive(i.activeType), i.markAsActive = !1))
                }
            }, !0), r.$on("Roblox.Chat.MarkDialogInactive", function(n, t) {
                t.layoutId === r.dialogData.layoutId && r.markInactive()
            }), c.listenLocalStorage(r.handleLocalStorage)
        }
    }
}]);