"use strict";

notificationStream.directive("clickInCard", ["eventStreamService", "hybridService", "notificationStreamUtility", "$log", function(n, t, i, r) {
        return {
            restrict:"A", scope: !0, link:function(u, f) {
                f.bind("click", function(f) {
                        var e, o, h, l, s, c; if( !f.target)return !1; if(e=angular.element(f.target), o=e.attr("type"), f.target&&o&&(h=angular.copy(u.library.eventStreamMetaData), u.notification&&(h.notificationType=u.notification.notificationSourceType, u.interactNotification(u.notification)), l=n.eventNames.notificationStream[o], n.sendEventWithTarget(l, f.type, h), u.library.inApp)) {
                            f.stopPropagation(), f.preventDefault(); switch(o) {
                                case i.links.settingLinkName:s= {
                                    feature:i.links.settingTabName, urlPath:i.links.settingLink
                                }

                                , t.navigateToFeature(s, function(n) {
                                        r.debug("navigateToFeature ---- status:" +n)

                                    }); break; case i.links.friendRequestLinkName:s= {
                                    feature:i.links.friendRequestTabName, urlPath:i.links.friendRequestLink
                                }

                                , t.navigateToFeature(s, function(n) {
                                        r.debug("openUserProfile ---- status:" +n)

                                    }); break; case i.links.profileLinkName:c=e.attr("href")&&e.attr("href").match(/users\/(\d+)/, "")?e.attr("href").match(/users\/(\d+)/, "")[1]:u.userIds[0], t.openUserProfile(parseInt(c), function(n) {
                                        r.debug("openUserProfile ---- status:" +n)
                                    })
                            }

                            return !1
                        }
                    })
            }
        }
    }

    ]);