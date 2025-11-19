;// bundle: Widgets___GroupImage___19715bdc4466eb0d9b88eeddc2a4e78f_m
;// files: modules/Widgets/GroupImage.js

;// modules/Widgets/GroupImage.js
Roblox.define("Widgets.GroupImage",[],function(){function i(n){var t=$(n);return{imageSize:t.attr("data-image-size")||"medium",noClick:typeof t.attr("data-no-click")!="undefined",groupId:t.attr("data-group-id")||0}}function t(r,u){for($.type(r)!=="array"&&(r=[r]);r.length>0;){for(var e=r.splice(0,10),o=[],f=0;f<e.length;f++)o.push(i(e[f]));$.getJSON(n.endpoint,{params:JSON.stringify(o)},function(n,i){return function(r){for(var a=[],f,l,s,e=0;e<r.length;e++)if(f=r[e],f!=null){var h=n[e],o=$(h),c=$("<div>").css("position","relative");o.html(c),o=c,i[e].noClick||(l=$("<a>").attr("href",f.url),o.append(l),o=l),s=$("<img>").attr("title",f.name).attr("alt",f.name).attr("border",0),s.load(function(n,t){return function(){n.width(t.width),n.height(t.height)}}(c,h,s,f)),o.append(s),s.attr("src",f.thumbnailUrl),f.thumbnailFinal||a.push(h)}u=u||1,u<4&&window.setTimeout(function(){t(a,u+1)},u*2e3)}}(e,o))}}function r(){t($(n.selector+":empty").toArray())}var n={selector:".roblox-group-image",endpoint:"/group-thumbnails?jsoncallback=?"};return{config:n,load:t,populate:r}});

;//Bundle detector
Roblox && Roblox.BundleDetector && Roblox.BundleDetector.bundleDetected('Widgets___GroupImage');