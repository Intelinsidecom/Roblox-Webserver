"use strict";

var Roblox=Roblox|| {}

;

Roblox.CurrentUser=Roblox.CurrentUser|| {}

,
Roblox.UserService=function() {
    function t() {
        Roblox.CurrentUser.isAuthenticated= !0,
        Roblox.CurrentUser.userId=n.dataset.userid,
        Roblox.CurrentUser.name=n.dataset.name,
        Roblox.CurrentUser.isUnder13=n.dataset.isunder13==="true",
        Roblox.CurrentUser.is13orOver= !Roblox.CurrentUser.isUnder13
    }

    function i() {
        n?t(): Roblox.CurrentUser.isAuthenticated= !1
    }

    var n=document.querySelector('meta[name="user-data"]');
    i()
}

();