import { createContainer } from "unstated-next";
import { useRef, useState } from "react";

const FavouriteButtonStore = createContainer(() => {
    const [isFavorited, setIsFavorited] = useState(null);
    const [favoriteCount, setFavoriteCount] = useState(0);
    const [locked, setLocked] = useState(false);
    
    const [iconHovered, setIconHovered] = useState(false);
    const mouseEnter = () => setIconHovered(true);
    const mouseLeave = () => setIconHovered(false);
    const deb = useRef(false);
    const lastOpt = useRef({
        assetId: 0,
        initFavCount: -1,
    });
    
    return {
        isFavorited,
        setIsFavorited,
        favoriteCount,
        setFavoriteCount,
        locked,
        setLocked,
        iconHovered,
        setIconHovered,
        mouseEnter,
        mouseLeave,
        deb,
        lastOpt,
    }
});

export default FavouriteButtonStore;