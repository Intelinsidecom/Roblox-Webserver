import {createUseStyles} from "react-jss";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import { useEffect, useState } from "react";
import { getRecommendations } from "../../../services/catalog";
import Link from "../../link";
import { getUrlForAssetType } from "../../sharedAssetPage";
import ItemImage from "../../itemImage";
import { CreatorType } from "../../../models/enums";
import CreatorLink from "../../creatorLink";
import { abbreviateNumber } from "../../../lib/numberUtils";
import { wait } from "../../../lib/utils";

const useEntryStyles = createUseStyles({
    recomCardContainer: {
        // borderRadius: 3,
        padding: 0,
        flex: "0 0 calc(14.285714285% - 9px)",
        boxShadow: "none",
        minWidth: 85,
        "& *": {
            textAlign: "left",
        },
        "&:hover": {
            boxShadow: "0 1px 4px 0 rgba(25,25,25,.3)!important",
        },
        "@media(max-width: 970px)": {
            flex: "0 0 calc(16.66667% - 8px)",
        },
        "@media(max-width: 767px)": {
            flex: "0 0 calc(20% - 8px)",
        },
        "@media(max-width: 576px)": {
            flex: "0 0 calc(33.3333% - 6px)",
        },
    },
    thumbContainer: {
        borderTopLeftRadius: 3,
        borderTopRightRadius: 3,
        width: "100%",
        aspectRatio: "1 / 1",
        minWidth: 85,
        minHeight: 85,
        position: "relative",
        borderBottom: "1px solid #e3e3e3",
        "& img": {
            padding: 0,
            borderTopLeftRadius: 3,
            borderTopRightRadius: 3,
            width: "100%",
            height: "100%",
            minWidth: 85,
            minHeight: 85,
        },
    },
    text: {
        padding: "0 5px",
    },
    nameContainer: {
        height: 45,
        overflow: "hidden",
        whiteSpace: "normal",
        width: "100%",
        minWidth: 85,
        fontWeight: 500,
    },
    creatorContainer: {
        "& *": {
            fontSize: 10,
            fontWeight: 500,
        },
    },
    
    currencyIcon: {
        marginTop: 3,
        marginRight: 3,
    },
    currencyText: { fontWeight: 500 },
})

/**
 * @param {RecommendedItemEntry} recom
 * @param {number} assetType
 * @constructor
 */
function AssetRecommendationEntry({ recom, assetType }) {
    const s = useEntryStyles();
    const href = getUrlForAssetType({
        assetId: recom.item.assetId,
        assetTypeId: assetType,
        name: recom.item.name,
    });
    
    return <div className={`section-content hoverShadow flex flex-column ${s.recomCardContainer}`}>
        <div className="flex flex-column" style={{ paddingBottom: 5, }}>
            <Link href={href}>
                <a href={href} className="flex flex-column">
                    <div className={s.thumbContainer}>
                        <ItemImage name={recom.item.name} id={recom.item.assetId} />
                    </div>
                    <span className={`${s.nameContainer} ${s.text}`}>{recom.item.name}</span>
                </a>
            </Link>
            <div className={`${s.creatorContainer} ${s.text} flex w-fit-content`}>
                <span style={{ marginRight: 3, color: "var(--text-color-secondary)" }}>By</span>
                <CreatorLink id={recom.creator.creatorId} type={recom.creator.creatorType} name={recom.creator.name}/>
            </div>
            <div className={`${s.text} flex w-fit-content`}>
                {
                    recom.product.priceInRobux
                    ?
                    <>
                        <span className={`${s.currencyIcon} icon-robux-16x16`} />
                        <span className={`${s.currencyText} text-robux`}>{abbreviateNumber(recom.product.priceInRobux)}</span>
                    </>
                    :
                    recom.product.priceInTickets
                    ?
                    <>
                        <span className={`${s.currencyIcon} icon-tix-16x16`} />
                        <span className={`${s.currencyText} text-tix`}>{abbreviateNumber(recom.product.priceInTickets)}</span>
                    </>
                    :
                    <span className={`${s.currencyText} text-robux`}>{recom.product.noPriceText || "Free"}</span>
                }
            </div>
        </div>
    </div>
}

const useStyles = createUseStyles({
    recommendationsContainer: {
        display: "flex",
        flexDirection: "column",
        gap: 6,
        marginTop: 3,
        "@media(max-width: 767px)": {
            padding: "0 6px",
        },
    },
    recommendationsHeaderContainer: {
        "& h3": {
            fontSize: 20,
            fontWeight: 700,
            paddingBottom: 5,
        },
    },
    recommendationsList: {
        display: "flex",
        flexWrap: "nowrap",
        overflowX: "auto",
        padding: 3,
        gap: 10,
        scrollbarWidth: "thin",
        "-ms-overflow-style": "thin",
    },
});

/**
 * @returns {JSX.Element}
 * @constructor
 */
function AssetRecommendations() {
    const s = useStyles();
    const store = AssetDetailsStore.useContainer();
    const { details } = store;
    
    const [recommendations, setRecommendations] = useState([]);
    const [loadingRecoms, setLoadingRecoms] = useState(false);
    
    useEffect(async () => {
        if (loadingRecoms) return;
        setLoadingRecoms(true);
        const recommend = await getRecommendations({ assetId: details.id, assetTypeId: details.assetType, limit: 10 });
        if (!recommend || !Array.isArray(recommend?.data)) {
            setRecommendations(null);
            return;
        }
        setRecommendations(recommend.data);
        await wait(0.25);
        setLoadingRecoms(false);
    }, [details.id]);
    
    return <div className={s.recommendationsContainer}>
        <div className={`flex ${s.recommendationsHeaderContainer}`}>
            <h3 style={{ margin: 0, }}>Recommended Items</h3>
        </div>
        <div className={s.recommendationsList}>
            {
                !recommendations ? // Failed to load recommendations
                <div className="section-content-off w-100">
                    Could not fetch recommendations for this item.
                </div>
                                 :
                recommendations.length > 0
                ?
                recommendations.map(recommendation => <AssetRecommendationEntry recom={recommendation}
                                                                                assetType={details.assetType}/>)
                : // Loading
                loadingRecoms
                ?
                <span className="spinner" style={{ backgroundSize: "auto 36px" }}/>
                :
                <span className="section-content-off w-100">No recommendations for this item.</span>
            }
        </div>
    </div>
}

export default AssetRecommendations;

/**
 * @param {number} creatorId
 * @param {number} creatorType
 * @returns string
 */
export const getCreatorUrl = (creatorId, creatorType) =>
    creatorType === CreatorType.Group ? `/users/${creatorId}/profile` : `/My/Groups.aspx?gid=${creatorId}`;
