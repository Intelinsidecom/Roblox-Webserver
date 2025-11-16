typeof Roblox=="undefined" &&(Roblox= {}),
typeof Roblox.PushNotificationEventPublishers=="undefined" &&(Roblox.PushNotificationEventPublishers= {}),
Roblox.PushNotificationEventPublishers.RegistrationEventTypes= {
    promptShown: "promptShown", promptAccepted:"promptAccepted", propmtDismissed:"promptDismissed", settingsPageEnabled:"settingsPageEnabled", settingsPageDisabled:"settingsPageDisabled"
}

,
Roblox.PushNotificationEventPublishers.Registration=function(n) {
    "using strict";

    this.Publish=function(t, i) {
        try {
            var r= {
                platformType: n
            }

            ;
            i&&(r.notificationType=i),
            Roblox.EventStream.SendEvent("pushNotificationRegistration", t, r)
        }

        catch(u) {}
    }
}

;