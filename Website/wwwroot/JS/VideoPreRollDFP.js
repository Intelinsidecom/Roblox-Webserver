typeof Roblox=="undefined" &&(Roblox= {}),
Roblox.VideoPreRollDFP= {

    newValue:"",
    showVideoPreRoll: !1,
    videoInitialized: !1,
    videoStarted: !1,
    videoCompleted: !1,
    videoSkipped: !1,
    videoCancelled: !1,
    videoErrored: !1,
    loadingBarMaxTime:3e4,
    loadingBarCurrentTime:0,
    loadingBarIntervalID:0,
    loadingBarID:"videoPrerollLoadingBar",
    loadingBarInnerID:"videoPrerollLoadingBarCompleted",
    loadingBarPercentageID:"videoPrerollLoadingPercent",
    videoDiv:"videoPrerollMainDiv",
    companionAdDiv:"videoPrerollCompanionAd",
    contentElement:"contentElement",
    videoLoadingTimeout:7e3,
    videoPlayingTimeout:23e3,
    videoLogNote:"",
    logsEnabled: !1,
    excludedPlaceIds:"",
    isSwfPreloaderEnabled: !1,
    isFlashInstalled: !1,
    isPrerollShownEveryXMinutesEnabled: !1,
    isAgeTargetingEnabled: !0,
    isAgeOrSegmentTargetingEnabled: !1,
    adUnit:"",
    adTime:0,
    placeId:0,
    customTargeting: {
        userAge: "", userAgeOrSegment:"", userGender:"", gameGenres:"", environment:"", adTime:"", PLVU: !1
    }

    ,
    adsManager:null,
    adsLoader:null,
    adDisplayContainer:null,
    intervalTimer:null,
    videoContent:null,
    isCompanionAdRenderedByGoogleTag: !1,
    contentEndedListener:function() {
        adsLoader.contentComplete()
    }

    ,
    createVideoContent:function() {
        Roblox.VideoPreRollDFP.videoContent=document.getElementById(this.contentElement)
    }

    ,
    createAdDisplayContainer:function() {
        adDisplayContainer=new google.ima.AdDisplayContainer(document.getElementById(this.videoDiv), Roblox.VideoPreRollDFP.videoContent)
    }

    ,
    requestAds:function() {
        google.ima.settings.setVpaidAllowed( !0),
        this.createVideoContent(),
        this.createAdDisplayContainer(),
        adDisplayContainer.initialize(),
        adsLoader=new google.ima.AdsLoader(adDisplayContainer),
        adsLoader.addEventListener(google.ima.AdsManagerLoadedEvent.Type.ADS_MANAGER_LOADED, this.onAdsManagerLoaded, !1),
        adsLoader.addEventListener(google.ima.AdErrorEvent.Type.AD_ERROR, this.onAdError, !1),
        this.videoContent.addEventListener("ended", this.contentEndedListener);
        var n=new google.ima.AdsRequest,
        t=this.constructUrl();
        n.adTagUrl=t,
        n.linearAdSlotWidth=400,
        n.linearAdSlotHeight=300,
        n.nonLinearAdSlotWidth=400,
        n.nonLinearAdSlotHeight=300,
        adsLoader.requestAds(n)
    }

    ,
    constructUrl:function() {
        var r="http://pubads.g.doubleclick.net/gampad/ads?impl=s&gdfp_req=1&env=vp&output=xml_vast2&unviewed_position_start=1&url=[referrer_url]&description_url=[description_url]&correlator=[timestamp]",
        u="&sz=400x300",
        f="&iu="+this.adUnit,
        e="&ciu_szs=300x250",
        n="",
        t,
        i;
        return this.isAgeTargetingEnabled&&(n+="&Age=" +this.customTargeting.userAge),
        this.isAgeOrSegmentTargetingEnabled&&(n+="&A=" +this.customTargeting.userAgeOrSegment),
        t=encodeURIComponent("Env=" +this.customTargeting.environment+"&Gender=" +this.customTargeting.userGender+n+"&Genres=" +this.customTargeting.gameGenres+"&PlaceID=" +this.placeId+"&Time=" +this.customTargeting.adTime+"&PLVU=" +this.customTargeting.PLVU),
        i=r+u+f+e+"&cust_params="+t+"&",
        i
    }

    ,
    onAdsManagerLoaded:function(n) {
        adsManager=n.getAdsManager(Roblox.VideoPreRollDFP.videoContent),
        adsManager.addEventListener(google.ima.AdErrorEvent.Type.AD_ERROR, Roblox.VideoPreRollDFP.onAdError),
        adsManager.addEventListener(google.ima.AdEvent.Type.ALL_ADS_COMPLETED, Roblox.VideoPreRollDFP.onAdEvent),
        adsManager.addEventListener(google.ima.AdEvent.Type.LOADED, Roblox.VideoPreRollDFP.onAdEvent),
        adsManager.addEventListener(google.ima.AdEvent.Type.STARTED, Roblox.VideoPreRollDFP.onAdEvent),
        adsManager.addEventListener(google.ima.AdEvent.Type.SKIPPED, Roblox.VideoPreRollDFP.onAdEvent),
        adsManager.addEventListener(google.ima.AdEvent.Type.COMPLETE, Roblox.VideoPreRollDFP.onAdEvent);

        try {
            adsManager.init(400, 300, google.ima.ViewMode.NORMAL),
            adsManager.start()
        }

        catch(t) {
            Roblox.VideoPreRollDFP.onAdError()
        }
    }

    ,
    onAdEvent:function(n) {
        var i,
        t;

        switch(n.type) {
            case google.ima.AdEvent.Type.STARTED:if(Roblox.VideoPreRollDFP.videoStarted= !0, Roblox.VideoPreRollDFP.isCompanionAdRenderedByGoogleTag) {
                if( !googletag)break;

                function r() {
                    googletag.defineSlot(Roblox.VideoPreRollDFP.adUnit, [300, 250], Roblox.VideoPreRollDFP.companionAdDiv).addService(googletag.companionAds()),
                    googletag.enableServices(),
                    googletag.display(Roblox.VideoPreRollDFP.companionAdDiv)
                }

                googletag.cmd.push(r)
            }

            else if(i=n.getAd(), t=i.getCompanionAds(300, 250), t.length>0) {
                var u=t[0],
                f=u.getContent(),
                e=document.getElementById(Roblox.VideoPreRollDFP.companionAdDiv);
                e.innerHTML=f
            }

            break;
            case google.ima.AdEvent.Type.SKIPPED:Roblox.VideoPreRollDFP.videoCompleted= !0,
            Roblox.VideoPreRollDFP.videoSkipped= !0,
            Roblox.VideoPreRollDFP.showVideoPreRoll= !1;
            break;
            case google.ima.AdEvent.Type.COMPLETE:Roblox.VideoPreRollDFP.videoStarted&&Roblox.VideoPreRollDFP.videoCancelled== !1&&(Roblox.VideoPreRollDFP.videoCompleted= !0, Roblox.VideoPreRollDFP.showVideoPreRoll= !1, Roblox.VideoPreRollDFP.newValue !="" &&$.cookie("RBXVPR", Roblox.VideoPreRollDFP.newValue, 180))
        }
    }

    ,
    onAdError:function() {
        Roblox.VideoPreRollDFP.videoCompleted= !0,
        Roblox.VideoPreRollDFP.videoErrored= !0,
        Roblox.VideoPreRollDFP.videoLogNote="AdError"
    }

    ,
    checkEligibility:function() {
        Roblox.VideoPreRollDFP.showVideoPreRoll&&(Roblox.VideoPreRollDFP.checkFlashEnabled()&&(Roblox.VideoPreRollDFP.isFlashInstalled= !0), $("#PlaceLauncherStatusPanel").data("is-protocol-handler-launch-enabled")=="True" ||Roblox.Client.IsRobloxInstalled()?typeof google=="undefined" ||typeof google.ima=="undefined" ?(Roblox.VideoPreRollDFP.videoLogNote="NoGoogle", Roblox.VideoPreRollDFP.showVideoPreRoll= !1):Roblox.Client.isIDE()?(Roblox.VideoPreRollDFP.videoLogNote="RobloxStudio", Roblox.VideoPreRollDFP.showVideoPreRoll= !1):Roblox.Client.isRobloxBrowser()?(Roblox.VideoPreRollDFP.videoLogNote="RobloxPlayer", Roblox.VideoPreRollDFP.showVideoPreRoll= !1):(window.chrome||window.safari)&&window.location.hash=="#chromeInstall" &&(Roblox.VideoPreRollDFP.showVideoPreRoll= !1):Roblox.VideoPreRollDFP.showVideoPreRoll= !1)
    }

    ,
    checkFlashEnabled:function() {
        var n= !1,
        t;

        try {
            t=new ActiveXObject("ShockwaveFlash.ShockwaveFlash"),
            t&&(n= !0)
        }

        catch(i) {
            navigator.mimeTypes&&navigator.mimeTypes["application/x-shockwave-flash"] !=undefined&&navigator.mimeTypes["application/x-shockwave-flash"].enabledPlugin&&(n= !0)
        }

        return n
    }

    ,
    isExcluded:function(n) {
        var i,
        t;
        if(typeof n=="undefined" &&typeof play_placeId !="undefined" &&(n=play_placeId), Roblox.VideoPreRollDFP.showVideoPreRoll&&Roblox.VideoPreRollDFP.excludedPlaceIds !=="" &&(i=Roblox.VideoPreRollDFP.excludedPlaceIds.split(","), typeof n !="undefined"))for(t=0; t<i.length; t++)if(n==i[t])return Roblox.VideoPreRollDFP.videoLogNote="ExcludedPlace",
        !0;
        return !1
    }

    ,
    start:function() {
        this.placeId===0&&typeof play_placeId !="undefined" &&(this.placeId=play_placeId),
        this.videoInitialized= !0,
        this.videoStarted= !1,
        this.videoCancelled= !1,
        this.videoCompleted= !1,
        this.videoSkipped= !1,
        this.loadingBarCurrentTime=0,
        this.videoLogNote="";
        var n=1e3;

        LoadingBar.init(this.loadingBarID, this.loadingBarInnerID, this.loadingBarPercentageID),
        this.loadingBarIntervalID=setInterval(function() {
                Roblox.VideoPreRollDFP.loadingBarCurrentTime+=n, LoadingBar.update(Roblox.VideoPreRollDFP.loadingBarID, Roblox.VideoPreRollDFP.loadingBarCurrentTime/Roblox.VideoPreRollDFP.loadingBarMaxTime)
            }

            , n),
        this.isSwfPreloaderEnabled&&this.isFlashInstalled?this.renderImaPreloader():this.requestAds()
    }

    ,
    cancel:function() {
        this.videoCancelled= !0,
        $.modal.close()
    }

    ,
    skip:function() {
        this.videoCompleted= !0,
        this.videoSkipped= !0,
        this.showVideoPreRoll= !1
    }

    ,
    close:function() {
        MadStatus.running&&MadStatus.stop(""),
        RobloxLaunch.launcher&&(RobloxLaunch.launcher._cancelled= !0),
        clearInterval(this.loadingBarIntervalID),
        LoadingBar.dispose(this.loadingBarID),
        this.isPlaying()&&(this.videoCancelled= !0),
        $.modal.close(),
        this.logVideoPreRoll(),
        this.isPrerollShownEveryXMinutesEnabled&&this.videoInitialized&&this.videoCompleted&&this.updatePrerollCount()
    }

    ,
    logVideoPreRoll:function() {
        if(Roblox.VideoPreRollDFP.logsEnabled) {
            var n="";
            if(Roblox.VideoPreRollDFP.videoCompleted)n="Complete",
            Roblox.VideoPreRollDFP.videoLogNote=="" &&(Roblox.VideoPreRollDFP.videoLogNote="NoTimeout"),
            Roblox.VideoPreRollDFP.logsEnabled= !1;
            else if(Roblox.VideoPreRollDFP.videoCancelled)n="Cancelled",
            Roblox.VideoPreRollDFP.videoLogNote=RobloxLaunch.state;
            else if(Roblox.VideoPreRollDFP.videoInitialized== !1&&Roblox.VideoPreRollDFP.videoLogNote !="")n="Failed",
            Roblox.VideoPreRollDFP.logsEnabled= !1;
            else return;
            GoogleAnalyticsEvents.FireEvent(["DFPPreRoll", n, Roblox.VideoPreRollDFP.videoLogNote])
        }
    }

    ,
    isPlaying:function() {
        return Roblox.VideoPreRollDFP.videoInitialized?(Roblox.VideoPreRollDFP.videoInitialized&& !Roblox.VideoPreRollDFP.videoStarted&&Roblox.VideoPreRollDFP.loadingBarCurrentTime>Roblox.VideoPreRollDFP.videoLoadingTimeout&&(Roblox.VideoPreRollDFP.videoCompleted= !0, Roblox.VideoPreRollDFP.videoLogNote="LoadingTimeout"), Roblox.VideoPreRollDFP.videoStarted&& !Roblox.VideoPreRollDFP.videoCompleted&&Roblox.VideoPreRollDFP.loadingBarCurrentTime>Roblox.VideoPreRollDFP.videoPlayingTimeout&&(Roblox.VideoPreRollDFP.videoCompleted= !0, Roblox.VideoPreRollDFP.videoLogNote="PlayingTimeout"), !Roblox.VideoPreRollDFP.videoCompleted): !1
    }

    ,
    correctIEModalPosition:function(n) {
        if(n.container.innerHeight()<=30) {
            var t=$("#videoPrerollPanel"),
            i=-Math.floor(t.innerHeight()/2);

            t.css({
                position:"relative", top:i+"px"

            }),
        n.container.find(".VprCloseButton").css({
            top:i-10+"px", "z-index":"1003"
        })
}
}

,
test:function() {
    _popupOptions= {

        escClose: !0,
        opacity:80,
        overlayCss: {
            backgroundColor: "#000"
        }

        ,
        onShow:function() {
            Test.VideoPreRollDFP.start(),
            $("#prerollClose").hide(),
            $("#prerollClose").delay(1e3*Roblox.VideoPreRollDFP.adTime).show(300)
        }

        ,
        onClose:function() {
            Roblox.VideoPreRollDFP.close()
        }

        ,
        closeHTML:'<a href="#" class="ImageButton closeBtnCircle_35h ABCloseCircle VprCloseButton"></a>'
    }

    ,
    $("#videoPrerollPanel").modal(_popupOptions),
    MadStatus.running||(MadStatus.init($("#videoPrerollPanel").find(".MadStatusField"), $("#videoPrerollPanel").find(".MadStatusBackBuffer"), 2e3, 800), MadStatus.start()),
    $("#videoPrerollPanel").find(".MadStatusStarting").css("display", "none"),
    $("#videoPrerollPanel").find(".MadStatusSpinner").css("visibility", status===3||status===4||status===5?"hidden":"visible")
}

,
renderImaPreloader:function() {
    var n=encodeURIComponent(Roblox.VideoPreRollDFP.constructUrl()),
    t="adTagUrl="+n;

    $.ajax({
        url:"/game/preloader", data: {
            url:t
        }

        , method:"GET", crossDomain: !0, xhrFields: {
            withCredentials: !0
        }

    }).success(function(n) {
        $("#videoPrerollMainDiv").html(n), Roblox.VideoPreRollDFP.videoErrored||(Roblox.VideoPreRollDFP.videoStarted= !0)
    })
}

,
updatePrerollCount:function() {
    $.ajax({
        url:"/game/updateprerollcount", method:"GET", crossDomain: !0, xhrFields: {
            withCredentials: !0
        }
    })
}
}

;

var LoadingBar= {

    bars:[],
    init:function(n, t, i, r) {
        var u=this.get(n);

        u==null&&(u= {}),
        u.barID=n,
        u.innerBarID=t,
        u.percentageID=i,
        typeof r=="undefined" &&(u.percentComplete=0),
        this.bars.push(u),
        this.update(n, u.percentComplete)
    }

    ,
    get:function(n) {
        for(var t=0; t<this.bars.length; t++)if(this.bars[t].barID==n)return this.bars[t];
        return null
    }

    ,
    dispose:function(n) {
        for(var t=0; t<this.bars.length; t++)this.bars[t].barID==n&&this.bars.splice(t, 1)
    }

    ,
    update:function(n, t) {
        var i=this.get(n),
        r,
        u;

        i&&(t>1&&(t=1), r=$("#" +n).width(), u=Math.round(r*t), $("#" +i.innerBarID).animate({
                width:u
            }

            , 200, "swing"), i.percentageID&&$("#" +i.percentageID).length>0&&$("#" +i.percentageID).html(Math.round(t*100)+"%"), i.percentComplete=t)
}
}

;