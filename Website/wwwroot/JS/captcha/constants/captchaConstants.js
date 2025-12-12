// captcha/constants/captchaConstants.js
"use strict";
var Roblox = Roblox || {};
Roblox.CaptchaConstants = Roblox.CaptchaConstants || {
    endpoints: {
        sendMessage: "",
        addFriend: "",
        follow: "",
        signup: "",
        joinGroup: "",
        login: "",
        postComment: ""
    },
    serviceData: {
        sitekey: "",
        successSuffix: "Captcha_Success",
        failSuffix: "Captcha_Failed",
        displayedSuffix: "Captcha_Displayed",
        captchaSolvedPrefix: "Captcha_User_Solved_InSeconds_",
        captchaSolveTimeIntervals: [{
            seconds: 1,
            suffix: "Less_Than_1"
        }, {
            seconds: 3,
            suffix: "1_To_3"
        }, {
            seconds: 10,
            suffix: "4_To_10"
        }, {
            seconds: 20,
            suffix: "11_To_20"
        }, {
            seconds: 30,
            suffix: "21_To_30"
        }, {
            seconds: 40,
            suffix: "31_To_40"
        }, {
            seconds: 50,
            suffix: "41_To_50"
        }],
        captchaSolveTimeLarge: "Greater_Than_50"
    },
    types: {
        signup: "signup",
        sendMessage: "sendMessage",
        addFriend: "addFriend",
        follow: "follow",
        joinGroup: "joinGroup",
        login: "login",
        postComment: "postComment"
    },
    messages: {
        error: "We currently cannot verify CAPTCHA, please try again later."
    }
};