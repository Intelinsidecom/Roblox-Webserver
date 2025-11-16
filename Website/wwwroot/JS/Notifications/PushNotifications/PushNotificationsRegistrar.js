typeof Roblox=="undefined" &&(Roblox= {}),
Roblox.PushNotificationRegistrar=function() {

    var s="Roblox.PushNotificationRegistrar.LastRegistrationSendTimestamp",
    i= {
        permissionDenied: "permissionDenied", other:"other"
    }

    ,
    n= {
        notificationsHost: null, registrationPath:null, registrationResendInterval:null, registrationShouldSendDeliveryEndpoint:null, platformType:null
    }

    ,
    u,
    f= !1,
    r= !1,
    t=null,
    p=new Promise(function(n) {
            t=n

        }),
    a=function() {
        var t=$("#push-notification-registrar-settings");
        n.notificationsHost=t.data("notificationshost"),
        n.registrationPath=t.data("registrationpath"),
        n.registrationResendInterval=t.data("reregistrationinterval")||288e5,
        n.registrationShouldSendDeliveryEndpoint=t.data("shoulddeliveryendpointbesentduringregistration")|| !1,
        n.platformType=t.data("platformtype")||""
    }

    ,
    e=function(t, i, r, u) {

        var e=n.notificationsHost+"/v2/push-notifications/"+n.registrationPath,
        o=Roblox.PushNotificationRegistrationUtilities.getRegistrationToken(t),
        f= {
            notificationToken: o, initiatedByUser:i
        }

        ;

        n.registrationShouldSendDeliveryEndpoint&&(f.notificationEndpoint=Roblox.PushNotificationRegistrationUtilities.getRegistrationEndpoint(t)),
        $.ajax({
            method:"POST", url:e, data:JSON.stringify(f), xhrFields: {
                withCredentials: !0
            }

            , crossDomain: !0, contentType:Roblox.Constants.http.contentType, success:function() {
                w(), r&&typeof r=="function" &&r()
            }

            , error:function(n) {
                u&&typeof u=="function" &&u(n)
            }
        })
}

,
o=function() {
    var n=Roblox.Cookies.getSessionId();
    return s+"_"+n
}

,
v=function() {
    var t=localStorage.getItem(o());
    if( !t)return !1;
    var i=+new Date(t),
    r=+new Date,
    u=r-i;
    return u<=n.registrationResendInterval
}

,
w=function() {
    localStorage.setItem(o(), new Date)
}

,
l=function() {
    return !Roblox.ServiceWorkerRegistrar|| !Roblox.ServiceWorkerRegistrar.serviceWorkersSupported()? !1: ServiceWorkerRegistration&&"showNotification" in ServiceWorkerRegistration.prototype?"PushManager" in window?n.notificationsHost&&n.registrationPath? !0: !1: !1: !1
}

,
c=function() {
    if(a(), u=new Roblox.PushNotificationEventPublishers.Registration(n.platformType), !l()) {
        t();
        return
    }

    if(f= !0, Notification.permission==="denied") {
        r= !0,
        t();
        return
    }

    if( !(navigator.serviceWorker&&navigator.serviceWorker.controller)) {
        t();
        return
    }

    navigator.serviceWorker.ready.then(function(n) {
            return n.pushManager.getSubscription()

        }).then(function(n) {
            if( !n) {
                t(); return
            }

            if(v()) {
                t(); return
            }

            e(n, !1, function() {
                    t()
                }

                , function() {
                    t()
                })

        }).catch(function() {
            t()
        })
}

,
h=function(n, t) {

    Roblox.ServiceWorkerRegistrar.register(),
    navigator.serviceWorker.ready.then(function(r) {
            r.pushManager.subscribe({
                userVisibleOnly: !0

            }).then(function(t) {
                e(t, !0, n)

            }).catch(function() {
                Notification.permission==="denied" ?t&&typeof t=="function" &&t(i.permissionDenied):t&&typeof t=="function" &&t(i.other)
            })
    })
}

,
y=function(t, i) {
    $.ajax({
        method:"POST", url:n.notificationsHost+"/v2/push-notifications/deregister-current-device", xhrFields: {
            withCredentials: !0
        }

        , crossDomain: !0, success:function(n) {
            n&&n.statusMessage&&n.statusMessage===Roblox.Constants.http.successStatus&&t&&typeof t=="function" &&t( !0)
        }

        , error:function(n) {
            i&&typeof i=="function" &&i(n)
        }
    })
}

,
b=function(t) {
    if(r) {
        t( !1);
        return
    }

    navigator.serviceWorker.ready.then(function(n) {
            return n.pushManager.getSubscription()

        }).then(function(i) {
            if( !i) {
                t( !1); return
            }

            p.then(function() {
                    $.ajax({
                        method:"GET", url:n.notificationsHost+"/v2/push-notifications/get-current-device-destination", xhrFields: {
                            withCredentials: !0
                        }

                        , crossDomain: !0, success:function(n) {
                            n&&n.destination?t( !0):t( !1)
                        }
                    })
            })
    })
}

;

return {

    initialize:c,
    subscribe:h,
    unsubscribe:y,
    isPushSupported:function() {
        return f
    }

    ,
    isPushEnabled:b,
    isPushBlockedByUser:function() {
        return r
    }

    ,
    subscriptionFailureReasons:i,
    getEventPublisher:function() {
        return u
    }
}
}

(),
$(Roblox.PushNotificationRegistrar.initialize);