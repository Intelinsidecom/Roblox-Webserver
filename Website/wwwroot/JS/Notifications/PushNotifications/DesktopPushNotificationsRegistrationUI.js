typeof Roblox=="undefined" &&(Roblox= {}),
Roblox.PushNotificationRegistrationUI=function() {

    var h="Roblox.PushNotificationRegistrationUI.PromptActive",
    u="Roblox.PushNotificationRegistrationUI.PromptMessage",
    ft= !1,
    ut= !1,
    rt= !1,
    it=[86400,
    604800],
    tt,
    l,
    t=null,
    n,
    e=null,
    f=null,
    yt=function() {
        var n=$("#push-notification-registration-ui-settings");
        ft=n.data("noncontextualpromptallowed"),
        ut=n.data("promptonfriendrequestsentenabled"),
        rt=n.data("promptonprivatemessagesentenabled"),
        it=n.data("promptintervals"),
        tt=n.data("notificationsdomain"),
        l=n.data("userid")
    }

    ,
    s=function(n) {
        return n+"_"+l
    }

    ,
    nt=function(n) {
        return localStorage.getItem(s(n))
    }

    ,
    o=function(n, t) {
        localStorage.setItem(s(n), t)
    }

    ,
    g=function(n) {
        localStorage.removeItem(s(n))
    }

    ,
    i=function() {
        e&&(e.hide(), f.removeClass("modal-open"), $(".modal-backdrop").hide(), e=null, f=null),
        $("#push-notifications-registration-prompt").remove()
    }

    ,
    et=function(n) {
        var t=$("#navContent");
        t.length>0?t.prepend(n): $(
            "#wrap .container-main").prepend(n)
    }

    ,
    d=function(n) {
        i(
    );
    var r=$("#" +n+"-template"),
    t=$('<div id="push-notifications-registration-prompt"></div>');
    t.append(r.html()),
    et(t)
}

,
a=function(n) {
    i();
    var r=$("#" +n+"-template"),
    t=$('<div id="push-notifications-registration-prompt"></div>');

    t.append(r.html()),
    f=$("#rbx-body"),
    f.append(t),
    e=$("#" +n+"-modal").bootstrapModal({
        backdrop:"static", show: !0, keyboard: !1
    })
}

,
k=function(n) {
    d(n);
    var t=$("#push-notifications-registration-prompt .alert-success");
    Roblox.BootstrapWidgets.ToggleSystemMessage(t, 100, 3e3)
}

,
r=function(n, i) {
    Roblox.PushNotificationRegistrar&&Roblox.PushNotificationRegistrar.isPushSupported()&&(i|| !t.IsTooSoon())&&($(".push-notifications-global-prompt .alert-info").length===0&&($(".alert-info").length>0||$(".alert-cookie-notice").length>0)||$.when(wt(), pt()).done(function(t, r) {
                if(t||r)c( !0); else {
                    d("push-notifications-initial-global-prompt"), n&&$("#push-notifications-registration-prompt .push-notifications-prompt-text").text(n); var u=$("#push-notifications-registration-prompt .push-notifications-prompt-actions"); u.find(".push-notifications-prompt-accept").click(ht), u.find(".push-notifications-dismiss-prompt").click(ct), i||Roblox.PushNotificationRegistrar.getEventPublisher().Publish(Roblox.PushNotificationEventPublishers.RegistrationEventTypes.promptShown), bt(n)
                }

            }).fail(function() {}))
}

,
wt=function() {
    var n=$.Deferred();

    return Roblox.PushNotificationRegistrar.isPushEnabled(function(t) {
            n.resolve(t)
        }),
    n
}

,
pt=function() {
    var n=$.Deferred();

    return $.ajax({
        method:"GET", url:tt+"/v2/notifications/get-settings", xhrFields: {
            withCredentials: !0
        }

        , crossDomain: !0, success:function(t) {
            t&&t.optedOutReceiverDestinationTypes&&t.optedOutReceiverDestinationTypes.indexOf("DesktopPush") !==-1?n.resolve( !0):n.resolve( !1)
        }
    }),
n
}

,
p=function() {
    navigator.permissions.query({
        name:"notifications"

    }).then(function(n) {
        n.onchange=function() {
            b()
        }
    })
}

,
v=function() {
    k("push-notifications-successfully-enabled"),
    c( !0),
    n&&typeof n=="function" &&n( !0)
}

,
y=function(t) {
    t===Roblox.PushNotificationRegistrar.subscriptionFailureReasons.permissionDenied&&(p(), a("push-notifications-permissions-disabled-instruction")),
    n&&typeof n=="function" &&n( !1)
}

,
w=function(t) {
    n=t,
    b()
}

,
at=function(n) {

    i(),
    lt(),
    Roblox.PushNotificationRegistrar.unsubscribe(function() {
            k("push-notifications-successfully-disabled"), n&&typeof n=="function" &&n()
        })
}

,
b=function() {
    i(),
    Notification.permission==="granted" ?Roblox.PushNotificationRegistrar.subscribe(v, y): Notification.permission==="denied" ?(p(), a("push-notifications-permissions-disabled-instruction")):(a("push-notifications-permissions-prompt"), Roblox.PushNotificationRegistrar.subscribe(v, y))
}

,
bt=function(n) {
    o(h, !0),
    n?o(u, n): g(u)
}

,
c=function(n) {
    o(h, !1),
    g(u),
    n=== !0?t.Reset(): t.RecordAction()
}

,
lt=function() {
    t.Reset(),
    t.RecordAction()
}

,
ct=function() {
    Roblox.PushNotificationRegistrar.getEventPublisher().Publish(Roblox.PushNotificationEventPublishers.RegistrationEventTypes.promptDeclined),
    i(),
    c( !1)
}

,
ht=function() {
    Roblox.PushNotificationRegistrar.getEventPublisher().Publish(Roblox.PushNotificationEventPublishers.RegistrationEventTypes.promptAccepted),
    w()
}

,
st=function() {
    i()
}

,
ot=function() {
    try {
        ut&&r("Do you want to know when your friend request is accepted?")
    }

    catch(n) {}
}

,
vt=function() {
    try {
        rt&&r("Do you want to know when you receive messages?")
    }

    catch(n) {}
}

,
kt=function() {
    var n,
    i;
    yt(),
    t=new Roblox.PersistedBackoffIntervalTracker(it, "Roblox.PushNotificationRegistrationUI.Prompt." +l),
    n=nt(h),
    n&&n==="true" ?(i=nt(u), r(i, !0)): ft&&t.HasBeenTooLong(2)&&r();
    $(document).on("Roblox.Friendship.FriendRequestSent", ot);
    $(document).on("Roblox.Messages.MessageSent", vt)
}

;

return {
    initialize: kt, prompt:r, enable:w, disable:at, closeMessage:st
}
}

(),
$(Roblox.PushNotificationRegistrationUI.initialize);