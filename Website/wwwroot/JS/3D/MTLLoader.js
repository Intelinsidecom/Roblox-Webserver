THREE.MTLLoader=function(n, t) {
    this.options=n,
    this.crossOrigin=t
}

,
THREE.MTLLoader.prototype= {
    getHashUrl:function(n) {
        for(var i=31, t=0; t<32; t++)i^=n[t].charCodeAt(0);
        return"https://t"+(i%8).toString()+".rbxcdn.com/"+n
    }

    ,
    constructor:THREE.MTLLoader,
    load:function(n, t, i, r) {
        var f=this,
        u=new THREE.XHRLoader;

        u.setCrossOrigin(this.crossOrigin),
        u.load(n, function(n) {
                t(f.parse(n))
            }

            , function() {}

            , r)
    }

    ,
    parse:function(n) {
        for(var h=n.split("\n"), u= {}

            , l=/\s+/, c= {}

            , t, f, i, r, o, s, e=0; e<h.length; e++)(t=h[e], t=t.trim(), t.length !==0&&t.charAt(0) !=="#")&&(f=t.indexOf(" "), i=f>=0?t.substring(0, f):t, i=i.toLowerCase(), r=f>=0?t.substring(f+1):"", r=r.trim(), i==="newmtl" ?(u= {
                    name:r
                }

                , c[r]=u):u&&(i==="ka" ||i==="kd" ||i==="ks" ?(o=r.split(l, 3), u[i]=[parseFloat(o[0]), parseFloat(o[1]), parseFloat(o[2])]):u[i]=r));
        return s=new THREE.MTLLoader.MaterialCreator(this.options),
        s.setMaterials(c),
        s
    }
}

,
THREE.MTLLoader.MaterialCreator=function(n) {

    this.options=n,
    this.materialsInfo= {}

    ,
    this.materials= {}

    ,
    this.materialsArray=[],
    this.nameLookup= {}

    ,
    this.side=this.options&&this.options.side?this.options.side:THREE.FrontSide,
    this.wrap=this.options&&this.options.wrap?this.options.wrap:THREE.RepeatWrapping
}

,
THREE.MTLLoader.MaterialCreator.prototype= {

    constructor:THREE.MTLLoader.MaterialCreator,
    setMaterials:function(n) {

        this.materialsInfo=this.convert(n),
        this.materials= {}

        ,
        this.materialsArray=[],
        this.nameLookup= {}
    }

    ,
    getHashUrl:function(n) {
        for(var i=31, t=0; t<32; t++)i^=n[t].charCodeAt(0);
        return"https://t"+(i%8).toString()+".rbxcdn.com/"+n
    }

    ,
    convert:function(n) {
        var i,
        r,
        u,
        f,
        e;
        if( !this.options)return n;

        i= {}

        ;

        for(r in n) {

            u=n[r],
            f= {}

            ,
            i[r]=f;

            for(e in u) {
                var o= !0,
                t=u[e],
                s=e.toLowerCase();

                switch(s) {
                    case"kd": case"ka":case"ks":this.options&&this.options.normalizeRGB&&(t=[t[0]/255, t[1]/255, t[2]/255]), this.options&&this.options.ignoreZeroRGBs&&t[0]===0&&t[1]===0&&t[1]===0&&(o= !1);
                    break;
                    case"d": this.options&&this.options.invertTransparency&&(t=1-t)
                }

                o&&(f[s]=t)
            }
        }

        return i
    }

    ,
    preload:function() {
        for(var n in this.materialsInfo)this.create(n)
    }

    ,
    getIndex:function(n) {
        return this.nameLookup[n]
    }

    ,
    getAsArray:function() {
        var n=0,
        t;
        for(t in this.materialsInfo)this.materialsArray[n]=this.create(t),
        this.nameLookup[t]=n,
        n++;
        return this.materialsArray
    }

    ,
    create:function(n) {
        return this.materials[n]===undefined&&this.createMaterial_(n),
        this.materials[n]
    }

    ,
    createMaterial_:function(n) {

        var u=this.materialsInfo[n],
        t= {
            name: n, side:this.side
        }

        ,
        r,
        i;

        for(r in u) {
            i=u[r];

            switch(r.toLowerCase()) {
                case"kd": t.color=(new THREE.Color).fromArray(i);
                break;
                case"ks": t.specular=(new THREE.Color).fromArray(i);
                break;
                case"map_kd": t.map=this.loadTexture(this.getHashUrl(i)), t.map.wrapS=this.wrap, t.map.wrapT=this.wrap;
                break;
                case"ns": t.shininess=i;
                break;
                case"d": i<1&&(t.transparent= !0, t.opacity=i)
            }
        }

        return t.shininess=0,
        this.materials[n]=new THREE.MeshPhongMaterial(t)
    }

    ,
    loadTexture:function(n, t, i, r) {
        var s=/\.dds$/i.test(n),
        u;
        if(s)u=THREE.ImageUtils.loadCompressedTexture(n, t, i, r);

        else {
            var h=new Image,
            u=new THREE.Texture(h, t),
            e=new THREE.ImageLoader;
            e.crossOrigin="anonymous";
            var f=0,
            c=4,
            l=5e3;

            function o() {
                e.load(n, y, v, a)
            }

            function a() {
                f<c?(f=f+1, setTimeout(o, l)): typeof r=="function" &&r("Unable to load 3D thumbnail")
            }

            function v() {}

            function y(n) {
                u.image=THREE.MTLLoader.ensurePowerOfTwo_(n),
                u.needsUpdate= !0,
                i&&i(u)
            }

            o()
        }

        return u
    }
}

,
THREE.MTLLoader.ensurePowerOfTwo_=function(n) {
    var t,
    i;
    return !THREE.MTLLoader.isPowerOfTwo_(n.width)|| !THREE.MTLLoader.isPowerOfTwo_(n.height)?(t=document.createElement("canvas"), t.width=THREE.MTLLoader.nextHighestPowerOfTwo_(n.width), t.height=THREE.MTLLoader.nextHighestPowerOfTwo_(n.height), i=t.getContext("2d"), i.drawImage(n, 0, 0, n.width, n.height, 0, 0, t.width, t.height), t): n
}

,
THREE.MTLLoader.isPowerOfTwo_=function(n) {
    return(n&n-1)==0
}

,
THREE.MTLLoader.nextHighestPowerOfTwo_=function(n) {
    --n;
    for(var t=1; t<32; t<<=1)n=n|n>>t;
    return n+1
}

;