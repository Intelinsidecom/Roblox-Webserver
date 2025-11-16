typeof Roblox=="undefined" &&(Roblox= {}),
Roblox.PersistedBackoffIntervalTracker=function(n, t) {
    "use strict";

    function u(n) {
        return"Roblox.PersistedBackoffIntervalTracker."+t+"."+n
    }

    function r(n) {
        return localStorage.getItem(u(n))
    }

    function f(n, t) {
        localStorage.setItem(u(n), t)
    }

    function e(n) {
        localStorage.removeItem(u(n))
    }

    function o() {
        return+new Date
    }

    function c() {
        var n=r(i.actionCount)||0,
        t=parseInt(n, 10);
        f(i.actionCount, t+1),
        f(i.actionLastPerformed, o())
    }

    function h() {
        var n=r(i.actionCount);
        return n?parseInt(n, 10): 0
    }

    function s() {
        var t=h(),
        i;
        return t===0?0: i=t>n.length?n[n.length-1]:n[t-1]
    }

    function l() {
        var n=r(i.actionLastPerformed),
        t,
        u;
        return n?(t=o()-n, u=s(), t<u): !1
    }

    function a(n) {
        var t=r(i.actionLastPerformed),
        u,
        f;
        return t?(u=o()-t, f=s(), u>=f*n): !0
    }

    function v() {
        e(i.actionCount),
        e(i.actionLastPerformed)
    }

    var i= {
        actionLastPerformed: "actionLastPerformed", actionCount:"actionCount"
    }

    ;

    this.RecordAction=c,
    this.GetActionCount=h,
    this.GetCurrentInterval=s,
    this.IsTooSoon=l,
    this.HasBeenTooLong=a,
    this.Reset=v,
    this._Internal= {
        Keys: i, LocalStorageGet:r, LocalStorageSet:f, LocalStorageRemove:e
    }
}

;