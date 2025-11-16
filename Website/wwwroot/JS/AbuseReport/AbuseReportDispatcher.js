var Roblox=Roblox|| {}

;

Roblox.AbuseReportDispatcher=function() {

    var r= !1,
    i=Roblox.AbuseReportPVMeta,
    n=function(n, t) {
        n=n.replace(/[\[\]]/g, "\\$&");
        var r=new RegExp("[?&]" +n+"(=([^&#]*)|&|#|$)"),
        i=r.exec(t);
        return i?i[2]?decodeURIComponent(i[2].replace(/\+/g, " ")): "":null
    }

    ,
    u=function(t) {
        var o,
        u,
        f,
        r,
        e,
        h;
        if(typeof t !="string")return !1;

        if(t=t.toLowerCase(), o=t.split("?"), u=o[0], u&&typeof u=="string" &&(f=u.split("abusereport/")[1]), f&&(r= {
                    actionName:f, id:n("id", t), redirectUrl:n("redirecturl", t)
                }

                , n("conversationid", t)&&(r.conversationId=n("conversationid", t)), n("partyguid", t)&&(r.partyGuid=n("partyguid", t)), r.id&&r.redirectUrl)) {
            var c=$.param(r),
            s="abusereport/embedded/"+f+"?"+c,
            l=Roblox.Endpoints.getAbsoluteUrl("/" +s);

            i.inApp?i.inAppEnabled?(e= {
                    urlPath:s, feature:"Abuse Report"
                }

                , console.debug("Calling navigateToFeature for Hybrid Overlay"), Roblox.wrapErrors?(h=Roblox.wrapErrors.wrap(Roblox.Hybrid.Navigation.navigateToFeature), h(e, function(n) {
                            console.debug("navigateToFeature ---- status:" +n)

                        })):Roblox.Hybrid.Navigation.navigateToFeature(e, function(n) {
                        console.debug("navigateToFeature ---- status:" +n)
                    })):window.location.href=t:window.location.href=i.phoneEnabled?l:t
        }
    }

    ,
    t=function(n) {
        n.preventDefault();
        var t=$(this).attr("href");
        t&&u(t)
    }

    ,
    f=function() {
        if( !r) {
            $(".abuse-report-modal").click(t);
            $(".messages-container").on("click", ".abuse-report-modal", t);
            $("#AjaxCommentsContainer").on("click", ".abuse-report-modal", t);
            $("#item-context-menu").on("click", ".abuse-report-modal", t);
            $(".GroupWallPane").on("click", ".abuse-report-modal", t);
            $(".group-details-container").on("click", ".abuse-report-modal", t);
            r= !0
        }
    }

    ;

    return {
        triggerUrlAction: u, initialize:f
    }
}

(),
$(document).ready(function() {
        Roblox.AbuseReportDispatcher.initialize()
    });