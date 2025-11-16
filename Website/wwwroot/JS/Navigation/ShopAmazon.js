Roblox=Roblox|| {}

,
Roblox.ShopAmazon=function() {
    function t() {
        $("a.roblox-shop-interstitial").on("click", function(n) {
                n.preventDefault(), Roblox.Dialog.open({
                    titleText:"You are leaving ROBLOX", bodyContent:r(), allowHtmlContentInBody: !0, acceptText:"Continue to Shop", declineText:"Cancel", xToCancel: !0, acceptColor:Roblox.Dialog.green, declineColor:Roblox.Dialog.white, onAccept:i
                })
        })
}

function i() {
    window.open(n, "_blank")
}

function r() {
    return"<p>Your are about to visit our amazon store. You will be redirected to ROBLOX merchandise store on <a class='text-link' target='_blank' href='"+n+"' >Amazon.com</a>.</p> <p>Please note that you need to be over 18 to purchase products online. The amazon store is not part of ROBLOX.com and is governed by a separate privacy policy.</p>"
}

function u() {
    t()
}

var n="https://www.amazon.com/roblox?&_encoding=UTF8&tag=r05d13-20&linkCode=ur2&linkId=4ba2e1ad82f781c8e8cc98329b1066d0&camp=1789&creative=9325";

$(function() {
        u()
    })
}

();