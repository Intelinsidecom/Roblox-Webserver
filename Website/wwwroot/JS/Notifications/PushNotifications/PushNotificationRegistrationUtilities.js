typeof Roblox=="undefined" &&(Roblox= {}),
Roblox.PushNotificationRegistrationUtilities=function() {
    var n=function(n) {
        return n.endpoint||null
    }

    ,
    t=function(n) {
        return !n.endpoint||typeof n.endpoint !="string" ?null: n.endpoint.substr(n.endpoint.lastIndexOf("/")+1)
    }

    ;

    return {
        getRegistrationEndpoint: n, getRegistrationToken:t
    }
}

();