"use strict";

notificationStream.directive("notificationCard", ["$log", "notificationStreamService", function(n, t) {
        var i= {
            transition:"transitionend", OTransition:"oTransitionEnd", MozTransition:"transitionend", WebkitTransition:"webkitTransitionEnd"
        }

        , r=function() {
            var n, t=document.createElement("supportedEvent"); for(n in i)if(angular.isDefined(t.style[n]))return i[n]
        }

        , u=r(); return {
            restrict:"A", link:function(i, r) {
                r.bind(u, function(t) {
                        n.debug("got a css transition event", t); var r=t.target.className.search("slide-out-left"); r>=0&&i.$evalAsync(function() {
                                i.removeNotification(i.notification.id)
                            })

                    }), i.updateNotificationSetting=function(r) {
                    t.updateNotificationSettings(i.notification.notificationSourceType, r).then(function(t) {
                            n.debug("turnOffNotification -- success", t), i.notification.isTurnOff= !r
                        }

                        , function() {
                            n.debug("turnOffNotification --fail")
                        })
                }
            }
        }
    }

    ]);