// utilities/CrossTabCommunication/Kingmaker.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.CrossTabCommunication == "undefined" && (Roblox.CrossTabCommunication = {}), Roblox.CrossTabCommunication.Kingmaker = function() {
    function i(n) {
        for (var i = it.slice(0), t = 0; t < i.length; t++) try {
            i[t](n)
        } catch (r) {}
    }

    function ct() {
        var i = localStorage.getItem(n.masterId),
            t;
        ht(), t = localStorage.getItem(n.masterLastResponseTime), f = t && t.length !== 0 ? parseInt(t) : 0, i ? i === e ? r = !0 : f > 0 && Date.now() - f > d ? p() : tt() : p(), ot(), l()
    }

    function lt() {
        var n = +new Date;
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(t) {
            var i = (n + Math.random() * 16) % 16 | 0;
            return n = Math.floor(n / 16), (t === "x" ? i : i & 3 | 8).toString(16)
        })
    }

    function b() {
        var n = Date.now();
        return n.toString()
    }

    function pt() {
        o = "", t = null, r = !1, y = null, f = Date.now() - 1e4, w = 2500, d = 2e4, a = Math.floor(Math.random() * 100 + 1), v = 500, g = 2e3 + a, ft = 1500 + a, ut = 400 + a
    }

    function c() {
        i("Master is:" + t)
    }

    function s(n) {
        var r, t;
        for (i("Announcing: Is this tab the master? " + n), r = rt.slice(0), t = 0; t < r.length; t++) try {
            r[t](n)
        } catch (u) {
            i("Error running subscribed election result handler: " + JSON.stringify(u))
        }
    }

    function h() {
        i("Declaring myself as the master" + t), t = e, r = !0, Roblox.CrossTabCommunication.PubSub.Publish(n.masterId, t), localStorage.removeItem(n.electionInProgress), s(!0), $(window).unbind("unload." + u).bind("unload." + u, function() {
            var t = localStorage.getItem(n.masterId);
            t && t === e && st()
        })
    }

    function st() {}

    function k() {
        var t = localStorage.getItem(n.electionInProgress),
            i = parseInt(t);
        t && Date.now() - i > v && localStorage.removeItem(n.electionInProgress), window.setTimeout(k, v)
    }

    function ot() {
        window.setTimeout(function() {
            k()
        }, v)
    }

    function p() {
        var r = localStorage.getItem(n.electionInProgress);
        t = "", r ? (i("Election already in progress"), window.setTimeout(function() {
            t.length === 0 ? h() : t !== e && s(!1), c()
        }, ut)) : (i("Election not in progress"), localStorage.setItem(n.electionInProgress, b()), t.length === 0 ? h() : t !== e && s(!1), c())
    }

    function ht() {
        i("Binding to events"), Roblox.CrossTabCommunication.PubSub.Subscribe(n.masterIdRequest, u, function(t) {
            r === !0 && t === et && (i("Query Received - Confirming Still Master"), Roblox.CrossTabCommunication.PubSub.Publish(n.masterIdResponse, e), localStorage.setItem(n.masterLastResponseTime, b()))
        }), Roblox.CrossTabCommunication.PubSub.Subscribe(n.masterId, u, function(u) {
            if (u) {
                i("Received Notice Of Master"), f = Date.now(), t = u;
                var o = r;
                r = t === e, r === !1 && o && (s(!1), l()), r !== !0 || o || h(), localStorage.removeItem(n.electionInProgress), c()
            }
        }), Roblox.CrossTabCommunication.PubSub.Subscribe(n.masterIdResponse, u, function(n) {
            n ? (i("Master Responded to Query"), f = Date.now(), o = n, l()) : i("Master Responded to Query - no message")
        })
    }

    function tt() {
        (i("Checking if Master still active"), r === !0 || Date.now() - f <= w) || (o = "", Roblox.CrossTabCommunication.PubSub.Publish(n.masterIdRequest, et), window.setTimeout(function() {
            if (o.length === 0) {
                if (r === !0 || Date.now() - f <= w) {
                    h();
                    return
                }
                i("Master did not respond. Initiating election"), p()
            } else t !== o && (s(!1), t = o, c())
        }, ft))
    }

    function l() {
        y && clearTimeout(y), y = window.setTimeout(function() {
            r === !1 ? tt() : localStorage.setItem(n.masterLastResponseTime, b()), l()
        }, g)
    }
    var u = "Roblox.CrossTabCommunication.Kingmaker",
        n = {
            masterId: u + ".masterId",
            electionInProgress: u + ".electionInProgress",
            masterIdRequest: u + ".masterIdRequest",
            masterIdResponse: u + ".masterIdResponse",
            masterLastResponseTime: u + ".masterLastResponseTime"
        },
        et = "q",
        o, t, r, y, f, w, d, a, v, g, ft, ut, e, rt = [],
        it = [],
        wt = function(n) {
            it.push(n)
        },
        nt = function() {
            return Roblox.CrossTabCommunication.PubSub.IsAvailable()
        },
        yt = function() {
            e = lt(), pt(), $(function() {
                nt() && ct()
            })
        },
        vt = function() {
            return r
        },
        at = function(n) {
            rt.push(n)
        };
    return yt(), {
        IsAvailable: nt,
        IsMasterTab: vt,
        SubscribeToMasterChange: at,
        AttachLogger: wt
    }
}();