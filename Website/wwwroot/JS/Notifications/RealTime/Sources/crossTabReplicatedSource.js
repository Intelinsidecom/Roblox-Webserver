// Notifications/RealTime/Sources/crossTabReplicatedSource.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.RealTime = Roblox.RealTime || {}, Roblox.RealTime.Sources = Roblox.RealTime.Sources || {}, Roblox.RealTime.Sources.CrossTabReplicatedSource = function(n, t) {
    var r = "Roblox.RealTime.Sources.CrossTabReplicatedSource",
        u = !1,
        f, e, o, i = function(n, i) {
            t && t("CrossTabReplicatedSource: " + n, i)
        },
        s = function() {
            return !Roblox.CrossTabCommunication || !Roblox.CrossTabCommunication.Kingmaker || !Roblox.CrossTabCommunication.PubSub ? (i("CrossTabCommunication dependencies are not present"), !1) : Roblox.CrossTabCommunication.Kingmaker.IsAvailable() ? Roblox.CrossTabCommunication.Kingmaker.IsMasterTab() ? (i("This is the master tab - it needs to send the events, not listen to them"), !1) : !0 : (i("CrossTabCommunication.Kingmaker not available - cannot pick a master tab"), !1)
        },
        h = function() {
            Roblox.CrossTabCommunication.Kingmaker.SubscribeToMasterChange(function(n) {
                n && u && f && (i("Tab has been promoted to master tab - triggering end of this source"), f())
            }), Roblox.CrossTabCommunication.PubSub.Subscribe(Roblox.RealTime.Events.Notification, r, function(n) {
                i("Notification Received: " + n, !0), n && e(JSON.parse(n))
            }), Roblox.CrossTabCommunication.PubSub.Subscribe(Roblox.RealTime.Events.ConnectionEvent, r, function(n) {
                i("Connection Event Received: " + n), n && o(JSON.parse(n))
            })
        },
        c = function() {
            Roblox.CrossTabCommunication.PubSub.Publish(Roblox.RealTime.Events.RequestForConnectionStatus, Roblox.RealTime.Events.RequestForConnectionStatus)
        },
        l = function() {
            i("Stopping. Unsubscribing from Cross-Tab events"), u = !1, Roblox.CrossTabCommunication.PubSub.Unsubscribe(Roblox.RealTime.Events.Notification, r), Roblox.CrossTabCommunication.PubSub.Unsubscribe(Roblox.RealTime.Events.ConnectionEvent, r)
        },
        a = function(n, t, i) {
            return s() ? (u = !0, f = n, e = t, o = i, h(), c(), !0) : !1
        };
    this.IsAvailable = s, this.Start = a, this.Stop = l, this.Name = "CrossTabReplicatedSource"
};