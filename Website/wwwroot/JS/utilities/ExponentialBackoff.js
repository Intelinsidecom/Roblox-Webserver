typeof Roblox=="undefined" &&(Roblox= {}),
typeof Roblox.Utilities=="undefined" &&(Roblox.Utilities= {}),
Roblox.Utilities.ExponentialBackoff=function(n, t, i) {
    function s() {
        a();
        var n=v();
        return r++,
        n
    }

    function h() {
        r=0,
        e=+new Date,
        f= !1
    }

    function c() {
        return r
    }

    function l() {
        return e
    }

    function a() {
        r===0&&t&&t(u)&&(f= !0)
    }

    function v() {
        var t=y(),
        i,
        n;
        return r===0?(n=t.FirstAttemptDelay(), n+o(n, t.FirstAttemptRandomnessFactor())): (i=t.SubsequentDelayBase(), n=i*Math.pow(2, r-1), n>t.MaximumDelayBase()&&(n=t.MaximumDelayBase()), n+o(n, t.SubsequentDelayRandomnessFactor()))
    }

    function o(n, t) {
        return Math.floor(Math.random()*n*t)
    }

    function y() {
        return f&&i?i: n
    }

    var u=this,
    r=0,
    f= !1,
    e=null;
    u.StartNewAttempt=s,
    u.Reset=h,
    u.GetAttemptCount=c,
    u.GetLastResetTime=l
}

;