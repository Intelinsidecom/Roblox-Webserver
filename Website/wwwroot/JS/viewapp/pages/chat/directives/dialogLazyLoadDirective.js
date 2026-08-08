// ~/viewapp/pages/chat/directives/dialogLazyLoadDirective.js
"use strict";
chat.directive("dialogLazyLoad", ["chatService", "chatUtility", "messageService", "$timeout", "$log", function(n, t, i, r, u) {
    return {
        restrict: "A",
        scope: !0,
        link: function(r, f) {
            var s = function() {
                    r.updateDialog()
                },
                o = function() {
                    if (!r.dialogParams.loadMoreMessages || !r.dialogLayout.IsdialogContainerVisible) return !1;
                    r.dialogLayout.isChatLoading = !0;
                    var f = r.dialogData.ChatMessages.length;
                    n.getMessages(r.dialogData.Id, r.dialogData.ChatMessages[f - 1].Id, r.dialogParams.pageSizeOfGetMessages).then(function(n) {
                        if (r.dialogLayout.isChatLoading = !1, n) {
                            if (n.length > 0) {
                                r.dialogLayout.scrollToBottom = !1;
                                for (var u = 0; u < n.length; u++) t.sanitizeMessage(n[u]), i.buildFallbackTimeStamp(n[u], r.dialogData), i.setFallbackClusterMaster(r.dialogData, n[u])
                            }
                            n.length < r.dialogParams.pageSizeOfGetMessages && (r.dialogParams.loadMoreMessages = !1)
                        } else r.dialogParams.loadMoreMessages = !1, i.manipulateMessages(r.dialogData, n, r.chatLibrary.friendsDict)
                    }, function() {
                        r.dialogLayout.isChatLoading = !1, u.debug("---error from get getMessages in dialogLazyLoadDirective.js---")
                    })
                },
                h = function() {
                    u.debug("---- onInit callback ---- Scrollbars updated"), r.dialogLayout.scrollToBottom = !0
                };
            r.chatLibrary.inApp ? (r.dialogLayout.IsdialogContainerVisible = !0, Roblox.Scrollbar.scrollToBottom(f), Roblox.Scrollbar.listenToScroll(f, o), r.$watch(function() {
                return f.height()
            }, function(n) {
                n && Roblox.Scrollbar.scrollToBottom(f)
            }, !0), r.$watch(function() {
                return r.dialogData.ChatMessages
            }, function(n, t) {
                (!t || t.length === 0 || n.length > 0 && t.length > 0 && n[0].parsedTimestamp !== t[0].parsedTimestamp) && Roblox.Scrollbar.scrollToBottom(f)
            }, !0)) : f.mCustomScrollbar({
                autoExpandScrollbar: !1,
                scrollInertia: 5,
                contentTouchScroll: 1,
                mouseWheel: {
                    preventDefault: !0
                },
                callbacks: {
                    onInit: h,
                    onUpdate: function() {
                        u.debug("---- onUpdate callback ---- Scrollbars updated"), r.dialogLayout.scrollToBottom ? (f.mCustomScrollbar("update"), f.mCustomScrollbar("scrollTo", "bottom", {
                            scrollInertia: 0
                        })) : r.dialogLayout.scrollToBottom = !0, f.hasClass("mCS_no_scrollbar") && r.updateDialog()
                    },
                    onTotalScroll: s,
                    onTotalScrollOffset: 60,
                    onTotalScrollBack: o
                }
            })
        }
    }
}]);