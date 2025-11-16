function showTosModal() {
    Roblox.Dialog.open({
        titleText:title, bodyContent:body, acceptText:"I AGREE", onAccept:submitTosAgreement, acceptColor:Roblox.Dialog.blue, fieldValidationRequired: !1, dismissable: !1, xToCancel: !1, declineColor:Roblox.Dialog.none, allowHtmlContentInBody: !0
    })
}

function submitTosAgreement() {
    $.ajax({
        type:"POST", url:agreeUrl, contentType:"application/json; charset=utf-8", success:function() {}
    })
}

var title,
body,
agreeUrl,
submitUrl="/UserCheck/show-tos";

$(function() {
        var n=$("#TosAgreementInfo").data("terms-check-needed"); $.ajax({
            type:"GET", url:submitUrl, data: {
                isLicensingTermsCheckNeeded:n
            }

            , contentType:"application/json; charset=utf-8", success:function(n) {
                n.partialViewName&&(n.partialViewName==="CaptureTosAgreement_v2" ?(title="TERMS OF USE HAVE CHANGED", body='By clicking "I Agree", you are agreeing to the <a href="https://www.roblox.com/info/terms" class="text-link" target="_blank">Terms of Use</a> and <a href="https://www.roblox.com/info/privacy" class="text-link" target="_blank">Privacy Policy</a>.', agreeUrl="/UserCheck/update-tos", showTosModal()):n.partialViewName==="CaptureTosAgreementWithExplicitLicenseClause" &&(title="TERMS OF USE AGREEMENT", body='By clicking "I Agree", you are agreeing to the <a href="https://www.roblox.com/info/terms" class="text-link" target="_blank">Roblox Terms of Use</a>. This includes the license to Roblox of past and future content you provide to the service for our use online, offline, and in tangible items.', agreeUrl="/UserCheck/update-tos-licensing", showTosModal()))
            }

            , error:function() {}
        })
});