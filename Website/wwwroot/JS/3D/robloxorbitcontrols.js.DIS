THREE.OrbitControls=function(n, t, i, r) {
    function tr() {
        g=undefined
    }

    function ni() {
        g=$.now()
    }

    function di() {
        tt=kt,
        ft=0
    }

    function oi() {
        return Math.pow(.95, u.zoomSpeed)
    }

    function li(n) {
        if(n.preventDefault(), n.button===0) {
            if(u.noRotate=== !0)return;
            tr(),
            s=1,
            u.cancelAllTweens(),
            e=f.ROTATE,
            nt.set(n.clientX, n.clientY)
        }

        else if(n.button===1) {
            if(u.noZoom=== !0)return;
            e=f.DOLLY,
            a.set(n.clientX, n.clientY)
        }

        else if(n.button===2) {
            if(u.noPan=== !0)return;
            e=f.PAN,
            w.set(n.clientX, n.clientY)
        }

        u.domElement.addEventListener("mousemove", fi, !1),
        u.domElement.addEventListener("mouseup", dt, !1),
        u.dispatchEvent(wt)
    }

    function fi(n) {
        if(u.enabled !== !1) {
            this.autoRotate=== !0&&it=== !0&&(it= !1),
            n.preventDefault();
            var t=u.domElement===document?u.domElement.body: u.domElement;

            if(e===f.ROTATE) {
                if(u.noRotate=== !0)return;
                k.set(n.clientX, n.clientY),
                b.subVectors(k, nt),
                u.rotateLeft(2*Math.PI*b.x/t.clientWidth*u.rotateSpeed),
                u.rotateUp(2*Math.PI*b.y/t.clientHeight*u.rotateSpeed),
                nt.copy(k)
            }

            else if(e===f.DOLLY) {
                if(u.noZoom=== !0)return;
                l.set(n.clientX, n.clientY),
                et.subVectors(l, a),
                et.y>0?u.dollyIn(): u.dollyOut(), a.copy(l)
            }

            else if(e===f.PAN) {
                if(u.noPan=== !0)return;
                p.set(n.clientX, n.clientY),
                y.subVectors(p, w),
                u.pan(y.x, y.y),
                w.copy(p)
            }

            u.update()
        }
    }

    function ai(n) {
        e==f.ROTATE&&(dt(), g=undefined, u.tweenToLastPosition()),
        n.preventDefault(),
        n.stopPropagation()
    }

    function dt() {
        u.enabled !== !1&&(ni(), u.domElement.removeEventListener("mousemove", fi, !1), u.domElement.removeEventListener("mouseup", dt, !1), u.dispatchEvent(yt), e=f.NONE)
    }

    function ti(n) {
        if(u.enabled !== !1&&u.noZoom !== !0) {
            e !=f.ROTATE&&(ni(), u.cancelAllTweens()),
            n.preventDefault(),
            n.stopPropagation();
            var t=0;
            n.wheelDelta !==undefined?t=n.wheelDelta: n.detail !==undefined&&(t=-n.detail), t>0?u.dollyOut():u.dollyIn(), u.update(), u.dispatchEvent(wt), u.dispatchEvent(yt)
        }
    }

    function hi(n) {
        if(u.enabled !== !1&&u.noKeys !== !0&&u.noPan !== !0)switch(n.keyCode) {
            case u.keys.UP: u.pan(0, u.keyPanSpeed), u.update();
            break;
            case u.keys.BOTTOM: u.pan(0, -u.keyPanSpeed), u.update();
            break;
            case u.keys.LEFT: u.pan(u.keyPanSpeed, 0), u.update();
            break;
            case u.keys.RIGHT: u.pan(-u.keyPanSpeed, 0), u.update();
            break;
            case u.keys.ZOOMIN: u.dollyOut(), u.update();
            break;
            case u.keys.ZOOMOUT: u.dollyIn(), u.update()
        }
    }

    function ci(n) {
        if(u.enabled !== !1) {
            switch(n.touches.length) {
                case 1: if(u.noRotate=== !0)return;
                e=f.TOUCH_ROTATE,
                nt.set(n.touches[0].pageX, n.touches[0].pageY);
                break;
                case 2: if(u.noZoom=== !0)return;
                e=f.TOUCH_DOLLY;
                var t=n.touches[0].pageX-n.touches[1].pageX,
                i=n.touches[0].pageY-n.touches[1].pageY,
                r=Math.sqrt(t*t+i*i);
                a.set(0, r);
                break;
                case 3: if(u.noPan=== !0)return;
                e=f.TOUCH_PAN,
                w.set(n.touches[0].pageX, n.touches[0].pageY);
                break;
                default: e=f.NONE
            }

            u.dispatchEvent(wt)
        }
    }

    function bi(n) {
        var t;

        if(u.enabled !== !1) {
            n.preventDefault(),
            n.stopPropagation(),
            t=u.domElement===document?u.domElement.body: u.domElement;

            switch(n.touches.length) {
                case 1: if(u.noRotate=== !0)return;
                if(e !==f.TOUCH_ROTATE)return;
                k.set(n.touches[0].pageX, n.touches[0].pageY),
                b.subVectors(k, nt),
                u.rotateLeft(2*Math.PI*b.x/t.clientWidth*u.rotateSpeed),
                u.rotateUp(2*Math.PI*b.y/t.clientHeight*u.rotateSpeed),
                nt.copy(k),
                u.update();
                break;
                case 2: if(u.noZoom=== !0)return;
                if(e !==f.TOUCH_DOLLY)return;
                var i=n.touches[0].pageX-n.touches[1].pageX,
                r=n.touches[0].pageY-n.touches[1].pageY,
                o=Math.sqrt(i*i+r*r);
                l.set(0, o),
                et.subVectors(l, a),
                et.y>0?u.dollyOut(): u.dollyIn(), a.copy(l), u.update();
                break;
                case 3: if(u.noPan=== !0)return;
                if(e !==f.TOUCH_PAN)return;
                p.set(n.touches[0].pageX, n.touches[0].pageY),
                y.subVectors(p, w),
                u.pan(y.x, y.y),
                w.copy(p),
                u.update();
                break;
                default: e=f.NONE
            }
        }
    }

    function rr() {
        u.enabled !== !1&&(u.dispatchEvent(yt), e=f.NONE)
    }

    var d,
    c,
    st,
    ri,
    ot,
    si,
    g;
    r=r||"static",
    this.object=n,
    this.domElement=t !==undefined?t:document,
    this.enabled= !0,
    this.maxDistance=1e3,
    this.dynamicDampingFactor=.3,
    r==="animated" ?(this.autoRotate= !1, this.fullRotation= !1, this.noResetToInitialCameraPosition= !0):(this.autoRotate= !0, this.fullRotation= !0, this.noResetToInitialCameraPosition= !1),
    d=i.aabb.max,
    d=new THREE.Vector3(d.x, d.y, d.z),
    c=i.aabb.min,
    c=new THREE.Vector3(c.x, c.y, c.z),
    st=new THREE.Vector3,
    st.copy(d).add(c).multiplyScalar(.5),
    this.target=st,
    this.center=this.target;
    var ht=i.camera.position,
    ct=i.camera.direction,
    h=new THREE.Vector3(ht.x, ht.y, ht.z),
    vi=new THREE.Vector3(ct.x, ct.y, ct.z),
    ui=22.5*(Math.PI/180),
    vt=new THREE.Vector3;
    vt.copy(h),
    vt.sub(vi),
    this.object.position.set(h.x, h.y, h.z),
    this.object.lookAt(vt);
    var ei=new THREE.Vector3,
    pi=h.z-this.target.z,
    wi=h.x-this.target.x,
    pt=Math.atan2(wi, pi),
    rt=h.distanceTo(this.target),
    ii=.5;

    this.minDistance=rt*ii,
    ri=new THREE.Vector3(this.target.x+rt*Math.sin(pt), this.target.y+rt*Math.sin(ui), this.target.z+rt*Math.cos(pt)),
    this.lastPosition=ri,
    this.noZoom= !1,
    this.zoomSpeed=1,
    this.noRotate= !1,
    this.rotateSpeed=1,
    this.noPan= !0,
    this.keyPanSpeed=7,
    ot=Math.PI/180,
    this.minPolarAngle=0+ot,
    this.maxPolarAngle=Math.PI-ot,
    this.noKeys= !0,
    this.keys= {
        LEFT: 37, UP:38, RIGHT:39, BOTTOM:40, ZOOMIN:65, ZOOMOUT:90
    }

    ,
    si=.75*1e3;
    var kt=pt,
    nr=Math.PI/6,
    tt=0,
    ft=0,
    gi=-.01;

    var ki=1500,
    it= !1,
    u=this,
    bt=1e-6,
    nt=new THREE.Vector2,
    k=new THREE.Vector2,
    b=new THREE.Vector2,
    w=new THREE.Vector2,
    p=new THREE.Vector2,
    y=new THREE.Vector2,
    v=new THREE.Vector3,
    o=new THREE.Vector3,
    a=new THREE.Vector2,
    l=new THREE.Vector2,
    et=new THREE.Vector2,
    at=0,
    lt=0,
    s=1,
    ut=new THREE.Vector3,
    f= {
        NONE: -1, ROTATE:0, DOLLY:1, PAN:2, TOUCH_ROTATE:3, TOUCH_DOLLY:4, TOUCH_PAN:5, IDLE:6
    }

    ,
    e=this.autoRotate?f.IDLE:f.NONE;
    this.target0=this.target.clone(),
    this.position0=this.object.position.clone();

    var gt=new THREE.Quaternion,
    yi=gt.clone().inverse(),
    ir= {
        type: "change"
    }

    ,
    wt= {
        type: "start"
    }

    ,
    yt= {
        type: "end"
    }

    ;

    this.cancelAllTweens=function() {
        TWEEN.removeAll()
    }

    ,
    this.tweenToLastPosition=function() {
        u.noResetToInitialCameraPosition||(this.tweenCamera(this.lastPosition, ki, function() {
                    e=f.IDLE
                }), di())
    }

    ,
    this.tweenCamera=function(n, t, i) {
        function f() {
            it=== !1&&r.stop(),
            u.object.position.x=this.x,
            u.object.position.y=this.y,
            u.object.position.z=this.z,
            u.object.lookAt(u.target)
        }

        it= !0;
        var r;

        r=new TWEEN.Tween(u.object.position).to(n, t).easing(TWEEN.Easing.Quadratic.InOut).onUpdate(f).onComplete(function() {
                it= !1, i()
            }).start()
    }

    ,
    this.rotateLeft=function(n) {
        n===undefined&&(n=getAutoRotationAngle()),
        lt-=n
    }

    ,
    this.rotateUp=function(n) {
        n===undefined&&(n=getAutoRotationAngle()),
        at-=n
    }

    ,
    this.panLeft=function(n) {
        var t=this.object.matrix.elements;
        v.set(t[0], t[1], t[2]),
        v.multiplyScalar(-n),
        ut.add(v)
    }

    ,
    this.panUp=function(n) {
        var t=this.object.matrix.elements;
        v.set(t[4], t[5], t[6]),
        v.multiplyScalar(n),
        ut.add(v)
    }

    ,
    this.pan=function(n, t) {
        var i=u.domElement===document?u.domElement.body: u.domElement;

        if(u.object.fov !==undefined) {
            var f=u.object.position,
            e=f.clone().sub(u.target),
            r=e.length();
            r*=Math.tan(u.object.fov/2*Math.PI/180),
            u.panLeft(2*n*r/i.clientHeight),
            u.panUp(2*t*r/i.clientHeight)
        }

        else u.object.top !==undefined&&(u.panLeft(n*(u.object.right-u.object.left)/i.clientWidth), u.panUp(t*(u.object.top-u.object.bottom)/i.clientHeight))
    }

    ,
    this.dollyIn=function(n) {
        n===undefined&&(n=oi()),
        s/=n,
        s=Math.max(ii, s)
    }

    ,
    this.dollyOut=function(n) {
        n===undefined&&(n=oi()),
        s*=n
    }

    ,
    this.update=function() {
        var r,
        t,
        h,
        n,
        i;

        if(this.autoRotate=== !0&&e===f.IDLE) {
            r=Math.max(this.minDistance, Math.min(this.maxDistance, rt*s)),
            t=new THREE.Vector3(this.target.x+r*Math.sin(tt), this.target.y+r*Math.sin(ui), this.target.z+r*Math.cos(tt)),
            this.object.position.set(t.x, t.y, t.z),
            this.object.lookAt(this.target),
            tt=u.fullRotation=== !0?kt+ft: kt+nr*Math.sin(ft), ft+=gi;
            return
        }

        g !==undefined&&$.now()>=g+si&&(g=undefined, u.tweenToLastPosition()),
        t=this.object.position,
        o.copy(t).sub(this.target),
        o.applyQuaternion(gt),
        h=Math.atan2(o.x, o.z),
        n=Math.atan2(Math.sqrt(o.x*o.x+o.z*o.z), o.y),
        h+=lt,
        n+=at,
        n=Math.max(this.minPolarAngle, Math.min(this.maxPolarAngle, n)),
        n=Math.max(bt, Math.min(Math.PI-bt, n)),
        i=o.length()*s,
        i=Math.max(this.minDistance, Math.min(this.maxDistance, i)),
        this.target.add(ut),
        o.x=i*Math.sin(n)*Math.sin(h),
        o.y=i*Math.cos(n),
        o.z=i*Math.sin(n)*Math.cos(h),
        o.applyQuaternion(yi),
        t.copy(this.target).add(o),
        this.object.lookAt(this.target),
        lt=0,
        at=0,
        s=1,
        ut.set(0, 0, 0),
        ei.distanceToSquared(this.object.position)>bt&&(this.dispatchEvent(ir), ei.copy(this.object.position))
    }

    ,
    this.reset=function() {
        e=f.NONE,
        this.target.copy(this.target0),
        this.object.position.copy(this.position0),
        this.update()
    }

    ,
    Roblox.FixedUI.isMobile== !1&&(this.domElement.addEventListener("mouseleave", ai, !1), this.domElement.addEventListener("mousedown", li, !1), this.domElement.addEventListener("mousewheel", ti, !1), this.domElement.addEventListener("DOMMouseScroll", ti, !1), this.domElement.addEventListener("touchstart", ci, !1), this.domElement.addEventListener("touchend", rr, !1), this.domElement.addEventListener("touchmove", bi, !1), window.addEventListener("keydown", hi, !1), this.update())
}

,
THREE.OrbitControls.prototype=Object.create(THREE.EventDispatcher.prototype);