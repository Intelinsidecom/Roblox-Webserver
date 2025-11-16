import {useState, useEffect} from 'react';

function useWindowQuery(query) {
    const [matches, setMatches] = useState(false);
    
    useEffect(() => {
        if (typeof window === 'undefined') return;
        
        const media = window.matchMedia(`(${query})`);
        setMatches(media.matches);
        
        const listener = () => setMatches(media.matches);
        media.addEventListener('change', listener);
        return () => media.removeEventListener('change', listener);
    }, [query]);
    
    return matches;
}

export default useWindowQuery;
