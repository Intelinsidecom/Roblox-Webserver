Roblox=window.Roblox|| {}

,
Roblox.GameLauncher=function() {
    function t(t, i, r) {
        n.gameLaunchInterface.editGameInStudio(t, i, r)
    }

    function r() {
        n.gameLaunchInterface.openStudio()
    }

    function i(t, i) {
        var r,
        u;

        return typeof i=="undefined" &&(i= !0),
        r= {
            placeId: t, isPartyLeader: !1
        }

        ,
        r.isPartyLeader=n.partyInterface.isPartyLeader(),
        u=n.bcUpsellModalInterface.checkBcRequirement(r, i).then(n.genderFetcher.getCharacterGender).then(n.prerollPlayer.waitForPreroll).then(n.gameLaunchInterface.joinMultiplayerGame)
    }

    function u(t) {
        var i= {
            userId: t
        }

        ;
        return n.genderFetcher.getCharacterGender(i).then(n.prerollPlayer.waitForPreroll).then(n.gameLaunchInterface.followPlayerIntoGame)
    }

    function f(t, i, r) {
        var u= {
            placeId: t, partyGuid:i, gameId:r
        }

        ;
        return n.prerollPlayer.waitForPreroll(u).then(n.gameLaunchInterface.joinGameWithParty)
    }

    function e(t, i) {
        var u= {
            placeId: t, gameId:i
        }

        ;
        return n.genderFetcher.getCharacterGender(u).then(n.prerollPlayer.waitForPreroll).then(n.gameLaunchInterface.joinGameInstance)
    }

    function o(t, i, r) {
        var u= {
            placeId: t, accessCode:i, linkCode:r
        }

        ;
        return n.prerollPlayer.waitForPreroll(u).then(n.gameLaunchInterface.joinPrivateGame)
    }

    $(function() {
            n.gameLaunchLogger||typeof Roblox.GameLaunchLogger=="undefined" ||(n.gameLaunchLogger=Roblox.GameLaunchLogger), n.genderFetcher||typeof Roblox.GenderFetcher=="undefined" ||(n.genderFetcher=Roblox.GenderFetcher), n.prerollPlayer||typeof Roblox.PrerollPlayer=="undefined" ||(n.prerollPlayer=Roblox.PrerollPlayer), n.partyInterface||typeof Roblox.PartyInterface=="undefined" ||(n.partyInterface=Roblox.PartyInterface), n.bcUpsellModalInterface||typeof Roblox.BCUpsellModalInterface=="undefined" ||(n.bcUpsellModalInterface=Roblox.BCUpsellModalInterface), $("body").bindGameLaunch(), Roblox.PartyInterface.listenToIsPartyLeader()

        }),
    $.fn.bindGameLaunch=function() {
        return this.find(".VisitButtonPlayGLI").click(function() {
                var n=$(this), t=n.attr("placeid"), r=n.data("is-membership-level-ok"); i(t, r)

            }),
        this.find(".VisitButtonEditGLI").click(function() {
                var n=$(this), i=n.attr("placeid"), r=n.data("universeid"), u=n.data("allowupload")? !0: !1; t(i, r, u)
            }),
        this
    }

    ;

    var n= {
        genderFetcher: null, prerollPlayer:null, gameLaunchLogger:null, gameLaunchInterface:null, partyInterface:null, bcUpsellModalInterface:null, joinMultiplayerGame:i, openStudio:r, editGameInStudio:t, followPlayerIntoGame:u, joinGameWithParty:f, joinGameInstance:e, joinPrivateGame:o, startClientAttemptedEvent:"startClientAttempted", startClientFailedEvent:"startClientFailed", startClientSucceededEvent:"startClientSucceeded", beginInstallEvent:"beginInstall", successfulInstallEvent:"successfulInstall", manualDownloadEvent:"manualDownload"
    }

    ;
    return n
}

