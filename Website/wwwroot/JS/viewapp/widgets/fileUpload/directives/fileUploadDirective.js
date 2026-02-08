// ~/viewapp/widgets/fileUpload/directives/fileUploadDirective.js
"use strict";
fileUpload.directive("fileUpload", ["$log", "$parse", "fileTypes", "fileWidgetLayout", function(n, t, i, r) {
    return {
        restrict: "A",
        templateUrl: r.template,
        link: function(n, u, f) {
            function c(n, t) {
                return t === i.image ? i.imageMimeTypes[n.type] : !1
            }
            n.fileUpload = {
                name: "",
                isFileInvalid: !1,
                allowedFileTypes: i.allowedFileTypes
            };
            var o = t(f.fileModel)(n) || {},
                s = f.fileName,
                e = u.find(r.selectors.fileInput),
                h = u.find(r.selectors.fileButton);
            h.click(function() {
                e.trigger("click")
            }), e.bind("change", function(t) {
                var r = t.target.files,
                    u = r[0];
                c(u, i.image) ? (n.fileUpload.isFileInvalid = !1, o[s] = r[0], n.fileUpload.name = r[0].name) : (n.fileUpload.isFileInvalid = !0, o[s] = null, n.fileUpload.name = "", e.val("")), n.$apply()
            })
        }
    }
}]);