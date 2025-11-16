var Roblox=Roblox|| {}

;

Roblox.UrlParser=function() {
    function n(n) {
        var f=decodeURIComponent(window.location.search.substring(1)),
        r=f&&f.split("&"),
        i,
        u,
        t;
        if( !r)return null;
        for(i=0; i<r.length; i++)if(u=r[i], t=u&&u.split("="), t&&t.length>1&&t[0]===n)return t[1];
        return null
    }

    return {
        getParameterValueByName: n
    }
}

();