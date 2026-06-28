// Places/CreateEditDeveloperProducts.js
$(function () {
    Roblox || (Roblox = {}), Roblox.DeveloperProductsForm || (Roblox.DeveloperProductsForm = {}), Roblox.DeveloperProductsForm.init = function () {
        var n = $("#AddDeveloperProduct").parent(),
            t, i;
        n.trigger("onViewChange", ["createEdit"]), n.unbind("onRefreshed").bind("onRefreshed", function () {
            var refreshUrl = n.data("load-url") || n.attr("src");
            if (!refreshUrl) return;
            Roblox.DeveloperProductsForm.onAjaxStart(), $.ajax({
                cache: !1,
                type: "GET",
                url: refreshUrl
            }).done(function (t) {
                $("#DeveloperProductsLoading").hide(), n.trigger("onActionComplete", [t])
            }).fail(function () {
                $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").show()
            })
        }), $("body").unbind("developerProductImageIFrameLoaded").bind("developerProductImageIFrameLoaded", function (n, t) {
            Roblox.DeveloperProductsForm.onImageUploadComplete(!0, t)
        }), $("body").unbind("developerProductImageIFrameLoadError").bind("developerProductImageIFrameLoadError", function (n, t) {
            Roblox.DeveloperProductsForm.onImageUploadComplete(!1, t)
        }), t = function (n) {
            Roblox.DeveloperProductsForm.onAjaxSuccess();
            if (n.redirectUrl) {
                window.location.href = n.redirectUrl;
            } else {
                $("#DeveloperProductsFormContainer").hide();
                $("#AddDeveloperProduct").parent().trigger("onActionComplete", [n]);

                setTimeout(function () {
                    var contentHtml = $("#DeveloperProductsContent").html();
                    if (!contentHtml || contentHtml.length === 0) {
                        var universeId = $("#DeveloperProductUniverseID").val();
                        if (universeId) {
                            $.ajax({
                                cache: false,
                                url: "/universes/" + universeId + "/developer-products",
                                type: "GET",
                                success: function (response) {
                                    var devProductsElement = $("#DevProducts");
                                    if (devProductsElement.length > 0) {
                                        devProductsElement.html(response);
                                        if (typeof Roblox.DeveloperProductsListing !== 'undefined' &&
                                            typeof Roblox.DeveloperProductsListing.init === 'function') {
                                            Roblox.DeveloperProductsListing.init();
                                        }
                                        if (typeof Roblox.DeveloperProductsForm !== 'undefined' &&
                                            typeof Roblox.DeveloperProductsForm.init === 'function') {
                                            Roblox.DeveloperProductsForm.init();
                                        }
                                    } else if ($("#developerProducts").length > 0) {
                                        $("#developerProducts").html('<div id="DevProducts" data-load-url="/universes/' + universeId + '/developer-products" data-universe-id="' + universeId + '">' + response + '</div>');
                                        if (typeof Roblox.DeveloperProductsListing !== 'undefined' &&
                                            typeof Roblox.DeveloperProductsListing.init === 'function') {
                                            Roblox.DeveloperProductsListing.init();
                                        }
                                        if (typeof Roblox.DeveloperProductsForm !== 'undefined' &&
                                            typeof Roblox.DeveloperProductsForm.init === 'function') {
                                            Roblox.DeveloperProductsForm.init();
                                        }
                                    }
                                },
                                error: function () {
                                    $("#DeveloperProductsContent").show();
                                }
                            });
                        } else {
                            $("#DeveloperProductsContent").show();
                        }
                    } else {
                        $("#DeveloperProductsContent").show();
                    }

                    var devProductsTab = $(".verticaltab[data-maindiv='developerProducts']");
                    if (devProductsTab.length > 0) {
                        setTimeout(function () {
                            $(".configure-tab").hide();
                            $("#navbar div.selected").removeClass("selected");
                            devProductsTab.addClass("selected");
                            $("#developerProducts").show();
                            if (typeof $.address !== 'undefined') {
                                $.address.hash('developerProducts');
                            }
                        }, 50);
                    }
                }, 200);
            }
        }, i = function () {
            Roblox.DeveloperProductsForm.onAjaxFailure()
        };
        $("#AddDeveloperProduct").off("click").on("click", "a.developer-product-button", function () {
            var u = $(this).data("form-post-url"),
                formUrl = $(this).data("url");

            if ($("#DeveloperProductName").length === 0 || $("#DeveloperProductsFormContainer").is(':hidden')) {
                Roblox.DeveloperProductsForm.onAjaxStart();
                $("#DeveloperProductsContent").hide();

                $.ajax({
                    cache: false,
                    url: formUrl,
                    type: "GET",
                    success: function (response) {
                        if ($("#DeveloperProductsFormContainer").length === 0) {
                            $("#DeveloperProductsContent").after('<div id="DeveloperProductsFormContainer"></div>');
                        }
                        $("#DeveloperProductsFormContainer").html(response);
                        $("#DeveloperProductsFormContainer").show();
                        Roblox.DeveloperProductsForm.onAjaxSuccess();
                    },
                    error: function () {
                        $("#DeveloperProductsContent").show();
                        Roblox.DeveloperProductsForm.onAjaxFailure();
                    }
                });
            } else {
                var r = Roblox.DeveloperProductsForm.validateAndGetFormValues();
                if (r == null) {
                    return false;
                }
                Roblox.DeveloperProductsForm.onAjaxStart(), $.ajax({
                    cache: !1,
                    url: u,
                    type: "POST",
                    data: r,
                    success: t,
                    error: i
                });
            }
        }).on("click", "a.cancel-button", function () {
            var r = $(this).data("url");
            return Roblox.DeveloperProductsForm.onAjaxStart(), $.ajax({
                cache: !1,
                url: r,
                type: "GET",
                success: function (response) {
                    $("#DeveloperProductsFormContainer").hide();
                    var devProductsElement = $("#DevProducts");
                    if (devProductsElement.length > 0) {
                        devProductsElement.html(response);
                    } else if ($("#developerProducts").length > 0) {
                        $("#developerProducts").html('<div id="DevProducts" data-load-url="/universes/' + $("#DeveloperProductUniverseID").val() + '/developer-products" data-universe-id="' + $("#DeveloperProductUniverseID").val() + '">' + response + '</div>');
                    } else {
                        $("#DeveloperProductsContent").html(response);
                    }
                    $("#DeveloperProductsContent").show();
                    Roblox.DeveloperProductsForm.onAjaxSuccess();

                    // Re-bind events after content update
                    if (typeof Roblox.DeveloperProductsListing !== 'undefined' &&
                        typeof Roblox.DeveloperProductsListing.init === 'function') {
                        Roblox.DeveloperProductsListing.init();
                    }
                    if (typeof Roblox.DeveloperProductsForm !== 'undefined' &&
                        typeof Roblox.DeveloperProductsForm.init === 'function') {
                        Roblox.DeveloperProductsForm.init();
                    }
                },
                error: i
            }), !1
        });
        $("#DeveloperProductImageFile").on("change", function () {
            var fileInput = $(this);
            var file = fileInput[0].files[0];

            if (file && file.size > 0) {
                Roblox.DeveloperProductsForm.onAjaxStart();

                var hiddenForm = $("#ImageUploadForm");
                var hiddenFileInput = hiddenForm.find("input[name='image']");

                hiddenFileInput.replaceWith('<input type="file" name="image" />');
                hiddenFileInput = hiddenForm.find("input[name='image']");
                hiddenFileInput[0].files = fileInput[0].files;

                hiddenForm.submit();

                var iframe = $("#ImageUploaderIframe");
                iframe.unbind().load(function () {
                    try {
                        var iframeContent = iframe.contents();
                        var iframeBody = iframeContent.find('body');
                        var responseText = iframeBody.text() || iframeBody.html();
                        var hasImage = iframeContent.find('img').length > 0;

                        if (hasImage) {
                            var scripts = iframeContent.find('script');
                            var assetId = null;

                            scripts.each(function () {
                                var scriptContent = $(this).html();
                                if (scriptContent && scriptContent.includes('assetId')) {
                                    var match = scriptContent.match(/assetId:\s*(\d+)/);
                                    if (match) {
                                        assetId = parseInt(match[1]);
                                    }
                                }
                            });

                            if (assetId) {
                                Roblox.DeveloperProductsForm.onImageUploadComplete(true, assetId);
                            } else {
                                window.addEventListener('message', function handleMessage(event) {
                                    if (event.data && event.data.type === 'imageUploadComplete') {
                                        window.removeEventListener('message', handleMessage);
                                        if (event.data.success) {
                                            Roblox.DeveloperProductsForm.onImageUploadComplete(true, event.data.assetId);
                                        } else {
                                            Roblox.DeveloperProductsForm.onImageUploadComplete(false, null);
                                        }
                                    }
                                });
                            }
                        } else {
                            if (responseText) {
                                var response;
                                try {
                                    response = JSON.parse(responseText);
                                } catch (e) {
                                    response = responseText.trim();
                                }

                                if (response && response.success === true) {
                                    var imageId = response.assetId || response.imageId || response.id || response;
                                    Roblox.DeveloperProductsForm.onImageUploadComplete(true, imageId);
                                } else {
                                    Roblox.DeveloperProductsForm.onImageUploadComplete(false, null);
                                }
                            } else {
                                Roblox.DeveloperProductsForm.onImageUploadComplete(false, null);
                            }
                        }
                    } catch (e) {
                        Roblox.DeveloperProductsForm.onImageUploadComplete(false, null);
                    }

                    // Style the iframe to display the image properly
                    iframe.css({
                        'width': 'auto',
                        'height': 'auto',
                        'min-width': '256px',
                        'min-height': '256px',
                        'border': '1px solid #ccc',
                        'overflow': 'visible',
                        'display': 'inline-block',
                        'max-width': 'none',
                        'max-height': 'none'
                    });

                    setTimeout(function () {
                        try {
                            var iframeContent = iframe.contents();
                            iframeContent.find('body').css({
                                'margin': '0',
                                'padding': '0',
                                'overflow': 'hidden',
                                'display': 'flex',
                                'justify-content': 'center',
                                'align-items': 'center',
                                'min-height': '256px',
                                'min-width': '256px'
                            });
                            iframeContent.find('img').css({
                                'display': 'block',
                                'width': '1000px',
                                'height': '256px',
                                'object-fit': 'contain'
                            });
                        } catch (e) {
                            // Ignore cross-origin errors
                        }
                    }, 100);

                    Roblox.DeveloperProductsForm.onAjaxSuccess();
                    $("#AddDeveloperProductInnerContainer").show();
                    iframe.show();
                });
            }
            return false;
        });
        $("#DeveloperProductName").unbind("focusout").bind("focusout", function () {
            var n = $(this).attr("validation-url"),
                i = $(this).val(),
                t = $("#NameValidation");
            return t.hide(), $(this).attr("invalid", !1), i != null && i.length > 0 && (n = n + (n.indexOf("?") !== -1 ? "&" : "?"), n = n + "developerProductName=" + i, $.ajax({
                cache: !1,
                url: n,
                type: "GET"
            }).done(function (n) {
                n.Success ? (t.hide(), $("#DeveloperProductName").attr("invalid", "false")) : (t.show().text(n.Message), t.removeClass("validationMessageInvalid").addClass("validationMessageInvalid"), $("#DeveloperProductName").attr("invalid", "true"))
            }).fail(function () { })), !0
        })
    }, Roblox.DeveloperProductsForm.onImageUploadComplete = function (n, t) {
        var i = $("#DeveloperProductIconId");
        n ? (i.val(t), i.attr("uploaded", "true")) : i.attr("uploaded", "false"), Roblox.DeveloperProductsForm.onAjaxSuccess(), $("#AddDeveloperProductInnerContainer").show(), $("#ImageUploaderIframe").show()
    }, Roblox.DeveloperProductsForm.validatePrice = function (n) {
        var t = isNaN(n) ? "Please enter a valid number" : "";
        return t = t.length == 0 && parseInt(n) < 0 ? "Please enter a value above zero" : t
    }, Roblox.DeveloperProductsForm.showValidation = function (n, t) {
        if (t.length > 0) {
            $(n).show().text(t).removeClass("validationMessageInvalid").addClass("validationMessageInvalid");
        } else {
            $(n).hide();
        }
    }, Roblox.DeveloperProductsForm.onAjaxStart = function () {
        $(".validationMessage").hide(), $("#DeveloperProductsLoading").show(), $("#AddDeveloperProductInnerContainer").hide()
    }, Roblox.DeveloperProductsForm.onAjaxSuccess = function () {
        $("#DeveloperProductsLoading").hide()
    }, Roblox.DeveloperProductsForm.onAjaxFailure = function () {
        $("#DeveloperProductsLoading").hide(), $("#AddDeveloperProductInnerContainer").hide(), $("#DeveloperProductsError").show()
    }, Roblox.DeveloperProductsForm.validateAndGetFormValues = function () {
        var u, f, n, e, s;
        $(".validationMessage").hide();
        var r, h = $("#DeveloperProductUniverseID").val(),
            c = $("#DeveloperProductID").val(),
            o = $("#DeveloperProductName"),
            l = o.attr("invalid"),
            t = "",
            i = o.val();

        if (l == "true") {
            if (!i || $.trim(i).length == 0) {
                t = "Name cannot be empty";
            } else {
                o.attr("invalid", "false");
            }
        } else if (!i || $.trim(i).length == 0) {
            t = "Name cannot be empty";
        }
        Roblox.DeveloperProductsForm.showValidation("#NameValidation", t);

        u = $("#DeveloperProductPriceInRobux").val();
        f = Roblox.DeveloperProductsForm.validatePrice(u);
        Roblox.DeveloperProductsForm.showValidation("#RobuxValidation", f);

        var tixPrice = $("#DeveloperProductPriceInTix").val();
        var tixValidation = Roblox.DeveloperProductsForm.validatePrice(tixPrice);
        Roblox.DeveloperProductsForm.showValidation("#TixValidation", tixValidation);

        n = $("#DeveloperProductDescription").val();
        n = n != null ? n : "";
        e = $("#DeveloperProductIconId");
        s = e.val();

        r = t.length > 0 || f.length > 0 || tixValidation.length > 0 ? null : {
            universeId: h,
            name: i,
            developerProductId: c,
            priceInRobux: u,
            priceInTix: tixPrice,
            description: n,
            imageAssetId: s
        }
        return r;
    }
});