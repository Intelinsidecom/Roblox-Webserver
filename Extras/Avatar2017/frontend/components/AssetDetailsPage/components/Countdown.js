import React, { useEffect, useState } from "react";

/**
 * @param {string} timestamp
 * @param {string} className
 * @returns {JSX.Element}
 * @constructor
 */
function Countdown({ timestamp, className }) {
    const [timeStr, setTimeStr] = useState(getTimeStr());
    
    useEffect(() => {
        const interval = setInterval(() => {
            setTimeStr(getTimeStr());
        }, 1000);
        return () => clearInterval(interval);
    }, [timestamp]);
    
    function getTimeStr() {
        const now = new Date();
        const timestampDate = new Date(timestamp);
        const diff = Math.max(0, timestampDate - now);
        
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);
        
        return { hours, minutes, seconds };
    }
    
    return <span className={className}>
        <span>{timeStr.hours}</span>
        h
        <span>{timeStr.minutes}</span>
        m
        <span>{timeStr.seconds}</span>
        s
    </span>
}

export default Countdown;
