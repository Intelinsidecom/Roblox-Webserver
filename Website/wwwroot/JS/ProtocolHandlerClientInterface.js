Roblox=Roblox|| {}

,
Roblox.ProtocolHandlerImplementation=function() {
    function d(n, t) {
        var i=function() {
            clearTimeout(r),
            n()
        }

        ,
        r;

        ut(i, t.launchMode),
        r=setTimeout(function() {
                $.modal.close(), o(i, t)
            }

            , 5e3)
    }

    function ut(n, t) {
        t==="edit" ?$(".protocol-handler-container").each(function() {
                $(this).find(".play-logo-image").addClass("hidden"), $(this).find(".studio-logo-image").removeClass("hidden")

            }):$(".protocol-handler-container").each(function() {
                $(this).find(".play-logo-image").removeClass("hidden"), $(this).find(".studio-logo-image").addClass("hidden")

            }),
        $("#ProtocolHandlerStartingDialog").modal({
            escClose: !0, opacity:80, overlayCss: {
                backgroundColor:"#000"
            }

            , onClose:function() {
                n(), $.modal.close()
            }

            , zIndex:1031
        })
}

function k() {

    $.modal.close(),
    Roblox.Dialog.open({
        titleText:"Error starting game", bodyContent:"An error occurred trying to launch the game.  Please try again later.", acceptText:Roblox.Resources.Dialog.OK, showDecline: !1
    })
}

function o(n, t) {
    $("#ProtocolHandlerAreYouInstalled").modal({
        escClose: !0, opacity:80, overlayCss: {
            backgroundColor:"#000"
        }

        , onClose:function() {
            n(), $("#ProtocolHandlerInstallButton").off("click"), $.modal.close()
        }

        , zIndex:1031

    }),
$("#ProtocolHandlerInstallButton").click(function() {
        $.modal.close(), Roblox.Dialog.open({
            titleText:$("#InstallationInstructions .ph-modal-header .title").text(), allowHtmlContentInBody: !0, bodyContent:$("#InstallationInstructions .modal-content-container").html(), allowHtmlContentInFooter: !0, footerText:$("#InstallationInstructions .xsmall").html(), acceptColor:Roblox.Dialog.none, declineColor:Roblox.Dialog.none, cssClass:"install-instructions-modal", xToCancel: !0, onCloseCallback:function() {
                $("#ProtocolHandlerClickAlwaysAllowed").hide()
            }

        }), setTimeout(function() {
            $(".VisitButtonContinueGLI a").removeClass("disabled").click(t, ft)
        }

        , 5e3), h(t)
})
}

function ft(n) {
    var i=$(this),
    t;
    return i.hasClass("disabled")||(e(n.data), t=$("#ProtocolHandlerClickAlwaysAllowed"), typeof t.data("hideRememberOverlay")=="undefined" &&t.show()),
    !1
}

function t(t) {

    return t.launchTime=+new Date,
    $(Roblox.GameLauncher).trigger(Roblox.GameLauncher.startClientAttemptedEvent, {
        launchMethod:"Protocol", params:t

    }),
n.showDialog(function() {}

    , t),
$.when(g(t), nt()).then(e, n.showLaunchFailureDialog).then(rt).then(n.cleanUpAndLogSuccess, n.cleanUpAndLogFailure)
}

function rt(n) {
    var t=new $.Deferred;

    return clearInterval(f),
    f=setInterval(function() {
            var i=Roblox.Endpoints.getAbsoluteUrl("/client-status"); $.ajax(i, {
                success:function(i) {
                    i !="Unknown" &&(t.resolve(n), clearInterval(f))
                }

                , cache: !1
            })
    }

    , 3e3),
t
}

function it(n) {
    $.modal.close();

    var t= {
        launchMethod: "Protocol", params:n
    }

    ;
    $(Roblox.GameLauncher).trigger(Roblox.GameLauncher.startClientSucceededEvent, t),
    u&&($(Roblox.GameLauncher).trigger(Roblox.GameLauncher.successfulInstallEvent, t), u= !1)
}

function tt() {}

function nt() {
    var n=Roblox.Endpoints.getAbsoluteUrl("/client-status/set?status=Unknown");

    return $.ajax({
        method:"POST", url:n
    })
}

function g(n) {
    var i,
    t,
    r;

    return n.protocolVersion==2?(i="/game/get-hash?", t=[], $.each(n.otherParams, function(n, i) {
                typeof i !="object" &&t.push(n+"=" +encodeURIComponent(i))

            }), $.ajax({
            method:"POST", url:i+t.join("&")

        }).then(function(t) {
            var i=new $.Deferred, r= {
                hash:t, launchTime:n.launchTime
            }

            , u=n.protocolName, f=n.protocolVersion; return jQuery.each(n, function(t) {
                    delete n[t]
                }), n.protocolName=u, n.protocolVersion=f, n.protocolParams=r, i.resolve(n), i

        })):(r=Roblox.Endpoints.getAbsoluteUrl("/game-auth/getauthticket"), $.ajax({
        method:"GET", url:r

    }).then(function(t) {
        var i=new $.Deferred; return n.gameInfo=t, i.resolve(n), i
    }))
}

function e(t) {
    var f=new $.Deferred,
    e=n.protocolUrlSeparator,
    r=t.protocolName+":",
    i=[],
    u;

    return i.push(t.protocolVersion),
    t.protocolVersion==1?(i.push("launchmode:" +t.launchMode), i.push("gameinfo:" +encodeURIComponent(t.gameInfo)), n.protocolUrlIncludesLaunchTime&&i.push("launchtime:" +t.launchTime), u=t.otherParams):u=t.protocolParams,
    $.each(u, function(n, t) {
            n==t?i.push(n):i.push(n+":" +encodeURIComponent(t))

        }),
    r+=i.join(e),
    Roblox.GameLauncher.gameLaunchLogger.logToConsole("launchProtocolUrl: " +JSON.stringify({
            url:r, params:t
        })),
n.setLocationHref(r),
f.resolve(t),
f
}

function ot(t) {
    if(n.protocolDetectionEnabled&&typeof navigator.msLaunchUri !="undefined")navigator.msLaunchUri(t, function() {}

        , function() {});

    else {
        var i=$("iframe#gamelaunch");
        i.length>0&&i.remove(),
        i=$("<iframe id='gamelaunch' class='hidden'></iframe>").attr("src", t),
        $("body").append(i)
    }
}

function i(n, t) {
    var i=" ",
    u,
    f,
    r;

    return typeof Roblox.Endpoints !=typeof undefined&&typeof Roblox.Endpoints.Urls !=typeof undefined&&(i=Roblox.Endpoints.getAbsoluteUrl("/Game/PlaceLauncher.ashx")+"?"),
    i[0] !="h" &&(u="http://" +window.location.host, f="/Game/PlaceLauncher.ashx?", i=u+f),
    i=i.replace("placelauncher", "PlaceLauncher"),
    r= {
        request: n, browserTrackerId:Roblox.Cookies.getBrowserTrackerId()
    }

    ,
    $.extend(r, t),
    i+$.param(r)
}

function b(n, t, i) {
    return et("Edit.ashx", n, t, i)
}

function et(n, t, i, r) {
    var u=" ",
    f,
    e,
    o;

    return typeof Roblox.Endpoints !=typeof undefined&&typeof Roblox.Endpoints.Urls !=typeof undefined&&(u=Roblox.Endpoints.getAbsoluteUrl("/Game/" +n)+"?"),
    u[0] !="h" &&(f="http://" +window.location.host, e="/Game/" +n+"?", u=f+e),
    o= {
        placeId: t, upload:r?t:"", universeId:i, testMode: !1
    }

    ,
    u+$.param(o)
}

function w() {
    return t({
        protocolName:n.protocolNameForStudio, launchMode:"edit", protocolVersion:1, otherParams: {
            avatar:"avatar", browsertrackerid:Roblox.Cookies.getBrowserTrackerId()
        }
    })
}

function p(i, r, u) {
    var f=b(i, r, u);

    t({
        protocolName:n.protocolNameForStudio, launchMode:"edit", protocolVersion:1, otherParams: {
            script:f, avatar:"avatar", browsertrackerid:Roblox.Cookies.getBrowserTrackerId()
        }

        , placeId:i
    })
}

function y(r) {
    var f=n.protocolNameForClient,
    e=Roblox.Cookies.getBrowserTrackerId(),
    u;

    if(n.protocolVersion==1) {

        var o="play",
        s=i("RequestGame", r),
        h= {
            placelauncherurl: s, browsertrackerid:e
        }

        ;

        u= {
            protocolName: f, launchMode:o, otherParams:h, placeId:r.placeId
        }
    }

    else r.request="RequestGame",
    u= {
        protocolName: f, otherParams:r
    }

    ;
    return u.protocolVersion=n.protocolVersion,
    t(u)
}

function v(r) {
    var f=n.protocolNameForClient,
    u;

    if(n.protocolVersion==1) {

        var e="play",
        o=i("RequestFollowUser", r),
        s= {
            placelauncherurl: o, browsertrackerid:Roblox.Cookies.getBrowserTrackerId()
        }

        ;

        u= {
            protocolName: f, launchMode:e, otherParams:s
        }
    }

    else r.request="RequestFollowUser",
    u= {
        protocolName: f, otherParams:r
    }

    ;
    return u.protocolVersion=n.protocolVersion,
    t(u)
}

function a(r) {
    var f=n.protocolNameForClient,
    u;

    if(n.protocolVersion==1) {

        var e="play",
        o=i("RequestPlayWithParty", r),
        s= {
            placelauncherurl: o, browsertrackerid:Roblox.Cookies.getBrowserTrackerId()
        }

        ;

        u= {
            protocolName: f, launchMode:e, otherParams:s, placeId:r.placeId
        }
    }

    else r.request="RequestPlayWithParty",
    u= {
        protocolName: f, otherParams:r
    }

    ;
    return u.protocolVersion=n.protocolVersion,
    t(u)
}

function l(r) {
    var f=n.protocolNameForClient,
    u;

    if(n.protocolVersion==1) {

        var e="play",
        o=i("RequestGameJob", r),
        s= {
            placelauncherurl: o, browsertrackerid:Roblox.Cookies.getBrowserTrackerId()
        }

        ;

        u= {
            protocolName: f, launchMode:e, otherParams:s, placeId:r.placeId
        }
    }

    else r.request="RequestGameJob",
    u= {
        protocolName: f, otherParams:r
    }

    ;
    return u.protocolVersion=n.protocolVersion,
    t(u)
}

function c(r) {
    var f=n.protocolNameForClient,
    u;

    if(n.protocolVersion==1) {

        var e="play",
        o=i("RequestPrivateGame", r),
        s= {
            placelauncherurl: o, browsertrackerid:Roblox.Cookies.getBrowserTrackerId()
        }

        ;

        u= {
            protocolName: f, launchMode:e, otherParams:s, placeId:r.placeId
        }
    }

    else r.request="RequestPrivateGame",
    u= {
        protocolName: f, otherParams:r
    }

    ;
    return u.protocolVersion=n.protocolVersion,
    t(u)
}

function h(n) {
    var t=n.gameInfo;

    typeof n.gameInfo !="undefined" &&(n.gameInfo=undefined),
    $(Roblox.GameLauncher).trigger(Roblox.GameLauncher.beginInstallEvent, {
        launchMethod:"Protocol", params:n
    }),
u= !0,
n.url=window.location.href,
typeof t !="undefined" &&(n.gameInfo=t),
r()
}

function s() {
    $(Roblox.GameLauncher).trigger(Roblox.GameLauncher.manualDownloadEvent, {
        launchMethod:"Protocol", params: {}
    }),
r()
}

function r() {
    var n=document.getElementById("downloadInstallerIFrame");
    n.src="/install/setup.ashx"
}

var u= !1,
f=0,
n= {
    waitTimeBeforeFailure: 300, protocolNameForStudio:"roblox-studio", protocolNameForClient:"roblox-client", protocolUrlSeparator:"+", protocolUrlIncludesLaunchTime: !1, protocolDetectionEnabled: !1, protocolVersion:1, joinMultiplayerGame:y, openStudio:w, editGameInStudio:p, followPlayerIntoGame:v, joinGameWithParty:a, joinGameInstance:l, joinPrivateGame:c, manualDownload:s, startDownload:r, setLocationHref:ot, showDialog:d, showLaunchFailureDialog:k, cleanUpAndLogSuccess:it, cleanUpAndLogFailure:tt
}

;

return $(function() {
        Roblox.GameLauncher.gameLaunchInterface=Roblox.ProtocolHandlerClientInterface; var t=$("#PlaceLauncherStatusPanel"); n.protocolNameForClient=t.data("protocol-name-for-client"), n.protocolNameForStudio=t.data("protocol-name-for-studio"), n.protocolUrlIncludesLaunchTime=t.data("protocol-url-includes-launchtime"), n.protocolDetectionEnabled=t.data("protocol-detection-enabled"), n.protocolVersion=t.data("protocol-version"), n.logger||typeof Roblox.ProtocolHandlerLogger=="undefined" ||(n.logger=Roblox.ProtocolHandlerLogger)
    }),
n
}

,
Roblox.ProtocolHandlerClientInterface=Roblox.ProtocolHandlerImplementation();