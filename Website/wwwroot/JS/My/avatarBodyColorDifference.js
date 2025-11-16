Roblox=Roblox|| {}

,
Roblox.AvatarBodyColorDifference=function() {
    function o(n, t) {
        var i=u(n),
        r=u(t);
        return s(i, r)
    }

    function s(t, i) {
        var et=1,
        d=1,
        nt=1,
        o=n(360),
        v=n(180),
        rt=Math.pow(25, 7),
        lt=Math.sqrt(t.A*t.A+t.B*t.B),
        ft=Math.sqrt(i.A*i.A+i.B*i.B),
        it=(lt+ft)/2,
        tt=.5*(1-Math.sqrt(Math.pow(it, 7)/(Math.pow(it, 7)+rt))),
        l=(1+tt)*t.A,
        a=(1+tt)*i.A,
        h=Math.sqrt(l*l+t.B*t.B),
        y=Math.sqrt(a*a+i.B*i.B),
        e,
        f;
        t.B===0&&l===0?e=0: (e=Math.atan2(t.B, l), e<0&&(e+=o)), i.B===0&&a===0?f=0:(f=Math.atan2(i.B, a), f<0&&(f+=o));
        var at=i.L-t.L,
        g=y-h,
        r,
        k=h*y;
        k===0?r=0: (r=f-e, r<-v?r+=o:r>v&&(r-=o)), r=2*Math.sqrt(k)*Math.sin(r/2);
        var b=(t.L+i.L)/2,
        c=(h+y)/2,
        s=e+f,
        u;
        u=h*h==0?s: Math.abs(e-f)<=v?s/2:s<o?(s+o)/2:(s-o)/2;
        var ot=1-.17*Math.cos(u-n(30))+.24*Math.cos(2*u)+.32*Math.cos(3*u+n(6))-.2*Math.cos(4*u-n(63)),
        st=n(30)*Math.exp(-Math.pow((u-n(275))/n(25), 2)),
        ht=2*Math.sqrt(Math.pow(c, 7)/(Math.pow(c, 7)+rt)),
        ct=1+.015*Math.pow(b-50, 2)/Math.sqrt(20+Math.pow(b-50, 2)),
        w=1+.045*c,
        p=1+.015*c*ot,
        ut=-Math.sin(2*st)*ht;
        return Math.sqrt(Math.pow(at/(et*ct), 2)+Math.pow(g/(d*w), 2)+Math.pow(r/(nt*p), 2)+ut*(g/(d*w))*(r/(nt*p)))
    }

    function n(n) {
        return n*(Math.PI/180)
    }

    function u(n) {
        var t=h(n);
        return c(t)
    }

    function h(n) {
        var t=i(n.R/255),
        r=i(n.G/255),
        u=i(n.B/255),
        f=t*.4124+r*.3576+u*.1805,
        e=t*.2126+r*.7152+u*.0722,
        o=t*.0193+r*.1192+u*.9505;

        return {
            X: f, Y:e, Z:o
        }
    }

    function c(n) {
        var u=r(n.X/t.X),
        i=r(n.Y/t.Y),
        f=r(n.Z/t.Z);

        return {
            L: Math.max(0, 116*i-16), A:500*(u-i), B:200*(i-f)
        }
    }

    function i(n) {
        return n>.04045?Math.pow((n+.055)/1.055, 2.4)*100: n/12.92*100
    }

    function r(n) {
        return n>f?Math.pow(n, 1/3): e*n+16/116
    }

    var f=216/24389,
    e=24389/27,
    t= {
        X: 95.047, Y:100, Z:108.883
    }

    ;

    return {
        DeltaE: o
    }
}

();