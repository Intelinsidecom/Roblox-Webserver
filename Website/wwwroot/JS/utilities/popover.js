var Roblox=Roblox|| {}

;

Roblox.Popover=function() {
    "use strict";

    function u(n, i) {
        var u=$(n),
        f=$(i),
        e=$(t),
        h=e.outerWidth(),
        c=u.find(r).outerWidth(),
        l=e.offset().left,
        o=0,
        s;
        (u.hasClass("bottom")||u.hasClass("top"))&&(s=$("body").outerWidth()-parseInt(f.width()+f.offset().left), o=$("body").outerWidth()-l-s-h/2-c/2, u.find(r).css("right", o))
    }

    function f(t) {
        return t.data("hiddenClassName")&&(n=t.data("hiddenClassName")),
        n
    }

    function o() {
        $(t).on("click touchstart", function(t) {
                var s=$(this).data("bind"), h=s?"#" +s:i, r=$(h), c=$(this).data("container"), l=c?"#" +c:e, o; n=f(r), r.hasClass("manual")||r.toggleClass(n), o= !r.hasClass(n), $(document).triggerHandler("Roblox.Popover.Status", {
                    isOpen:o, eventType:t.type
                }), o&&u(h, l)
        })
}

function s() {
    $("body").on("click touchstart", function(r) {
            $(t).each(function() {
                    var u=$(this).data("bind"), t=u?$("#" +u):$(i), o="roblox-popover-open-always", e="roblox-popover-close"; if(n=f(t), $(t).hasClass(o)&& !$(r.target).hasClass(e))return !1; !$(r.target).hasClass(e)&&($(this).is(r.target)||$(this).has(r.target).length !==0||t.has(r.target).length !==0||t.hasClass(n)||r.type !=="click")||(t.addClass(n), $(document).triggerHandler("Roblox.Popover.Status", {
                            isHidden: !0, eventType:r.type
                        }))
            })
    })
}

function h() {
    o(),
    s()
}

var t=".roblox-popover",
i=".roblox-popover-content",
e=".roblox-popover-container",
r=".arrow",
n="hidden";

return $(function() {
        h()

    }),
{
setUpTrianglePosition: u
}
}

();