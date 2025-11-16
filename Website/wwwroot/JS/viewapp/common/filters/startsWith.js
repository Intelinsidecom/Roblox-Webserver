"use strict";

robloxFilters.filter("startsWith", function() {
        return function(n, t, i) {
            var u=[], r, f; if(n)for(r=0; r<n.length; r++)f=i?n[r][i].toLowerCase():n[r].toLowerCase(), f.indexOf(t.toLowerCase())===0&&t.length<f.length&&u.push(n[r]); return u.length===0&&(u=n), u
        }
    });