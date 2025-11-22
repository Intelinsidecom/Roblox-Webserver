// common/robloxError.js
"use strict";
var RobloxError = function() {
    var n = function(n, t) {
        var i = t && t.name;
        switch (i) {
            case "TypeError":
                this.error = new TypeError(n);
                break;
            case "EvalError":
                this.error = new EvalError(n);
                break;
            case "RangeError":
                this.error = new RangeError(n);
                break;
            case "ReferenceError":
                this.error = new ReferenceError(n);
                break;
            default:
                this.error = new Error(n)
        }
    };
    return n.prototype.throw = function(n) {
        if (Roblox && Roblox.jsConsoleEnabled) throw this.error;
        n && typeof n == "function" && n()
    }, n
}();