(),
Roblox.GenderFetcher=function() {
    function i(n, t) {
        Roblox.CharacterSelect.robloxLaunchFunction=function(i) {
            n.genderId=i,
            t.resolve(n)
        }

        ,
        Roblox.CharacterSelect.modalOpacity=80,
        Roblox.CharacterSelect.show()
    }

    function r(n) {
        var r="gameDetails",
        i;
        Roblox.FormEvents&&Roblox.FormEvents.SendInteractionClick(r, n),
        i=t.signupUrl+encodeURIComponent(window.location.pathname+window.location.search),
        window.location.href=Roblox&&Roblox.Endpoints?Roblox.Endpoints.getAbsoluteUrl(i): i
    }

    function e() {
        var t,
        i;
        return localStorage?(t=Date.parse(localStorage.getItem(n.lastPopUpDateKey)), !t||isNaN(t))? !0: (i=new Date, i.setDate(i.getDate()-1), new Date(t)<i): !0
    }

    function o(t) {
        localStorage&&localStorage.setItem(n.lastPopUpDateKey, t)
    }

    function s() {
        var n="/game/increment-play-count",
        t=Roblox&&Roblox.Endpoints?Roblox.Endpoints.getAbsoluteUrl(n): n;
        $.post(t)
    }

    function h(f) {
        var s=new $.Deferred,
        h,
        c;

        return $("#PlaceLauncherStatusPanel").data("is-user-logged-in")=="True" ?(s.resolve(f), s):(h="/game/require-auth", c=Roblox&&Roblox.Endpoints?Roblox.Endpoints.getAbsoluteUrl(h):h, $.get(c, null, function(h) {
                    var c=h&&h.RequireAuth, l; c&&h.AbTestToDisplay===u.removeGuestMode?Roblox.Dialog.open({
                        titleText:t.dialogTitle, bodyContent:t.dialogContent, acceptColor:Roblox.Dialog.green, acceptText:t.acceptText, declineText:t.declineText, onDecline:function() {
                            Roblox.FormEvents&&Roblox.FormEvents.SendInteractionClick(t.eventContext, t.loginField); var n=t.loginUrl+encodeURIComponent(window.location.pathname+window.location.search); window.location.href=Roblox&&Roblox.Endpoints?Roblox.Endpoints.getAbsoluteUrl(n):n
                        }

                        , onAccept:function() {
                            r(t.signupField)
                        }

                    }):c&&h.AbTestToDisplay===u.limitedGuestModeCalculatedInDays?h.DaysLeft<=0?Roblox.Dialog.open({
                    titleText:n.dialogTitleOver, bodyContent:n.dialogContentOver, acceptText:n.acceptText, declineText:n.declineTextOver, onAccept:function() {
                        r(n.signupFieldForDays)
                    }

                }):e()?(l=n.dialogContentForDaysPt2Plural, h.DaysLeft===1&&(l=n.dialogContentForDaysPt2Single), Roblox.Dialog.open({
                    titleText:n.dialogTitleCountdown, bodyContent:n.dialogContentForDaysPt1+h.DaysLeft+l, acceptText:n.acceptText, declineText:n.declineTextCountdown, onAccept:function() {
                        r(n.signupFieldForDays)
                    }

                    , onDecline:function() {
                        i(f, s)
                    }

                }), o(new Date)):i(f, s):c&&h.AbTestToDisplay===u.limitedGuestModeCalculatedInPlays?h.PlaysLeft<=0?Roblox.Dialog.open({
            titleText:n.dialogTitleOver, bodyContent:n.dialogContentOver, acceptText:n.acceptText, declineText:n.declineTextOver, onAccept:function() {
                r(n.signupFieldForPlays)
            }

        }):h.PlaysLeft%n.playCountPopUpFrequency==0?Roblox.Dialog.open({
        titleText:n.dialogTitleCountdown, bodyContent:n.dialogContentForPlaysPt1+h.PlaysLeft+n.dialogContentForPlaysPt2, acceptText:n.acceptText, declineText:n.declineTextCountdown, onAccept:function() {
            r(n.signupFieldForPlays)
        }

        , onDecline:function() {
            i(f, s)
        }
    }):i(f, s):i(f, s)
}), s)
}

var f;
$(Roblox.GameLauncher).on(Roblox.GameLauncher.startClientSucceededEvent, s);

var t= {
    dialogTitle: "Sign up or log in to play!", dialogContent:"To access features like playing games, customizing avatar, and chatting with friends, you'll need an account. Sign up or log in to play now.", acceptText:"Sign Up", declineText:"Log In", loginUrl:"/newlogin?returnurl=", signupUrl:"/account/signupredir?returnurl=", eventContext:"gameDetails", loginField:"gameLaunch_login", signupField:"gameLaunch_signup"
}

,
n= {
    dialogTitleCountdown: "Sign Up Today!", dialogContentForDaysPt1:"You are playing in guest mode. To use all features available on Roblox, you will need to create an account. You have less than ", dialogContentForDaysPt2Plural:" days left before we require free sign up.", dialogContentForDaysPt2Single:" day left before we require free sign up.", dialogContentForPlaysPt1:"You are playing in guest mode. To use all features available on Roblox, you will need to create an account. You have ", dialogContentForPlaysPt2:" gameplays left before we require free sign up.", dialogTitleOver:"Trial Over!", dialogContentOver:"Your trial period has ended. Please sign up to play games - it's free!", acceptText:"Sign up now!", declineTextCountdown:"OK", declineTextOver:"Close", signupFieldForDays:"gameLaunch_signup_limitedGuestDays", signupFieldForPlays:"gameLaunch_signup_limitedGuestPlays", lastPopUpDateKey:"LimitedGuestModeLastPopUpDate", playCountPopUpFrequency:5
}

,
u= {
    removeGuestMode: 1, limitedGuestModeCalculatedInDays:2, limitedGuestModeCalculatedInPlays:3
}

;

return f= {
    getCharacterGender: h
}
}

