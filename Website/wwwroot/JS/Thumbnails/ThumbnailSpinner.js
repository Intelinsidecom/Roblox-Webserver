Roblox=Roblox|| {}

,
Roblox.ThumbnailSpinner=function() {
    var t='<div class="thumbnail-loader"><div class="loading-animated"><div><div></div><div></div><div></div></div></div></div>',
    n=".thumbnail-loader";

    return {
        show:function(i) {
            if(!(i.find(n).length>0)) {
                var r=$(t);
                i.append(r)
            }
        }

        ,
        hide:function(t) {
            t.find(n).remove()
        }
    }
}

();