var Roblox=Roblox|| {}

;

Roblox.StringFormat=function() {
    String.prototype.format=function() {
        var n=arguments;

        return this.replace(/\{(\d+)\}/g, function(t, i) {
                return n[i]||""
            })
    }
}

();