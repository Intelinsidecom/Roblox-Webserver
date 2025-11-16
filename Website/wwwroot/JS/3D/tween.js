Date.now===undefined&&(Date.now=function() {
        return(new Date).valueOf()
    });

var TWEEN=TWEEN||function() {
    var n=[];

    return {

        REVISION:"13",
        getAll:function() {
            return n
        }

        ,
        removeAll:function() {
            n=[]
        }

        ,
        add:function(t) {
            n.push(t)
        }

        ,
        remove:function(t) {
            var i=n.indexOf(t);
            i !==-1&&n.splice(i, 1)
        }

        ,
        update:function(t) {
            if(n.length===0)return !1;
            var i=0;
            for(t=t !==undefined?t:typeof window !="undefined" &&window.performance !==undefined&&window.performance.now !==undefined?window.performance.now():Date.now(); i<n.length; )n[i].update(t)?i++: n.splice(i, 1);
            return !0
        }
    }
}

();

TWEEN.Tween=function(n) {

    var i=n,
    r= {}

    ,
    t= {}

    ,
    u= {}

    ,
    d=1e3,
    o=0,
    p= !1,
    y= !1,
    k= !1,
    a=0,
    e=null,
    g=TWEEN.Easing.Linear.None,
    b=TWEEN.Interpolation.Linear,
    f=[],
    l=null,
    c= !1,
    h=null,
    s=null,
    v=null,
    w;
    for(w in n)r[w]=parseFloat(n[w], 10);

    this.to=function(n, i) {
        return i !==undefined&&(d=i),
        t=n,
        this
    }

    ,
    this.start=function(n) {
        TWEEN.add(this),
        y= !0,
        c= !1,
        e=n !==undefined?n: typeof window !="undefined" &&window.performance !==undefined&&window.performance.now !==undefined?window.performance.now():Date.now(), e+=a;

        for(var f in t) {
            if(t[f]instanceof Array) {
                if(t[f].length===0)continue;
                t[f]=[i[f]].concat(t[f])
            }

            r[f]=i[f],
            r[f]instanceof Array== !1&&(r[f]*=1),
            u[f]=r[f]||0
        }

        return this
    }

    ,
    this.stop=function() {
        return y?(TWEEN.remove(this), y= !1, v !==null&&v.call(i), this.stopChainedTweens(), this): this
    }

    ,
    this.stopChainedTweens=function() {
        for(var n=0, t=f.length; n<t; n++)f[n].stop()
    }

    ,
    this.delay=function(n) {
        return a=n,
        this
    }

    ,
    this.repeat=function(n) {
        return o=n,
        this
    }

    ,
    this.yoyo=function(n) {
        return p=n,
        this
    }

    ,
    this.easing=function(n) {
        return g=n,
        this
    }

    ,
    this.interpolation=function(n) {
        return b=n,
        this
    }

    ,
    this.chain=function() {
        return f=arguments,
        this
    }

    ,
    this.onStart=function(n) {
        return l=n,
        this
    }

    ,
    this.onUpdate=function(n) {
        return h=n,
        this
    }

    ,
    this.onComplete=function(n) {
        return s=n,
        this
    }

    ,
    this.onStop=function(n) {
        return v=n,
        this
    }

    ,
    this.update=function(n) {
        var v,
        w,
        nt,
        tt,
        y,
        rt,
        it,
        ut;
        if(n<e)return !0;
        c=== !1&&(l !==null&&l.call(i), c= !0),
        w=(n-e)/d,
        w=w>1?1: w, nt=g(w);
        for(v in t)tt=r[v]||0,
        y=t[v],
        y instanceof Array?i[v]=b(y, nt): (typeof y=="string" &&(y=tt+parseFloat(y, 10)), typeof y=="number" &&(i[v]=tt+(y-tt)*nt));

        if(h !==null&&h.call(i, nt), w==1) {
            if(o>0) {
                isFinite(o)&&o--;
                for(v in u)typeof t[v]=="string" &&(u[v]=u[v]+parseFloat(t[v], 10)),
                p&&(rt=u[v], u[v]=t[v], t[v]=rt),
                r[v]=u[v];
                return p&&(k= !k),
                e=n+a,
                !0
            }

            for(s !==null&&s.call(i), it=0, ut=f.length; it<ut; it++)f[it].start(n);
            return !1
        }

        return !0
    }
}

