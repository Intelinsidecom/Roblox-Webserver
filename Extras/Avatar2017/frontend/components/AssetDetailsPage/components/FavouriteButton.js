import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../../lib/numberUtils";
import { createFavorite, deleteFavorite, getIsFavorited } from "../../../services/catalog";
import authentication from "../../../stores/authentication";
import { wait } from "../../../lib/utils";
import FavouriteButtonStore from "../stores/FavouriteButtonStore";

const useStyles = createUseStyles({
    wrapper: {},
    favoriteStar: {
        display: 'inline-block',
        width: '68px',
        height: '16px',
        textAlign: 'center',
        cursor: 'pointer',
        whiteSpace: 'nowrap',
        background: 'url("/img/FavoriteStar.png")',
        marginBottom: '-2px',
    },
    favoriteCount: {
        textAlign: 'center',
    },
    favoriteContainer: {
        // width: '68px',
        width: "fit-content",
        textAlign: 'center',
        cursor: 'pointer',
    },
    favoriteButton: {
        cursor: 'pointer',
        textDecoration: 'none',
        outline: 0,
        background: 'transparent',
        display: "flex",
        width: "fit-content",
    },
    favoriteIcon: {
        marginBottom: '4px',
        backgroundPosition: '0 -84px',
        backgroundImage: 'url(/img/branded.svg)',
        backgroundRepeat: 'no-repeat',
        backgroundSize: 'auto auto',
        width: '28px',
        height: '28px',
        display: 'inline-block',
        verticalAlign: 'middle',
    },
    favoriteIconSelected: {
        backgroundPosition: '-28px -84px'
    },
    favoriteLabel: {
        fontSize: 16,
        fontWeight: 400,
        padding: "4px 0 0 6px",
        color: "#F6B702",
    },
});

/**
 * @param {number} assetId
 * @param {number} initFavCount
 * @param {string|undefined|null} [id]
 * @returns {JSX.Element}
 * @constructor
 */
const FavouriteButton = ({ assetId, initFavCount, id }) => {
    const auth = authentication.useContainer();
    const s = useStyles();
    
    const store = FavouriteButtonStore.useContainer();
    const {
        isFavorited,
        setIsFavorited,
        favoriteCount,
        setFavoriteCount,
        locked,
        setLocked,
        iconHovered,
        mouseEnter,
        mouseLeave,
        deb,
        lastOpt,
    } = store;
    const buttonClass = (isFavorited || iconHovered) ? s.favoriteIconSelected : '';
    
    // should be rewritten this is garbage
    useEffect(() => {
        if (
            deb.current ||
            lastOpt.current.assetId === assetId ||
            lastOpt.current.initFavCount === initFavCount
        ) return;
        deb.current = true;
        lastOpt.current = {
            assetId,
            initFavCount,
        };
        setIsFavorited(null);
        setFavoriteCount(initFavCount);
        setLocked(false);
        
        if (auth.userId) {
            getIsFavorited({ assetId, userId: auth.userId }).then(data => {
                setIsFavorited(!!data);
            }).catch(() => {
                // undefined/null response causes axios to incorrectly return network error :)
                setIsFavorited(false);
            })
        }
        setTimeout(() => deb.current = false, 1000);
    }, [initFavCount, assetId, auth.userId]);
    
    return <div className={s.favoriteContainer} id={!id ? undefined : id}>
        <a className={`${s.favoriteButton}`} href="#" onClick={e => {
            e.preventDefault();
            if (locked) return;
            setLocked(true);
            setIsFavorited(!isFavorited);
            setFavoriteCount(isFavorited ? favoriteCount - 1 : favoriteCount + 1);
            if (isFavorited) {
                deleteFavorite({ userId: auth.userId, assetId }).finally(() => {
                    setLocked(false);
                })
            } else {
                createFavorite({ userId: auth.userId, assetId }).finally(() => {
                    setLocked(false);
                })
            }
        }} onMouseEnter={mouseEnter} onMouseLeave={mouseLeave}>
            <div>
                <span className={`${s.favoriteIcon} ${buttonClass}`}/>
            </div>
            <div className={s.favoriteLabel}>{abbreviateNumber(favoriteCount)}</div>
        </a>
    </div>
}

export default FavouriteButton;