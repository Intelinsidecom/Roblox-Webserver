"use strict";

robloxFilters.filter("capitalize", function() {
        return function(n) {
            var i, r, t, u; if(n !=null) {
                for(i=n.split(" "), r=[], t=0; t<i.length; t++)u=i[t].toLowerCase(), r.push(u.substring(0, 1).toUpperCase()+u.substring(1)); return r.join(" ")
            }
        }
    });