,
TWEEN.Easing= {
    Linear: {
        None:function(n) {
            return n
        }
    }

    ,
    Quadratic: {
        In:function(n) {
            return n*n
        }

        ,
        Out:function(n) {
            return n*(2-n)
        }

        ,
        InOut:function(n) {
            return(n*=2)<1?.5*n*n: -.5*(--n*(n-2)-1)
        }
    }

    ,
    Cubic: {
        In:function(n) {
            return n*n*n
        }

        ,
        Out:function(n) {
            return--n*n*n+1
        }

        ,
        InOut:function(n) {
            return(n*=2)<1?.5*n*n*n: .5*((n-=2)*n*n+2)
        }
    }

    ,
    Quartic: {
        In:function(n) {
            return n*n*n*n
        }

        ,
        Out:function(n) {
            return 1- --n*n*n*n
        }

        ,
        InOut:function(n) {
            return(n*=2)<1?.5*n*n*n*n: -.5*((n-=2)*n*n*n-2)
        }
    }

    ,
    Quintic: {
        In:function(n) {
            return n*n*n*n*n
        }

        ,
        Out:function(n) {
            return--n*n*n*n*n+1
        }

        ,
        InOut:function(n) {
            return(n*=2)<1?.5*n*n*n*n*n: .5*((n-=2)*n*n*n*n+2)
        }
    }

    ,
    Sinusoidal: {
        In:function(n) {
            return 1-Math.cos(n*Math.PI/2)
        }

        ,
        Out:function(n) {
            return Math.sin(n*Math.PI/2)
        }

        ,
        InOut:function(n) {
            return.5*(1-Math.cos(Math.PI*n))
        }
    }

    ,
    Exponential: {
        In:function(n) {
            return n===0?0: Math.pow(1024, n-1)
        }

        ,
        Out:function(n) {
            return n===1?1: 1-Math.pow(2, -10*n)
        }

        ,
        InOut:function(n) {
            return n===0?0: n===1?1:(n*=2)<1?.5*Math.pow(1024, n-1):.5*(-Math.pow(2, -10*(n-1))+2)
        }
    }

    ,
    Circular: {
        In:function(n) {
            return 1-Math.sqrt(1-n*n)
        }

        ,
        Out:function(n) {
            return Math.sqrt(1- --n*n)
        }

        ,
        InOut:function(n) {
            return(n*=2)<1?-.5*(Math.sqrt(1-n*n)-1): .5*(Math.sqrt(1-(n-=2)*n)+1)
        }
    }

    ,
    Elastic: {
        In:function(n) {
            var i,
            t=.1,
            r=.4;
            return n===0?0: n===1?1:( !t||t<1?(t=1, i=r/4):i=r*Math.asin(1/t)/(2*Math.PI), -(t*Math.pow(2, 10*(n-=1))*Math.sin((n-i)*2*Math.PI/r)))
        }

        ,
        Out:function(n) {
            var i,
            t=.1,
            r=.4;
            return n===0?0: n===1?1:( !t||t<1?(t=1, i=r/4):i=r*Math.asin(1/t)/(2*Math.PI), t*Math.pow(2, -10*n)*Math.sin((n-i)*2*Math.PI/r)+1)
        }

        ,
        InOut:function(n) {
            var i,
            t=.1,
            r=.4;
            return n===0?0: n===1?1:( !t||t<1?(t=1, i=r/4):i=r*Math.asin(1/t)/(2*Math.PI), (n*=2)<1)?-.5*t*Math.pow(2, 10*(n-=1))*Math.sin((n-i)*2*Math.PI/r):t*Math.pow(2, -10*(n-=1))*Math.sin((n-i)*2*Math.PI/r)*.5+1
        }
    }

    ,
    Back: {
        In:function(n) {
            var t=1.70158;
            return n*n*((t+1)*n-t)
        }

        ,
        Out:function(n) {
            var t=1.70158;
            return--n*n*((t+1)*n+t)+1
        }

        ,
        InOut:function(n) {
            var t=1.70158*1.525;
            return(n*=2)<1?.5*n*n*((t+1)*n-t): .5*((n-=2)*n*((t+1)*n+t)+2)
        }
    }

    ,
    Bounce: {
        In:function(n) {
            return 1-TWEEN.Easing.Bounce.Out(1-n)
        }

        ,
        Out:function(n) {
            return n<1/2.75?7.5625*n*n: n<2/2.75?7.5625*(n-=1.5/2.75)*n+.75:n<2.5/2.75?7.5625*(n-=2.25/2.75)*n+.9375:7.5625*(n-=2.625/2.75)*n+.984375
        }

        ,
        InOut:function(n) {
            return n<.5?TWEEN.Easing.Bounce.In(n*2)*.5: TWEEN.Easing.Bounce.Out(n*2-1)*.5+.5
        }
    }
}

,
TWEEN.Interpolation= {
    Linear:function(n, t) {
        var i=n.length-1,
        r=i*t,
        u=Math.floor(r),
        f=TWEEN.Interpolation.Utils.Linear;
        return t<0?f(n[0], n[1], r): t>1?f(n[i], n[i-1], i-r):f(n[u], n[u+1>i?i:u+1], r-u)
    }

    ,
    Bezier:function(n, t) {
        for(var u=0, r=n.length-1, f=Math.pow, e=TWEEN.Interpolation.Utils.Bernstein, i=0; i<=r; i++)u+=f(1-t, r-i)*f(t, i)*n[i]*e(r, i);
        return u
    }

    ,
    CatmullRom:function(n, t) {
        var i=n.length-1,
        u=i*t,
        r=Math.floor(u),
        f=TWEEN.Interpolation.Utils.CatmullRom;
        return n[0]===n[i]?(t<0&&(r=Math.floor(u=i*(1+t))), f(n[(r-1+i)%i], n[r], n[(r+1)%i], n[(r+2)%i], u-r)): t<0?n[0]-(f(n[0], n[0], n[1], n[1], -u)-n[0]):t>1?n[i]-(f(n[i], n[i], n[i-1], n[i-1], u-i)-n[i]):f(n[r?r-1:0], n[r], n[i<r+1?i:r+1], n[i<r+2?i:r+2], u-r)
    }

    ,
    Utils: {
        Linear:function(n, t, i) {
            return(t-n)*i+n
        }

        ,
        Bernstein:function(n, t) {
            var i=TWEEN.Interpolation.Utils.Factorial;
            return i(n)/i(t)/i(n-t)
        }

        ,
        Factorial:function() {
            var n=[1];

            return function(t) {
                var r=1,
                i;
                if(n[t])return n[t];
                for(i=t; i>1; i--)r*=i;
                return n[t]=r
            }
        }

        (),
        CatmullRom:function(n, t, i, r, u) {
            var f=(i-n)*.5,
            e=(r-t)*.5,
            o=u*u,
            s=u*o;
            return(2*t-2*i+f+e)*s+(-3*t+3*i-2*f-e)*o+f*u+t
        }
    }
}

;