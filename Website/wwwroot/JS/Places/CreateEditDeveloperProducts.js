// Places/CreateEditDeveloperProducts.js
$(function() {
    Roblox || (Roblox = {}), Roblox.DeveloperProductsForm || (Roblox.DeveloperProductsForm = {}), Roblox.DeveloperProductsForm.init = function() {
        var n = $("#AddDeveloperProduct").parent(),
            t, i;
        n.trigger("onViewChange", ["createEdit"]), n.unbind("onRefreshed").bind("onRefreshed", function() {
            Roblox.DeveloperProductsForm.onAjaxStart(), $.ajax({
                cache: !1,
                type: "GET",
                url: n.attr("src")
            }).done(function(t) {
                $("#DeveloperProductsLoading").hide(), n.trigger("onActionComplete", [t])
            }).fail(function() {
                $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").show()
            })
        }), $("body").unbind("developerProductImageIFrameLoaded").bind("developerProductImageIFrameLoaded", function(n, t) {
            Roblox.DeveloperProductsForm.onImageUploadComplete(!0, t)
        }), $("body").unbind("developerProductImageIFrameLoadError").bind("developerProductImageIFrameLoadError", function(n, t) {
            Roblox.DeveloperProductsForm.onImageUploadComplete(!1, t)
        }), t = function(n) {
            Roblox.DeveloperProductsForm.onAjaxSuccess(), $("#AddDeveloperProduct").parent().trigger("onActionComplete", [n])
        }, i = function() {
            Roblox.DeveloperProductsForm.onAjaxFailure()
        };
        $("#AddDeveloperProduct").off("click").on("click", "a.developer-product-button", function() {
            var u = $(this).data("form-post-url"),
                r = Roblox.DeveloperProductsForm.validateAndGetFormValues();
            r != null && (Roblox.DeveloperProductsForm.onAjaxStart(), $.ajax({
                cache: !1,
                url: u,
                type: "POST",
                data: r,
                success: t,
                error: i
            }))
        }).on("click", "a.cancel-button", function() {
            var r = $(this).data("url");
            return Roblox.DeveloperProductsForm.onAjaxStart(), $.ajax({
                cache: !1,
                url: r,
                type: "GET",
                success: t,
                error: i
            }), !1
        });
        $("#DeveloperProductImageFile").on("change", function() {
            var i = $(this).val(),
                t;
            return i && i != "" && (Roblox.DeveloperProductsForm.onAjaxStart(), $("#ImageUploadForm").submit(), t = $("#ImageUploaderIframe"), t.unbind().load(function() {
                Roblox.DeveloperProductsForm.onAjaxSuccess(), $("#AddDeveloperProductInnerContainer").show(), t.show()
            })), !1
        });
        $("#DeveloperProductName").unbind("focusout").bind("focusout", function() {
            var n = $(this).attr("validation-url"),
                i = $(this).val(),
                t = $("#NameValidation");
            return t.hide(), $(this).attr("invalid", !1), i != null && i.length > 0 && (n = n + (n.indexOf("?") !== -1 ? "&" : "?"), n = n + "developerProductName=" + i, $.ajax({
                cache: !1,
                url: n,
                type: "GET"
            }).done(function(n) {
                n.Success ? (t.hide(), $("#DeveloperProductName").attr("invalid", "false")) : (t.show().text(n.Message), t.removeClass("validationMessageInvalid").addClass("validationMessageInvalid"), $("#DeveloperProductName").attr("invalid", "true"))
            }).fail(function() {})), !0
        })
    }, Roblox.DeveloperProductsForm.onImageUploadComplete = function(n, t) {
        var i = $("#DeveloperProductIconId");
        n ? (i.val(t), i.attr("uploaded", "true")) : i.attr("uploaded", "false"), Roblox.DeveloperProductsForm.onAjaxSuccess(), $("#AddDeveloperProductInnerContainer").show(), $("#ImageUploaderIframe").show()
    }, Roblox.DeveloperProductsForm.validatePrice = function(n) {
        var t = isNaN(n) ? "Please enter a valid number" : "";
        return t = t.length == 0 && parseInt(n) < 0 ? "Please enter a value above zero" : t
    }, Roblox.DeveloperProductsForm.showValidation = function(n, t) {
        t.length > 0 && ($(n).show().text(t), $(n).removeClass("validationMessageInvalid").addClass("validationMessageInvalid"))
    }, Roblox.DeveloperProductsForm.onAjaxStart = function() {
        $(".validationMessage").hide(), $("#DeveloperProductsLoading").show(), $("#AddDeveloperProductInnerContainer").hide()
    }, Roblox.DeveloperProductsForm.onAjaxSuccess = function() {
        $("#DeveloperProductsLoading").hide()
    }, Roblox.DeveloperProductsForm.onAjaxFailure = function() {
        $("#DeveloperProductsLoading").hide(), $("#AddDeveloperProductInnerContainer").hide(), $("#DeveloperProductsError").show()
    }, Roblox.DeveloperProductsForm.validateAndGetFormValues = function() {
        var u, f, n, e, s;
        $(".validationMessage").hide();
        var r, h = $("#DeveloperProductUniverseID").val(),
            c = $("#DeveloperProductID").val(),
            o = $("#DeveloperProductName"),
            l = o.attr("invalid"),
            t = "",
            i = "";
        return l == "true" ? t = $("#NameValidation").text() : (i = o.val(), t = !i || $.trim(i).length == 0 ? "Name cannot be empty" : ""), Roblox.DeveloperProductsForm.showValidation("#NameValidation", t), u = $("#DeveloperProductPriceInRobux").val(), f = Roblox.DeveloperProductsForm.validatePrice(u), Roblox.DeveloperProductsForm.showValidation("#RobuxValidation", f), n = $("#DeveloperProductDescription").val(), n = n != null ? n : "", e = $("#DeveloperProductIconId"), s = e.val(), r = t.length > 0 || f.length > 0 || e.attr("uploaded") == "false" ? null : {
            universeId: h,
            name: i,
            developerProductId: c,
            priceInRobux: u,
            description: n,
            imageAssetId: s
        }
    }
});