(),
Roblox.PrerollPlayer= {
    waitForPreroll:function(n) {
        var r=new $.Deferred,
        t=Roblox.VideoPreRollDFP,
        i,
        u;

        return t.placeId=typeof n.placeId !="undefined" ?n.placeId:0,
        t&&t.showVideoPreRoll&& !t.isExcluded(n.placeId)?(i= {
                escClose: !0, opacity:80, overlayCss: {
                    backgroundColor:"#000"
                }

                , zIndex:1031
            }

            , i.onShow=function(n) {
                t.correctIEModalPosition(n), t.start(), $("#prerollClose").hide(), $("#prerollClose").delay(1e3*t.adTime).show(300)
            }

            , i.onClose=function() {
                t.close()
            }

            , i.closeHTML='<a href="#" id="prerollClose" class="ImageButton closeBtnCircle_35h ABCloseCircle VprCloseButton"></a>', $("#videoPrerollPanel").modal(i), u=setInterval(function() {
                    t.isPlaying()||($.modal.close(), clearInterval(u), t.videoCancelled?r.reject(n):r.resolve(n))
                }

                , 200)):(r.resolve(n), t.logVideoPreRoll()),
        r
    }
}

,
Roblox.PartyInterface= {

    gameDetailsPageSelector:$("#game-detail-page"),
    dataAttributeName:"data-ispartyleader",
    isPartyLeaderEnabled:"True",
    isPartyLeaderDisabled:"False",
    isPartyLeader:function() {
        return Roblox.PartyInterface.gameDetailsPageSelector&&Roblox.PartyInterface.gameDetailsPageSelector.length>0?Roblox.PartyInterface.gameDetailsPageSelector.attr(Roblox.PartyInterface.dataAttributeName)===Roblox.PartyInterface.isPartyLeaderEnabled: !1
    }

    ,
    listenToIsPartyLeader:function() {
        $(document).on("Roblox.Party.IsPartyLeader", function(n, t) {
                var i=t.isPartyLeader?Roblox.PartyInterface.isPartyLeaderEnabled:Roblox.PartyInterface.isPartyLeaderDisabled; Roblox.PartyInterface.gameDetailsPageSelector.attr(Roblox.PartyInterface.dataAttributeName, i)
            })
    }
}

,
Roblox.BCUpsellModalInterface= {
    checkBcRequirement:function(n, t) {
        var i=new $.Deferred;
        return t?i.resolve(n): typeof Roblox.BCUpsellModal !="undefined" ?(Roblox.BCUpsellModal.open(), i.reject(n)):typeof showBCOnlyModal !="undefined" ?(showBCOnlyModal(), i.reject(n)):i.resolve(n), i
    }
}

;