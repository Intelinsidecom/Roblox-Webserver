// RobloxEventManager.js
function RBXBaseEventListener() {
    if (!(this instanceof RBXBaseEventListener)) return new RBXBaseEventListener;
    this.init = function() {
        for (eventKey in this.events) this.events.hasOwnProperty(eventKey) && $(document).bind(this.events[eventKey], $.proxy(this.localCopy, this))
    }, this.events = [], this.localCopy = function(n, t) {
        var i = $.extend(!0, {}, n),
            r = $.extend(!0, {}, t);
        this.handleEvent(i, r)
    }, this.distillData = function() {
        return console.log("RBXEventListener distillData - Please implement me"), !1
    }, this.handleEvent = function() {
        return console.log("EventListener handleEvent - Please implement me"), !1
    }, this.fireEvent = function() {
        return console.log("EventListener fireEvent - Please implement me"), !1
    }
}
RobloxEventManager = new function() {
    var n = [],
        t = {};
    this.enabled = !1, this.initialized = !1, this.eventQueue = [], this.initialize = function(n) {
        for (this.initialized = !0, this.enabled = n; this.eventQueue.length > 0;) {
            var t = this.eventQueue.pop();
            this.triggerEvent(t.eventName, t.args)
        }
    }, this.triggerEvent = function(n, t) {
        this.initialized ? this.enabled && (typeof t == "undefined" && (t = {}), t.guid = Roblox.Cookies.getBrowserTrackerId(), t.guid != -1 && $(document).trigger(n, [t])) : this.eventQueue.push({
            eventName: n,
            args: t
        })
    }, this.registerCookieStoreEvent = function(t) {
        n.push(t)
    }, this.insertDataStoreKeyValuePair = function(n, i) {
        t[n] = i
    }, this.monitorCookieStore = function() {
        var i, r, u, t, f;
        try {
            if (typeof Roblox == "undefined" || typeof Roblox.Client == "undefined" || window.location.protocol == "https:") return;
            if (i = Roblox.Client.CreateLauncher(!1), i == null) return;
            for (r = 0; r < n.length; r++) try {
                u = n[r], t = i.GetKeyValue(u), t != "" && t != "-1" && t != "RBX_NOT_VALID" && (f = eval("(" + t + ")"), f.userType = f.userId > 0 ? "user" : "guest", RobloxEventManager.triggerEvent(u, f), i.SetKeyValue(u, "RBX_NOT_VALID"))
            } catch (e) {}
        } catch (e) {}
    }, this.startMonitor = function() {
        function f() {
            i ? r() : e()
        }

        function r() {
            clearTimeout(t), t = setTimeout(f, RobloxEventManager._idleInterval), i = !1;
            $(document).one("mousemove", function() {
                i = !0
            })
        }

        function u() {
            clearInterval(n), n = setInterval(RobloxEventManager.monitorCookieStore, 5e3), r()
        }

        function e() {
            clearTimeout(t), clearInterval(n);
            var i = document.getElementById("robloxpluginobj");
            Roblox.Client.ReleaseLauncher(i, !1, !1);
            $(document).one("mousemove", u)
        }
        var n, t, i;
        $("#PlaceLauncherStatusPanel").data("is-protocol-handler-launch-enabled") != "True" && typeof Roblox != "undefined" && typeof Roblox.Client != "undefined" && window.location.protocol != "https:" && u()
    }
};