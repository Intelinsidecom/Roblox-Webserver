import {createUseStyles} from "react-jss";
import Link from "../../link";
import React, { useEffect, useState } from "react";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import { AssetType } from "../../../models/enums";
import { getGamePassRootPlace, getGameUrl } from "../../../services/games";
import { getBadgeInfoByID } from "../../../services/badges";
import GameIconImage from "../../gameIconImage";

const useStyles = createUseStyles({
    relatedAssetContainer: {
        backgroundColor: "rgba(25,25,25,0.75)",
        position: "absolute",
        right: 0,
        bottom: 0,
        left: 0,
        color: "#fff",
        display: "flex",
        justifyContent: "space-between",
    },
    assetName: {
        maxWidth: "calc(100% - 50px - 18px)",
        margin: "8px 0 0 6px",
        textAlign: "left",
        fontSize: 16,
        "& span, & a": {
            fontSize: 12,
            fontWeight: 500,
            lineHeight: 1.5,
            wordWrap: "break-word",
            hyphens: "none",
        },
        "& span": {
            color: "#fff",
            fontWeight: 400,
        },
    },
    assetIcon: {
        margin: 6,
        "& a": {
            display: "flex",
        },
        "& img": {
            height: "50px!important",
            width: "50px!important",
            verticalAlign: "middle",
            padding: 0,
        },
    },
});

function RelatedGame() {
    const s = useStyles();
    const store = AssetDetailsStore.useContainer();
    const [placeId, setPlaceId] = useState(0);
    const [placeName, setPlaceName] = useState("");
    const [gameUrl, setGameUrl] = useState("#");
    
    useEffect(async () => {
        switch (store.details.assetType) {
            case AssetType.GamePass:
                let universeInfo = await getGamePassRootPlace({ assetId: store.details.id });
                if (!universeInfo) return;
                setPlaceId(universeInfo.id);
                setPlaceName(universeInfo.name);
                setGameUrl(getGameUrl({ placeId: universeInfo.id, name: universeInfo.name }));
                break;
            case AssetType.Badge:
                let badgeInfo = await getBadgeInfoByID({ badgeId: store.details.id });
                setPlaceId(badgeInfo.awardingUniverse.rootPlaceId);
                setPlaceName(badgeInfo.awardingUniverse.name);
                setGameUrl(getGameUrl({ placeId: badgeInfo.awardingUniverse.rootPlaceId, name: badgeInfo.awardingUniverse.name }));
                break;
        }
    }, [store.details, store.details?.id]);
    
    if (!store.details || !placeId || placeId === 0) { console.log(`dd ${placeId} ff ${placeName}`); return null; }
    
    return <div className={s.relatedAssetContainer}>
        <div className={s.assetName}>
            <span>{store.details.assetType === AssetType.GamePass ? "Use this Game Pass" : "Earn this Badge"} in: <Link href={gameUrl}>
                <a className={`link2018`} href={gameUrl}>
                    {placeName}
                </a>
            </Link></span>
        </div>
        <div className={s.assetIcon}>
            <Link href={gameUrl}>
                <a href={gameUrl}>
                    <GameIconImage name={placeName} id={placeId}/>
                </a>
            </Link>
        </div>
    </div>
}

export default RelatedGame;
