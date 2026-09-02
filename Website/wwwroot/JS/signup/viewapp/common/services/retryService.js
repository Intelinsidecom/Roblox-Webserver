"use strict";

freebloxiaAppService.factory("retryService", ["$log", function() {
        function t() {
            return Freebloxia&&Freebloxia.Utilities&&Freebloxia.Utilities.ExponentialBackoff&&Freebloxia.Utilities.ExponentialBackoffSpecification? !0: !1
        }

        return {
            isExponentialBackOffEnabled:t(), exponentialBackOff:function() {
                if(t()) {
                    var n=new Freebloxia.Utilities.ExponentialBackoffSpecification({
                        firstAttemptDelay:2e3, firstAttemptRandomnessFactor:3, subsequentDelayBase:1e4, subsequentDelayRandomnessFactor:.5, maximumDelayBase:3e5
                    }); return new Freebloxia.Utilities.ExponentialBackoff(n)
            }

            return null
        }
    }
}

]);