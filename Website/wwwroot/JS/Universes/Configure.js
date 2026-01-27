// Universes/Configure.js
$(function() {
    function n() {
        return $(".PlaceSelectorScript").length > 0
    }

    function t(t) {
        o(t);
        var i = t.data("maindiv");
        $("#" + i).show(), i !== "places" || n() || Roblox.PlaceSelector.Init(), $.address && $.address.hash(i)
    }

    function u(t) {
        $("#places").html(t), $(function() {
            n() || Roblox.PlaceSelector.Init()
        })
    }

    function showMessage(message, isError) {
        var currentTab = $('.verticaltab.selected').data('maindiv');
        var $messageContainer;
        
        if (currentTab === 'icon') {
            $messageContainer = $('#UploadStatus');
        } else if (currentTab === 'thumbnails') {
            $messageContainer = $('#thumbnailResponse');
        } else {
            $messageContainer = $('#thumbnailResponse');
        }
        
        if ($messageContainer.length === 0) {
            alert(message);
            return;
        }
        
        $messageContainer.empty();
        var $messageSpan = $('<span>')
            .addClass(isError ? 'status-error' : 'status-confirm')
            .text(message);
        
        $messageContainer.append($messageSpan);
        $messageContainer.show();
        setTimeout(function() {
            $messageContainer.fadeOut(500, function() {
                $messageContainer.empty();
            });
        }, 5000);
    }
    
    window.showMessage = showMessage;

    function f() {
        var currentTab = $('.verticaltab.selected').data('maindiv');
        
        if (currentTab === 'basicSettings') {
            var n = $("#Name").val().trim();
            if ($(".name-error").hide(), n == "") {
                $(".name-error").show();
                return
            }
        }
        
        e();
        
        function collectAllFormData() {
            var basicInfo = {
                id: $("#Id").val() || "",
                name: $("#Name").val() || "",
                description: $("#Description").val() || "",
                genre: $("#Genre").val() || "All"
            };
            
            var playableDevices = [];
            $('input[name^="PlayableDevices"][type="checkbox"]:checked').each(function() {
                var deviceType = $(this).siblings('input[name$=".DeviceType"]').val();
                if (deviceType) {
                    playableDevices.push(deviceType);
                }
            });
            
            var formData = {
                Id: basicInfo.id,
                Name: basicInfo.name,
                Description: basicInfo.description,
                Genre: basicInfo.genre,
                CharacterForce: $("input[name='CharacterForce']:checked").val(),
                ScaleChoice: $("input[name='ScaleChoice']:checked").val(),
                IconType: $('input[name="iconType"]:checked').val() || '',
                ThumbnailType: $('input[name="thumbnailType"]:checked').val() || '',
                NumberOfPlayersMax: $('#MaxPlayersInput').val() || '8',
                SocialSlotType: $('input[name="SocialSlotType"]:checked').val() || 'Automatic',
                NumberOfCustomSocialSlots: $('#FriendSlotsInput').val() || '4',
                Access: $('#Access').val() || 'Everyone',
                ArePrivateServersAllowed: $('#AllowPrivateServersCheckbox').is(':checked'),
                IsFreePrivateServer: $('input[name="IsFreePrivateServer"]:checked').val() === 'True',
                PrivateServersPrice: $('#PrivateServerPriceInput').val() || '100',
                PlayableDevices: playableDevices.join(', '),
                SellGameAccess: $('#SellGameAccessCheckbox').is(':checked'),
                Price: $('#PriceInput').val() || '0',
                IsCopyingAllowed: $('#IsCopyingAllowed').is(':checked'),
                IsAllGenresAllowed: $('input[name="IsAllGenresAllowed"]:checked').val() === 'True',
                AllowedGearTypes: collectAllowedGearTypes(),
                AllowPlaceToBeCopiedInGame: $('#AllowPlaceToBeCopiedInGame').is(':checked'),
                AllowPlaceToBeUpdatedInGame: $('#AllowPlaceToBeUpdatedInGame').is(':checked')
            };
            
            return formData;
        }

        function collectAllowedGearTypes() {
            var allowedGearTypes = [];
            $('input[name^="AllowedGearTypes"][type="checkbox"]:checked').each(function() {
                var categoryInput = $(this).closest('li').find('input[name$=".Category"]');
                if (categoryInput.length > 0) {
                    var category = categoryInput.val();
                    if (category) {
                        var categoryId = getGearTypeIdFromCategory(category);
                        if (categoryId) {
                            allowedGearTypes.push(categoryId);
                        }
                    }
                }
            });
            return JSON.stringify(allowedGearTypes);
        }
        
        function getGearTypeIdFromCategory(category) {
            var categoryMap = {
                'Melee': 1,
                'PowerUps': 2,
                'Ranged': 3,
                'Navigation': 4,
                'Explosive': 5,
                'Musical': 6,
                'Social': 7,
                'PersonalTransport': 8,
                'Building': 9
            };
            return categoryMap[category] || null;
        }
        
        var savePromise = new Promise(function(resolve, reject) {
            var formData = collectAllFormData();
            
            if (!formData.Id) {
                resolve({ hasError: false, type: 'all' });
                return;
            }
            
            $.ajax({
                url: "/places/doconfigure2",
                method: "POST",
                headers: { "X-Requested-With": "XMLHttpRequest" },
                data: formData,
                traditional: true,
                success: function(response) {0
                    if (response && response.success === false) {
                        resolve({ hasError: true, error: response.message || 'An error occurred while saving.', type: 'all' });
                    } else {
                        resolve({ hasError: false, type: 'all' });
                    }
                },
                error: function(xhr, status, error) {
                    var errorMessage = "An error occurred while saving.";
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    resolve({ hasError: true, error: errorMessage, type: 'all' });
                }
            });
        });
        
        if (currentTab === 'icon') {
            var iconType = $('input[name="iconType"]:checked').val();
            if (iconType === 'image' && $('#iconImageFile')[0].files.length > 0) {
                var uploadPromise = new Promise(function(resolve, reject) {
                    var uploadFormData = new FormData();
                    uploadFormData.append('iconImageFile', $('#iconImageFile')[0].files[0]);
                    uploadFormData.append('placeId', $('#IconDisplayContainer').data('place-id'));
                    
                    $.ajax({
                        url: '/places/icons/add-icon',
                        type: 'POST',
                        data: uploadFormData,
                        cache: false,
                        contentType: false,
                        processData: false,
                        success: function(response) {
                            if (response && response.success === false) {
                                resolve({ hasError: true, error: response.error || response.message || 'An error occurred while uploading icon.', type: 'icon' });
                            } else {
                                resolve({ hasError: false, type: 'icon' });
                            }
                        },
                        error: function(xhr, status, error) {
                            var errorMessage = "An error occurred while uploading icon.";
                            if (xhr.responseJSON && xhr.responseJSON.error) {
                                errorMessage = xhr.responseJSON.error;
                            }
                            resolve({ hasError: true, error: errorMessage, type: 'icon' });
                        }
                    });
                });
                
                Promise.all([savePromise, uploadPromise]).then(function(results) {
                    var hasErrors = results.some(function(result) { return result.hasError; });
                    var errors = results.filter(function(result) { return result.hasError; });
                    
                    if (hasErrors) {
                        errors.forEach(function(error) {
                            showMessage(error.error, true);
                        });
                    } else {
                        var currentTab = $('.verticaltab.selected').data('maindiv');
                        if (currentTab) {
                            sessionStorage.setItem('activeConfigureTab', currentTab);
                        }
                        location.reload();
                    }
                }).catch(function(error) {
                    console.error('Save error:', error);
                    showMessage('An error occurred while saving. Please try again.', true);
                }).finally(function() {
                    $.modal.close();
                });
                return;
            }
        }
        
        savePromise.then(function(result) {
            if (result.hasError) {
                showMessage(result.error, true);
            } else {
                var currentTab = $('.verticaltab.selected').data('maindiv');
                if (currentTab) {
                    sessionStorage.setItem('activeConfigureTab', currentTab);
                }
                location.reload();
            }
        }).catch(function(error) {
            console.error('Save error:', error);
            showMessage('An error occurred while saving. Please try again.', true);
        }).finally(function() {
            $.modal.close();
        });
    }

    function e() {
        var n = {
            overlayClose: !1,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            escClose: !1
        };
        typeof closeClass != "undefined" && closeClass !== "" && $.modal.close("." + closeClass), $("#ProcessingView").modal(n)
    }

    function o(n) {
        $(".configure-tab").hide(), $("#navbar div.selected").removeClass("selected"), n.addClass("selected")
    }

    function i(n, t, i) {
        $("#universe-error").hide(), $(n).toggle(), $(n).next(".loading-button").toggle();
        var f = $("#universe-configure").data("universeid"),
            r = {
                placeId: t,
                universeId: f
            },
            e = $("#universe-configure").data("configureplaceurl");
        $.post(i, r, function(t) {
            t.success ? $.ajax({
                url: e,
                data: r,
                success: function(n) {
                    u(n)
                },
                cache: !1
            }) : ($(n).toggle(), $(n).next(".loading-button").toggle(), $("#universe-error").text(t.message), $("#universe-error").show())
        }), $("[data-retry-url]").loadRobloxThumbnails(), $(".tooltip-top").tipsy({
            gravity: "s"
        })
    }

    function r() {
        var n = $.address ? $.address.hash() : "",
            i;
        n = n.replace("/", "").escapeHTML(), n.length == 0 && (n = "basicSettings"), i = $('[data-maindiv="' + n + '"]'), i.length > 0 && t(i)
    }
    $(".verticaltab").click(function() {
        return t($(this)), !1
    });
    var s = $("#UniverseAvatarType:checked").attr("value");
    $(document).ready(function() {
        $(".gameavatartype:checked").click()
    }), $("#okButton").click(function() {
        f();
    });
    $("#icon").on("click", ".configure-save-button", function() {
        f();
    });
    $("#thumbnails").on("click", ".configure-save-button", function() {
        f();
    });
    $("#access").on("click", ".configure-save-button", function() {
        f();
    });
    $("#permissions").on("click", ".configure-save-button", function() {
        f();
    });
    $("#games").on("click", ".configure-save-button", function() {
        f();
    });
    $("#universe-configure").on("click", ".add-place-button", function() {
        var n = this;
        Roblox.PlaceSelector.Open(function(t) {
            var r = $("#universe-configure").data("addplaceurl");
            i(n, t, r)
        })
    });
    $("#universe-configure").on("click", ".remove-place-button", function() {
        var n = $(this).data("placeid"),
            t = $("#universe-configure").data("removeplaceurl");
        i(this, n, t)
    });
    $("#universe-configure").on("click", ".load-more-places-button", function() {
        var t = $(this).parent(),
            n = t.parent(),
            i = n.find(".universe-place-container").length,
            r = $("#current-places").data("universeid"),
            u = n.data("isuniversecreation"),
            f = {
                startRow: i,
                universeId: r,
                isUniverseCreation: u
            },
            e = $("#universe-configure").data("loadmoreplacesurl");
        return $.ajax({
            url: e,
            cache: !1,
            data: f,
            dataType: "html",
            success: function(i) {
                t.remove();
                var r = $(i);
                r.hide().appendTo(n).fadeIn(), $("[data-retry-url]").loadRobloxThumbnails()
            }
        }), !1
    });
    $("[data-retry-url]").loadRobloxThumbnails(), r(), $.address && $.address.externalChange(r)
});