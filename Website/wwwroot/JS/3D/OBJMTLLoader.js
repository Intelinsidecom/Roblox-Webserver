// 3D/OBJMTLLoader.js
THREE.OBJMTLLoader = function() {}, THREE.OBJMTLLoader.prototype = {
    constructor: THREE.OBJMTLLoader,
    getHashUrl: function(n) {
        if (!n || n.length !== 32 || n.indexOf("/") >= 0) return n;
        var e = document.querySelector("[obj-loader-cdn-url]"),
            d = e && e.getAttribute("obj-loader-cdn-url");
        if (d) return d.replace(/\/+$/, "") + "/" + n;
        for (var i = 31, t = 0; t < 32; t++) i ^= n.charCodeAt(t);
        return "https://t" + (i % 8).toString() + ".cdn.freblx.com/" + n
    },
    loadWithRetries: function(n, t, i, r, u) {
        function h() {
            f < o ? (f = f + 1, setTimeout(e, s)) : u && u("Unable to load file", t)
        }

        function e() {
            n.load(t, i, r, h)
        }
        var f = 0,
            o = 4,
            s = 5e3;
        e()
    },
    interpretData: function(n, t, i, r, u) {
        function e(n) {
            return n.length === 32 && n.indexOf("/") === -1
        }

        function f(n) {
            return !e(n) && n.indexOf("/") > -1 && n.indexOf("\n") === -1
        }

        function o(n) {
            return !e(n) && !f(n)
        }
        var c = this,
            h = new THREE.FileLoader(c.manager);
        e(t) ? this.loadWithRetries(h, this.getHashUrl(t), function(t) {
            i(n, t)
        }, r, u) : f(t) ? this.loadWithRetries(h, t, function(t) {
            i(n, t)
        }, r, u) : o(t) && i(n, t)
    },
    cleanObjData: function(n) {
        return n.replace(new RegExp("-1.#IND", "g"), "0")
    },
    load: function(n, t, i, r, u) {
        var f, e, o = "anonymous",
            c = new THREE.MTLLoader({}, o);
        this.interpretData(this, t, function(t, e) {
            f = c.parse(e), t.interpretData(t, n, function(n, t) {
                e = n.cleanObjData(t), e = n.parse(e), e.traverse(function(n) {
                    if (n instanceof THREE.Mesh && n.material.name) {
                        var t = f.create(n.material.name);
                        t && (n.material = t)
                    }
                }), i(e, f)
            }, r, u)
        }, r, u)
    },
    parse: function(n, t) {
        function g(n, t, i) {
            return new THREE.Vector3(n, t, i)
        }

        function tt(n, t) {
            return new THREE.Vector2(n, t)
        }

        function d(n, t, i, r) {
            return new THREE.Face3(n, t, i, r)
        }

        function p(n, t) {
            s.length > 0 && (f.vertices = s, f.mergeVertices(), y.add(w), f = new THREE.Geometry, w = new THREE.Mesh(f, o), ht = 0), n !== undefined && (w.name = n), t !== undefined && (o = new THREE.MeshLambertMaterial, o.name = t, w.material = o)
        }

        function e(n, t, i, r) {
            r === undefined ? f.faces.push(d(parseInt(n) - (u + 1), parseInt(t) - (u + 1), parseInt(i) - (u + 1))) : f.faces.push(d(parseInt(n) - (u + 1), parseInt(t) - (u + 1), parseInt(i) - (u + 1), [l[parseInt(r[0]) - 1].clone(), l[parseInt(r[1]) - 1].clone(), l[parseInt(r[2]) - 1].clone()]))
        }

        function k(n, t, i) {
            f.faceVertexUvs[0].push([a[parseInt(n) - 1].clone(), a[parseInt(t) - 1].clone(), a[parseInt(i) - 1].clone()])
        }

        function c(n, t, i) {
            n[3] === undefined ? (e(n[0], n[1], n[2], i), !(t === undefined) && t.length > 0 && k(t[0], t[1], t[2])) : (!(i === undefined) && i.length > 0 ? (e(n[0], n[1], n[3], [i[0], i[1], i[3]]), e(n[1], n[2], n[3], [i[1], i[2], i[3]])) : (e(n[0], n[1], n[3]), e(n[1], n[2], n[3])), !(t === undefined) && t.length > 0 && (k(t[0], t[1], t[3]), k(t[1], t[2], t[3])))
        }
        for (var u = 0, b = new THREE.Object3D, y = b, f = new THREE.Geometry, o = new THREE.MeshLambertMaterial, w = new THREE.Mesh(f, o), s = [], ht = 0, l = [], a = [], ot = /v( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)/, et = /vn( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)/, rt = /vt( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)/, ft = /f( +\d+)( +\d+)( +\d+)( +\d+)?/, ut = /f( +(\d+)\/(\d+))( +(\d+)\/(\d+))( +(\d+)\/(\d+))( +(\d+)\/(\d+))?/, st = /f( +(\d+)\/(\d+)\/(\d+))( +(\d+)\/(\d+)\/(\d+))( +(\d+)\/(\d+)\/(\d+))( +(\d+)\/(\d+)\/(\d+))?/, it = /f( +(\d+)\/\/(\d+))( +(\d+)\/\/(\d+))( +(\d+)\/\/(\d+))( +(\d+)\/\/(\d+))?/, nt = n.split("\n"), r, i, v, h = 0; h < nt.length; h++)
            if (r = nt[h], r = r.trim(), r.length === 0 || r.charAt(0) === "#") continue;
            else(i = ot.exec(r)) !== null ? s.push(g(parseFloat(i[1]), parseFloat(i[2]), parseFloat(i[3]))) : (i = et.exec(r)) !== null ? l.push(g(parseFloat(i[1]), parseFloat(i[2]), parseFloat(i[3]))) : (i = rt.exec(r)) !== null ? a.push(tt(parseFloat(i[1]), parseFloat(i[2]))) : (i = ft.exec(r)) !== null ? c([i[1], i[2], i[3], i[4]]) : (i = ut.exec(r)) !== null ? c([i[2], i[5], i[8], i[11]], [i[3], i[6], i[9], i[12]]) : (i = st.exec(r)) !== null ? c([i[2], i[6], i[10], i[14]], [i[3], i[7], i[11], i[15]], [i[4], i[8], i[12], i[16]]) : (i = it.exec(r)) !== null ? c([i[2], i[5], i[8], i[11]], [], [i[3], i[6], i[9], i[12]]) : /^o /.test(r) ? (p(), u = u + s.length, s = [], y = new THREE.Object3D, y.name = r.substring(2).trim(), b.add(y)) : /^g /.test(r) ? p(r.substring(2).trim(), undefined) : /^usemtl /.test(r) ? p(undefined, r.substring(7).trim()) : /^mtllib /.test(r) ? t && (v = r.substring(7), v = v.trim(), t(v)) : /^s /.test(r);
        return p(undefined, undefined), b
    }
};