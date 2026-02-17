// PrivateServers/LinkJoin.js
var Roblox = Roblox || {};
Roblox.PrivateServerLinkJoin = function() {
    var n, t = function(t) {
            n = t
        },
        i = function() {
            typeof n == "function" && n()
        };
    return {
        init: t,
        execute: i
    }
}();