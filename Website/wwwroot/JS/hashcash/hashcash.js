// hashcash/hashcash.js
"use strict";
var Roblox = Roblox || {};
Roblox.Hashcash = function() {
    function u(n) {
        t = n
    }

    function f(n) {
        i = n
    }

    function e(u, f) {
        if (!window.Worker) {
            f && f(!1);
            return
        }
        var e = {
            hashRegex: t,
            username: u
        };
        n = new Worker(i), n.postMessage(e), n.onmessage = function(n) {
            var t = n.data;
            f && f(t), r()
        }, n.onerror = function() {
            f && f(!1), r()
        }
    }

    function r() {
        n.terminate(), n = undefined
    }

    function o() {
        return n
    }
    var n, t, i = "/js/hashcash/worker.js";
    return {
        getValueToHash: e,
        setRegex: u,
        setWorkerFile: f,
        getWorker: o
    }
}();