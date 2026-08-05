// ~/viewapp/pages/accounts/constants/modalConstants.js
"use strict";
accounts.constant("modalConstants", {
    "default": {
        positiveBtnText: "Yes",
        negativeBtnText: "No",
        submitButtonText: "OK",
        success: {
            title: "Success",
            body: "Saved Successfully!"
        },
        error: {
            title: "Error",
            body: "Something went wrong, please try again later."
        },
        errorMessage: "Error occurred"
    },
    secureSignOut: {
        success: {
            body: "You have been signed out of all other sessions."
        },
        error: {
            body: "There was an error signing you out of all other sessions, please try again later."
        }
    },
    disconnectXbox: {
        error: {
            body: "There was an error disconnecting your Xbox account, please try again later."
        }
    },
    updateNotificationSettings: {
        error: {
            body: "There was an error updating your notification settings, please try again later."
        }
    },
    twoStepVerification: {
        success: {
            title: "2 Step Verification Enabled",
            titleIcon: "icon-like",
            body: "Your account is now protected! No further action is required at this time. A security code will be sent next time you login from a new device."
        },
        disabling: {
            title: "Warning",
            body: "If you turn off 2-Step Verification, only your password will be needed when you login from a new device. Are you sure?"
        }
    },
    birthdayChange: {
        "default": {
            title: "Warning",
            body: "Changing your birthday to under age 13 cannot be un-done. Are you sure you want to continue?"
        },
        withSocialSignOn: {
            body: "Changing your birthday to under age 13 cannot be un-done. Your Social Sign On from Facebook will be disabled and you will need to sign on using your Roblox password.",
            positiveBtnText: "Ok",
            negativeBtnText: "Cancel"
        },
        needPassword: {
            title: "Must Add Password",
            body: "You must add a password to your Roblox account to change your birthday."
        }
    },
    emailSet: {
        emailRequiredMessage: "Email Required",
        emailSetSuccessMessage: "Email Address Changed",
        invalidEmailAddressMessage: "Invalid Email Address",
        addActionLabel: "Add",
        modifyActionLabel: "Change",
        over13Label: "My",
        under13Label: "Parent's",
        emailLabel: "Email"
    },
    phoneSet: {
        addActionLabel: "Add",
        editActionLabel: "Edit",
        updateSuccessMessage: "Phone has been successfully updated!",
        deleteSuccessMessage: "Phone has been removed",
        countryListErrorMessage: "Error loading country list",
        smsImageUrl: "/images/TwoStepVerification/sheild-sms.png"
    },
    passwordSet: {
        changePasswordMessage: "Change Password",
        addPasswordMessage: "Add Password",
        matchErrorMessage: "Passwords do not match"
    },
    pinCreate: {
        matchErrorMessage: "PINs do not match"
    }
});