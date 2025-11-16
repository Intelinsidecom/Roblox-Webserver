Roblox=Roblox|| {}
 
  ,
  Roblox.Avatar=function() {

    function f() {
        (i=$("#header"), r=10, t=$("#wrap"), n=$(".right-wrapper-placeholder"), n.length !==0)&&($(window).scroll(u), u())
    }

    function u() {
        var u=n[0].getBoundingClientRect().top,
        f=n.is(":visible"),
        e=i.height(),
        o=f&&u-e-r<0;
        o?t.addClass("pinned"): t.removeClass("pinned")
    }

     var i,
     r,
     t,
     n;
 
     $(function() {
             f()
         })
  }
 
 ();
 
 // Avatar customizer functionality
 let currentScales = {
     height: 1,
     width: 1,
     head: 1,
     bodytype: 0,
     proportions: 0
 };

 // Update scale display
 function updateScale(scaleType, value) {
     currentScales[scaleType] = parseFloat(value);
     const percentage = Math.round(value * 100);
     document.getElementById(scaleType + '-percentage').textContent = percentage + '%';
 }

 // Apply scale changes (this would connect to your avatar system)
 function applyScale(scaleType, value) {
     console.log('Applying scale:', scaleType, value);
     // Here you would integrate with your existing avatar scaling system
     // This could call the existing avatarScaleController functions
 }

 // Show spinner over the main avatar thumbnail using the same logic as the 3D
 // thumbnail path (Roblox.ThumbnailSpinner + .avatar-thumbnail .thumbnail-span).
 function showAvatarSpinner() {
    try {
        if (window.Roblox && Roblox.ThumbnailSpinner && window.$) {
            // Attach the spinner to the stable avatar-thumbnail container so it
            // persists across thumbnail-holder reloads.
            var container = $('.avatar-thumbnail');
            if (container.length) {
                Roblox.ThumbnailSpinner.show(container);
            }
        }
    } catch (e) {
        // no-op; fallback is just a blank background while loading
    }
 }

 function hideAvatarSpinner() {
    try {
        if (window.Roblox && Roblox.ThumbnailSpinner && window.$) {
            var container = $('.avatar-thumbnail');
            if (container.length) {
                Roblox.ThumbnailSpinner.hide(container);
            }
        }
    } catch (e) {
        // no-op; if spinner cannot be hidden we just leave it as-is
    }
 }

 function wireAvatarImageLoadHandler() {
     try {
         var container = document.querySelector('.avatar-thumbnail .thumbnail-span');
         if (!container) {
             return;
         }

         var attachHandler = function(img) {
             if (!img) return;
             // If the browser already has the image cached and it is
             // complete, hide the spinner immediately; otherwise wait
             // for the load event.
             if (img.complete && img.naturalWidth > 0) {
                 hideAvatarSpinner();
             } else {
                 img.addEventListener('load', function onLoad() {
                     img.removeEventListener('load', onLoad);
                     hideAvatarSpinner();
                 });
             }
         };

         // Attach for any image that might already be present when this runs.
         var existingImg = container.querySelector('img');
         if (existingImg) {
             attachHandler(existingImg);
         }

         // Observe future changes to the thumbnail span so whenever Roblox.ThumbnailView
         // replaces the <img> we still capture the new load completion.
         if (window.MutationObserver) {
             var observer = new MutationObserver(function(mutations) {
                 mutations.forEach(function(mutation) {
                     mutation.addedNodes && Array.prototype.forEach.call(mutation.addedNodes, function(node) {
                         if (node.tagName && node.tagName.toLowerCase() === 'img') {
                             attachHandler(node);
                         }
                     });
                 });
             });

             observer.observe(container, { childList: true, subtree: true });
         }
     } catch (e) {
         // fail-safe: spinner may stay visible but we avoid breaking the page
     }
 }

 // Refetch avatar
 function refetchAvatar() {
     console.log('Refetching avatar...');
     try {
         // Aggressively clear out any existing 2D/3D content in all avatar
         // thumbnail spans so the current image disappears immediately.
         var nodes = document.querySelectorAll('.thumbnail-holder .thumbnail-span img, .thumbnail-holder .thumbnail-span canvas');
         for (var i = 0; i < nodes.length; i++) {
             var node = nodes[i];
             if (node && node.parentNode) {
                 node.parentNode.removeChild(node);
             }
         }
     } catch (e) {}
     showAvatarSpinner();
     if (typeof Roblox !== 'undefined' && Roblox.ThumbnailView && typeof Roblox.ThumbnailView.reloadThumbnail === 'function') {
         Roblox.ThumbnailView.reloadThumbnail();
     }
 }

 // Redraw avatar
 function redrawAvatar() {
     console.log('Redrawing avatar...');
     showAvatarSpinner();
     try {
         if (window.angular) {
             var scopeEl = angular.element(document.querySelector('.avatar-right-panel'));
             var scope = scopeEl && scopeEl.scope && scopeEl.scope();
             if (scope && scope.redrawThumbnail) {
                 scope.redrawThumbnail();
                 return;
             }
         }
     } catch (e) {}
     // Fallback: just trigger a basic thumbnail reload
     if (typeof Roblox !== 'undefined' && Roblox.ThumbnailView && typeof Roblox.ThumbnailView.reloadThumbnail === 'function') {
         Roblox.ThumbnailView.reloadThumbnail();
     }
 }

 // Initialize sliders with proper styling classes based on their values
 function initializeSliders() {
     const sliders = document.querySelectorAll('input[type="range"]');
     sliders.forEach(slider => {
         updateSliderBackground(slider);
         slider.addEventListener('input', function() {
             updateSliderBackground(this);
         });
     });
 }

 // Update slider background based on value (for visual feedback)
 function updateSliderBackground(slider) {
     const value = slider.value;
     const min = slider.min || 0;
     const max = slider.max || 1;
     const percentage = ((value - min) / (max - min)) * 100;
     
     // Remove existing progress classes
     slider.className = slider.className.replace(/\bpr\d+\b/g, '');
     
     // Add new progress class
     const progressClass = 'pr' + Math.round(percentage);
     slider.classList.add(progressClass);
 }

 // Initialize when page loads
 document.addEventListener('DOMContentLoaded', function() {
     initializeSliders();
     
     // Ensure spinner is cleared as soon as the 2D avatar image actually
     // finishes loading (including subsequent redraws).
     wireAvatarImageLoadHandler();
     
     // Initialize scale percentages
     Object.keys(currentScales).forEach(scaleType => {
         const slider = document.querySelector(`input[oninput*="${scaleType}"]`);
         if (slider) {
             updateScale(scaleType, slider.value);
         }
     });

     // Ensure avatar thumbnail is loaded automatically on first page load
     try {
         if (typeof refetchAvatar === 'function') {
             refetchAvatar();
         }
     } catch (e) {
         // Fail silently if thumbnail helper is unavailable; manual redraw button still works
     }
 });

 // Angular templates drive the right panel (avatar-editor-tabs, avatar-items-list)
 // No custom tab JS needed here; existing Angular controllers handle interactions.

 // Debug script loading and ensure our implementations are used
 $(document).ready(function() {
     console.log('Page ready - checking 3D implementations');
     console.log('THREE.OrbitControls available:', typeof THREE.OrbitControls !== 'undefined');
     console.log('THREE.SimpleOBJMTLLoader available:', typeof THREE.SimpleOBJMTLLoader !== 'undefined');
     console.log('THREE.OBJMTLLoader available:', typeof THREE.OBJMTLLoader !== 'undefined');
     
     // Ensure SimpleOBJMTLLoader is available as OBJMTLLoader
     if (typeof THREE.SimpleOBJMTLLoader !== 'undefined' && typeof THREE.OBJMTLLoader === 'undefined') {
         THREE.OBJMTLLoader = THREE.SimpleOBJMTLLoader;
         console.log('Set THREE.OBJMTLLoader to SimpleOBJMTLLoader');
     }
 });