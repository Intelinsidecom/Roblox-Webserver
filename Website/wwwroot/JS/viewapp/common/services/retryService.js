"use strict";

robloxAppService.factory("retryService", ["$log", function() {
        function t() {
            return Roblox&&Roblox.Utilities&&Roblox.Utilities.ExponentialBackoff&&Roblox.Utilities.ExponentialBackoffSpecification? !0: !1
        }

        return {
            isExponentialBackOffEnabled:t(), exponentialBackOff:function() {
                if(t()) {
                    var n=new Roblox.Utilities.ExponentialBackoffSpecification({
                        firstAttemptDelay:2e3, firstAttemptRandomnessFactor:3, subsequentDelayBase:1e4, subsequentDelayRandomnessFactor:.5, maximumDelayBase:3e5
                    }); return new Roblox.Utilities.ExponentialBackoff(n)
            }

            return null
        }
    }
}

]);