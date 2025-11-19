// ~/viewapp/widgets/modal/constants/modalOptions.js
"use strict";
modal.constant("modalOptions", {
    params: {
        titleText: "",
        titleIcon: "",
        bodyText: "",
        footerText: "",
        imageUrl: "",
        actionButtonShow: !1,
        actionButtonClass: "btn-secondary-md",
        actionButtonId: "modal-action-button",
        neutralButtonShow: !0,
        neutralButtonClass: "btn-control-md",
        closeButtonShow: !0,
        cssClass: "modal-window"
    },
    defaults: {
        keyboard: !0,
        animation: !1
    },
    commonTemplateUrl: "cc-modal-template",
    commonController: "modalController"
});