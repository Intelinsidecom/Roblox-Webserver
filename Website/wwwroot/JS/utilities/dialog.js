typeof Roblox=="undefined" &&(Roblox= {}),
typeof Roblox.Dialog=="undefined" &&(Roblox.Dialog=function() {
        function tt(f) {
            var k, o, v, p, e, b; n.isOpen= !0, k= {
                titleText:"", bodyContent:"", footerText:"", acceptText:Roblox.Resources.Dialog.yes, declineText:Roblox.Resources.Dialog.No, acceptColor:r, declineColor:u, xToCancel: !1, onAccept:function() {
                    return !1
                }

                , onDecline:function() {
                    return !1
                }

                , onCancel:function() {
                    return !1
                }

                , imageUrl:null, showAccept: !0, showDecline: !0, allowHtmlContentInBody: !1, allowHtmlContentInFooter: !1, dismissable: !0, fieldValidationRequired: !1, onOpenCallback:function() {}

                , onCloseCallback:t, cssClass:null, checkboxAgreementText:Roblox.Resources.Dialog.Agree, checkboxAgreementRequired: !1
            }

            , f=$.extend({}

            , k, f), i.overlayClose=f.dismissable, i.escClose=f.dismissable, f.onCloseCallback&&(i.onClose=function() {
                f.onCloseCallback(), t()

            }), o=$(s), o.html(f.acceptText), o.attr("class", f.acceptColor), o.unbind(), o.bind("click", function() {
                return y(o)? !1:(f.fieldValidationRequired?w(f.onAccept):l(f.onAccept), !1)

            }), v=$(h), v.html(f.declineText), v.attr("class", f.declineColor), v.unbind(), v.bind("click", function() {
                return y(v)? !1:(l(f.onDecline), !1)

            }), p=$(g), p.unbind(), p.bind("change", function() {
                p.is(":checked")?a(o):c(o)

            }), e=$('[data-modal-type="confirmation"]'), e.find(".modal-title").text(f.titleText), f.imageUrl==null?e.addClass("noImage"):(e.find("img.modal-thumb").attr("src", f.imageUrl), e.removeClass("noImage")), n.extraClass&&(e.removeClass(n.extraClass), n.extraClass= !1), f.cssClass !=null&&(e.addClass(f.cssClass), n.extraClass=f.cssClass), f.allowHtmlContentInBody?e.find(".modal-message").html(f.bodyContent):e.find(".modal-message").text(f.bodyContent), f.checkboxAgreementRequired?(c(o), e.find(".modal-checkbox.checkbox > input").prop("checked", !1), e.find(".modal-checkbox.checkbox > label").text(f.checkboxAgreementText), e.find(".modal-checkbox.checkbox").show()):(e.find(".modal-checkbox.checkbox > input").prop("checked", !0), e.find(".modal-checkbox.checkbox").hide()), $.trim(f.footerText)=="" ?e.find(".modal-footer").hide():e.find(".modal-footer").show(), f.allowHtmlContentInFooter?e.find(".modal-footer").html(f.footerText):e.find(".modal-footer").text(f.footerText), e.modal(i), b=$(".modal-header .close"), b.unbind(), b.bind("click", function() {
                return l(f.onCancel), !1
            }), f.xToCancel||b.hide(), f.showAccept||o.hide(), f.showDecline||v.hide(), $("#rbx-body").addClass("modal-mask"), f.onOpenCallback()
    }

    function c(n) {
        n.hasClass(u)?n.addClass(o):n.hasClass(v)?n.addClass(f):n.hasClass(r)&&n.addClass(e)
    }

    function y(n) {
        return n.hasClass(e)||n.hasClass(o)||n.hasClass(f)? !0: !1
    }

    function p() {
        var n=$(s), t=$(h); c(n), c(t)
    }

    function a(n) {
        n.hasClass(o)?(n.removeClass(o), n.addClass(u)):n.hasClass(f)?(n.removeClass(f), n.addClass(v)):n.hasClass(e)&&(n.removeClass(e), n.addClass(r))
    }

    function b() {
        var n=$(s), t=$(h); a(n), a(t)
    }

    function k() {
        if(n.isOpen) {
            var t=$(s); t.click()
        }
    }

    function d() {
        var n=$(h); n.click()
    }

    function t(t) {
        n.isOpen= !1, typeof t !="undefined" ?$.modal.close(t):$.modal.close(), $("#rbx-body").removeClass("modal-mask")
    }

    function l(n) {
        t(), typeof n=="function" &&n()
    }

    function w(n) {
        if(typeof n=="function") {
            var i=n(); if(i !=="undefined" &&i== !1)return !1
        }

        t()
    }

    function it(n, t) {
        var i=$(".modal-body"); n?(i.find(".modal-btns").hide(), i.find(".modal-processing").show()):(i.find(".modal-btns").show(), i.find(".modal-processing").hide()), typeof t !="undefined" &&t !=="" &&$.modal.close("." +t)
    }

    var v="btn-primary-md", r="btn-secondary-md", u="btn-control-md", f="btn-primary-md disabled", e="btn-secondary-md disabled", o="btn-control-md disabled", nt="btn-none", s=".modal-btns #confirm-btn", h=".modal-btns #decline-btn", g="#modal-checkbox-input", n= {
        isOpen: !1
    }

    , i= {
        overlayClose: !0, escClose: !0, opacity:80, zIndex:1040, overlayCss: {
            backgroundColor:"#000"
        }

        , onClose:t, focus: !1
    }

    ; return {
        open:tt, close:t, disableButtons:p, enableButtons:b, clickYes:k, clickNo:d, status:n, toggleProcessing:it, green:v, blue:r, white:u, none:nt
    }
}

()),
$(document).keypress(function(n) {
        Roblox.Dialog.isOpen&&n.which===13&&Roblox.Dialog.clickYes()
    });