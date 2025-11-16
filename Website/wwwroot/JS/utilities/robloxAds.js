"use strict";

var Roblox=Roblox|| {}

;

Roblox.AdsHelper=Roblox.AdsHelper|| {}

,
Roblox.AdsLibrary= {}

,
Roblox.AdsLibrary.adsIdList=["Skyscraper-Abp-Left",
"Skyscraper-Abp-Right",
"Leaderboard-Abp",
"GamePageAdDiv1",
"GamePageAdDiv2",
"GamePageAdDiv3",
"ProfilePageAdDiv1",
"ProfilePageAdDiv2"],
Roblox.AdsLibrary.adsParameters= {
    "Skyscraper-Abp-Left": {
        element: $(
            "#Skyscraper-Abp-Left"),
            template:null,
            fitTwoAds: !0,
            fitOneAd: !1,
            isSkyscraper: !0
    }

    ,
    "Skyscraper-Abp-Right": {
        element: $(
            "#Skyscraper-Abp-Right"),
            tempalte:null,
            fitTwoAds: !0,
            fitOneAd: !0,
            isSkyscraper: !0
    }

    ,
    "Leaderboard-Abp": {
        element: $(
            "#Leaderboard-Abp"),
            tempalte:null,
            fitTwoAds: !0,
            fitOneAd: !0,
            isLeaderboard: !0
    }

    ,
    GamePageAdDiv1: {
        element: $(
            "#GamePageAdDiv1"),
            template:null,
            isPageAd: !0
    }

    ,
    GamePageAdDiv2: {
        element: $(
            "#GamePageAdDiv2"),
            template:null,
            isPageAd: !0
    }

    ,
    GamePageAdDiv3: {
        element: $(
            "#GamePageAdDiv3"),
            template:null,
            isPageAd: !0
    }

    ,
    ProfilePageAdDiv1: {
        element: $(
            "#ProfilePageAdDiv1"),
            template:null,
            isPageAd: !0
    }

    ,
    ProfilePageAdDiv2: {
        element: $(
            "#ProfilePageAdDiv2"),
            template:null,
            isPageAd: !0
    }
}

,
Roblox.AdsHelper.AdRefresher=function() {
    function c(n) {
        return !n|| !$.trim($(n).html())
    }

    function h(n) {
        t.push(n)
    }

    function s() {
        return googletag.pubadsReady
    }

    function r(n) {
        var u,
        f,
        e,
        i;
        typeof n=="undefined" &&(n=v),
        u= !1;

        for(f in t) {
            if(e="#" +t[f]+" [data-js-adtype]", i=$(e), typeof i=="undefined")return;
            if(i.attr("data-js-adtype")==="iframead")o(i
        );
        else if(i.attr("data-js-adtype")==="gptAd")if(s())u= !0;

        else if(n>0) {
            setTimeout(function() {
                    r(--n)
                }

                , y);
            break
        }
    }

    u&&googletag.cmd.push(function() {
            googletag.pubads().refresh()
        })
}

function o(n) {
    var i=n.attr("src"),
    r,
    u,
    t;
    typeof i=="string" &&(r=i.indexOf("?")<0?"?":"&", u=i+r+"nocache=" +(new Date).getMilliseconds().toString(), typeof n[0] !="undefined")&&(t=n[0].contentDocument, t===undefined&&(t=n[0].contentWindow), t.location.href !=="about:blank" &&t.location.replace(u))
}

function n(n, t, i) {
    i.length&&c(i)&&(i.append(n), r())
}

function b() {
    for(var u=$(window).width(), f, r, t, i=0; i<Roblox.AdsLibrary.adsIdList.length; i++)r=Roblox.AdsLibrary.adsIdList[i],
    t=Roblox.AdsLibrary.adsParameters[r],
    t&& !t.template&&(f=t.element, t.template=f.html());
    for(i=0; i<Roblox.AdsLibrary.adsIdList.length; i++)(r=Roblox.AdsLibrary.adsIdList[i], t=Roblox.AdsLibrary.adsParameters[r], t)&&(t.isSkyscraper?u>=e&&t.fitTwoAds?n(t.template, r, t.element):u<e&&u>=l?t.fitOneAd?n(t.template, r, t.element):t.element.empty():t.element.empty():t.isLeaderboard?u<p?t.element.empty():n(t.template, r, t.element):t.isPageAd&&(u<w?t.element.empty():n(t.template, r, t.element)))
}

var t=[],
i=20,
u=970,
f=160,
p=728,
y=100,
v=16,
e=$("[data-max-width-for-two-ads]").attr("data-max-width-for-two-ads")||u+f*2+48-i,
l=$("[data-max-width-for-one-ads]").attr("data-max-width-for-one-ads")||u+f+24-i,
a=1024,
w=a-i;

return {
    registerAd: h, refreshAds:r, adjustAdsFitScreen:b, getAds:n
}
}

(),
Roblox.AdsHelper.DynamicAdCreator=function() {
    function n() {
        var n=$(".dynamic-ad").filter(".unpopulated");

        n.each(function(n, t) {
                var i=$(t), u=i.attr("data-ad-slot"), f=parseInt(i.attr("data-ad-width")), e=parseInt(i.attr("data-ad-height")), r=i.children(".ad-slot").attr("id"); googletag.cmd.push(function() {
                        var n=googletag.defineSlot(u, [f, e], r).addService(googletag.pubads()); googletag.display(r), googletag.pubads().refresh([n])
                    }), i.removeClass("unpopulated")
            })
    }

    return {
        populateNewAds: n
    }
}

(),
$(function() {
        $(window).resize(function() {
                Roblox.AdsHelper.AdRefresher.adjustAdsFitScreen()
            })
    });