// Universes/Configure.js
$(function() {
    function n() {
        return $(".PlaceSelectorScript").length > 0
    }

    function t(t) {
        o(t);
        var i = t.data("maindiv");
        $("#" + i).show(), i !== "places" || n() || Roblox.PlaceSelector.Init(), $.address.hash(i)
    }

    function u(t) {
        $("#places").html(t), $(function() {
            n() || Roblox.PlaceSelector.Init()
        })
    }

    function f() {
        var currentTab = $('.verticaltab.selected').data('maindiv');
        console.log("Save button clicked, current tab:", currentTab);
        
        // Only validate name if we're on basic settings tab
        if (currentTab === 'basicSettings') {
            var n = $("#Name").val().trim();
            if ($(".name-error").hide(), n == "") {
                $(".name-error").show();
                return
            }
        }
        
        // Show processing modal immediately when save is clicked
        console.log("Showing processing modal");
        e();
        
        var savePromises = [];
        
        // Function to get consistent basic info from global fields or visible fields
        function getBasicInfo() {
            var idElement = $("#GlobalId").length ? $("#GlobalId") : $("#Id");
            var nameElement = $("#GlobalName").length ? $("#GlobalName") : $("#Name");
            var descElement = $("#GlobalDescription").length ? $("#GlobalDescription") : $("#Description");
            var genreElement = $("#GlobalGenre").length ? $("#GlobalGenre") : $("#Genre");
            
            return {
                id: idElement.val() || "",
                name: nameElement.val() || "",
                description: descElement.val() || "",
                genre: genreElement.val() || "All"
            };
        }
        
        // Only save basic info if we're on the basic settings tab
        if (currentTab === 'basicSettings') {
            var basicInfoPromise = new Promise(function(resolve, reject) {
                var basicInfo = getBasicInfo();
                
                if (!basicInfo.id) {
                    resolve({ hasError: false, type: 'basic' });
                    return;
                }
            
            var formData = {
                Id: basicInfo.id,
                Name: basicInfo.name,
                Description: basicInfo.description,
                Genre: basicInfo.genre,
                CharacterForce: $("input[name='CharacterForce']:checked").val(),
                ScaleChoice: $("input[name='ScaleChoice']:checked").val(),
                IconType: $('input[name="iconType"]:checked').val() || '',
                            };
            
            $.ajax({
                url: "/places/doconfigure2",
                method: "POST",
                headers: { "X-Requested-With": "XMLHttpRequest" },
                data: formData,
                traditional: true,
                success: function(response) {
                    resolve({ hasError: false, type: 'basic' });
                },
                error: function(xhr, status, error) {
                    var errorMessage = "An error occurred while saving basic info.";
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    resolve({ hasError: true, error: errorMessage, type: 'basic' });
                }
            });
        });
        savePromises.push(basicInfoPromise);
        }
        
        // If on icon tab, also save icon changes AND basic info
        if (currentTab === 'icon') {
            var iconPromise = new Promise(function(resolve, reject) {
                var iconType = $('input[name="iconType"]:checked').val();
                
                if (iconType === 'image' && $('#iconImageFile')[0].files.length > 0) {
                    // Upload custom image
                    var formData = new FormData();
                    formData.append('iconImageFile', $('#iconImageFile')[0].files[0]);
                    formData.append('placeId', $('#IconDisplayContainer').data('place-id'));
                    
                    $.ajax({
                        url: '/places/icons/add-icon',
                        type: 'POST',
                        data: formData,
                        cache: false,
                        contentType: false,
                        processData: false,
                        success: function(response) {
                            resolve({ hasError: false, type: 'icon' });
                        },
                        error: function(xhr, status, error) {
                            var errorMessage = "An error occurred while uploading the icon.";
                            if (xhr.responseJSON && xhr.responseJSON.error) {
                                errorMessage = xhr.responseJSON.error;
                            }
                            resolve({ hasError: true, error: errorMessage, type: 'icon' });
                        }
                    });
                } else {
                    // For autogenerated icons or no changes, let doconfigure2 handle it
                    // Skip the separate icon endpoint call to avoid duplicate processing
                    setTimeout(function() {
                        resolve({ hasError: false, type: 'icon', message: 'Icon type will be handled by main save' });
                    }, 100); // Short delay to show processing
                }
            });
            savePromises.push(iconPromise);
            
            // Also save basic info when on icon tab to ensure consistency across tabs
            var basicInfoPromise = new Promise(function(resolve, reject) {
                var basicInfo = getBasicInfo();
                var iconType = $('input[name="iconType"]:checked').val();
                
                if (!basicInfo.id) {
                    resolve({ hasError: false, type: 'basic' });
                    return;
                }
                
                var formData = {
                    Id: basicInfo.id,
                    Name: basicInfo.name,
                    Description: basicInfo.description,
                    Genre: basicInfo.genre,
                    CharacterForce: $("input[name='CharacterForce']:checked").val(),
                    ScaleChoice: $("input[name='ScaleChoice']:checked").val(),
                    IconType: iconType || ''
                };
                
                $.ajax({
                    url: "/places/doconfigure2",
                    method: "POST",
                    headers: { "X-Requested-With": "XMLHttpRequest" },
                    data: formData,
                    traditional: true,
                    success: function(response) {
                        resolve({ hasError: false, type: 'basic' });
                    },
                    error: function(xhr, status, error) {
                        var errorMessage = "An error occurred while saving basic info.";
                        if (xhr.responseJSON && xhr.responseJSON.message) {
                            errorMessage = xhr.responseJSON.message;
                        }
                        resolve({ hasError: true, error: errorMessage, type: 'basic' });
                    }
                });
            });
            savePromises.push(basicInfoPromise);
        }
        
        // If no promises were added (shouldn't happen with our logic), add a dummy one
        if (savePromises.length === 0) {
            savePromises.push(Promise.resolve({ hasError: false, type: 'none', message: 'No changes to save' }));
        }
        
        
        console.log("Total promises to execute:", savePromises.length);
        
        // Wait for all saves to complete
        Promise.all(savePromises).then(function(results) {
            console.log("All promises completed, results:", results);
            var hasErrors = results.some(function(result) { return result.hasError; });
            var errors = results.filter(function(result) { return result.hasError; });
            
            console.log("Has errors:", hasErrors, "Error count:", errors.length);
            
            if (hasErrors) {
                // Show all error messages
                errors.forEach(function(error) {
                    alert(error.error);
                });
            } else {
                // Success - reload page to show updated data
                location.reload();
            }
        }).catch(function(error) {
            console.error('Save error:', error);
            alert('An error occurred while saving. Please try again.');
        }).finally(function() {
            // Hide processing modal
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
        var n = $.address.hash(),
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
    $("[data-retry-url]").loadRobloxThumbnails(), r(), $.address.externalChange(r)
});