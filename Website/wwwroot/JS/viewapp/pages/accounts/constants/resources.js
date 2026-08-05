// ~/viewapp/pages/accounts/constants/resources.js
"use strict";
accounts.constant("accountConstantsResources", {
    controllers: {
        modal: {
            emailSet: "accountSettingsModalEmailSetController",
            emailVerify: "accountSettingsModalEmailVerifyController",
            passwordSet: "accountSettingsModalPasswordSetController",
            phoneSet: "accountSettingsModalPhoneSetController",
            pinCreate: "accountSettingsModalPinCreateController",
            pinUnlock: "accountSettingsModalPinUnlockController",
            usernameUpdate: "accountSettingsModalUsernameUpdateController"
        }
    },
    templates: {
        accountInfo: "account-info.html",
        accountBilling: "account-billing.html",
        accountNotifications: "account-notifications.html",
        accountPrivacy: "account-privacy.html",
        accountSecurity: "account-security.html",
        accountSocial: "account-social.html",
        accountPinStatus: "account-pin-status.html",
        modal: {
            emailSet: "modal-email-set.html",
            emailVerify: "modal-email-verify.html",
            passwordSet: "modal-password-set.html",
            phoneSet: "modal-phone-set.html",
            pinCreate: "modal-pin-create.html",
            pinUnlock: "modal-pin-unlock.html",
            usernameUpdate: "modal-username-update.html"
        }
    }
});