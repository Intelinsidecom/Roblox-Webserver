typeof Roblox=="undefined" &&(Roblox= {}),
typeof Roblox.Utilities=="undefined" &&(Roblox.Utilities= {}),
Roblox.Utilities.ExponentialBackoffSpecification=function(n) {
    n=n|| {}

    ;
    var t=typeof n.firstAttemptDelay=="number" ?n.firstAttemptDelay:5e3,
    i=typeof n.firstAttemptRandomnessFactor=="number" ?n.firstAttemptRandomnessFactor:.5,
    r=typeof n.subsequentDelayBase=="number" ?n.subsequentDelayBase:t*2,
    u=typeof n.subsequentDelayRandomnessFactor=="number" ?n.subsequentDelayRandomnessFactor:i,
    f=typeof n.maximumDelayBase=="number" ?n.maximumDelayBase:3e5;

    this.FirstAttemptDelay=function() {
        return t
    }

    ,
    this.FirstAttemptRandomnessFactor=function() {
        return i
    }

    ,
    this.SubsequentDelayBase=function() {
        return r
    }

    ,
    this.SubsequentDelayRandomnessFactor=function() {
        return u
    }

    ,
    this.MaximumDelayBase=function() {
        return f
    }
}

;