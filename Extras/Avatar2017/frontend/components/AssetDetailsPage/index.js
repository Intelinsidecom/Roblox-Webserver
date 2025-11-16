import { createUseStyles } from "react-jss";
import AssetDetailsStore from "./stores/AssetDetailsStore";
import Feedback from "../../stores/feedback";
import React, { useEffect, useRef, useState } from "react";
import UserAdvertisement from "../userAdvertisement";
import { AssetType, UserAdvertisementType } from "../../models/enums";
import ItemImage from "../itemImage";
import Link from "../link";
import CreatorLink from "../creatorLink";
import useButtonStyles from "../../styles/buttonStyles";
import ActionButton from "../actionButton";
import { genresList } from "../develop/components/library/genreFilter";
import AssetDetailsModalStore from "./stores/AssetDetailsModalStore";
import AssetRecommendations from "./components/AssetRecommendations";
import Theme2016 from "../theme2016";
import FavouriteButton from "./components/FavouriteButton";
import Countdown from "./components/Countdown";
import AssetDropdown from "./components/Dropdown";
import PriceChart from "./components/PriceChart";
import Resellers from "./components/Resellers";
import BuyModal from "./modals/BuyModal";
import { avPageStyleType, getAvPageStyle } from "../../services/theme";
import { AssetTypeCategory } from "../AvatarEditorPage/stores/avatarInfoStore";
import SellItemModal from "./modals/SellItemModal";
import ConfirmSellModal from "./modals/ConfirmSellModal";
import Authentication from "../../stores/authentication";
import Owners from "./components/Owners";
import RelatedGame from "./components/RelatedGame";
import AudioPlayButton from "../catalogDetailsPage/components/audioPlayButton";
import useWindowQuery from "../windowQuery";
import HorizontalTabs from "../horizontalTabs";
import ReactDOM from "react-dom";
import dayjs from "../../lib/dayjs";
import { wait } from "../../lib/utils";
import FavouriteButtonStore from "./stores/FavouriteButtonStore";
import RemoveItemModal from "./modals/RemoveItemModal";

const useStyles = createUseStyles({
    pageWrapper: {
        display: "flex",
        gap: 15,
        justifyContent: "flex-end",
        "@media(max-width: 1154px)": {
            justifyContent: "center",
        },
    },
    container: {
        width: "calc(100% - 185px)",
        maxWidth: 950,
        "@media(max-width: 1154px)": {
            width: "100%",
        },
    },
    itemContainer: {
        display: "flex",
        flexDirection: "row",
        gap: 12,
        // boxShadow: "none",
        "& *": {
            textAlign: "start",
            fontWeight: 500,
            fontSize: 16,
        },
        "@media(max-width: 767px)": {
            gap: 0,
            paddingBottom: 0,
        },
    },
    
    itemThumbContainer: {
        width: 420,
        "@media(max-width: 970px)": {
            width: 300
        },
        "@media(max-width: 767px)": {
            width: "100%",
            alignItems: "center",
        },
    },
    itemThumb: {
        "@media(max-width: 767px)": {
            width: "420px!important",
        },
        "@media(max-width: 576px)": {
            width: "200px!important",
        },
        "& img": {
            maxWidth: 420,
        },
    },
    itemDetailsContainer: {
        width: "calc(100% - 432px)",
        "@media(max-width: 970px)": {
            width: "calc(100% - 312px)",
        },
        "@media(max-width: 767px)": {
            width: "100%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
        },
    },
    itemHeaderContainer: {
        paddingBottom: 12,
        borderBottom: "1px solid #e3e3e3",
        position: "relative",
        "& h2": {
            fontSize: 32,
            fontWeight: 800,
            lineHeight: "1em",
            padding: "5px 0",
            margin: 0,
            maxHeight: "2.225em",
            overflow: "hidden",
            whiteSpace: "pre-wrap",
            wordBreak: "break-word",
            textOverflow: "ellipsis",
            "@media(max-width: 576px)": {
                fontSize: 24,
            },
        },
    },
    itemHeaderInfo: {
        "& *": {
            "@media(max-width: 576px)": {
                fontSize: "12px!important",
            },
        }
    },
    itemDescription: {
        fontWeight: 300,
        whiteSpace: "pre-wrap",
        wordWrap: "break-word",
        width: "calc(100% - 120px)",
        fontFamily: 'HCo Gotham SSm,Helvetica Neue,Helvetica,Arial,Lucida Grande,sans-serif',
        "@media(max-width: 767px)": {
            width: "100%",
            marginTop: 12,
            fontWeight: 400,
            marginBottom: 6,
        }
    },
    smallImg: {
        width: 420,
        height: 420,
        display: "flex",
        alignItems: "center",
        "& img": {
            width: 150,
            height: 150,
        },
        "@media(max-width: 970px)": {
            width: 300,
            height: 300,
        },
        "@media(max-width: 767px)": {
            width: "100%!important",
        },
    },
    
    img: { padding: 0 },
    
    itemDetails: {},
    
    priceWrapper: {
        display: "flex",
        justifyContent: "space-between",
        width: "calc(100% - 120px)",
        "@media(max-width: 767px)": {
            width: "100%",
            flexDirection: "column",
        },
    },
    priceContainer: {
        "@media(max-width: 767px)": {
            justifyContent: "center",
            marginBottom: 6,
        },
    },
    priceInfo: {
        marginRight: 12,
        "@media(max-width: 767px)": {
            justifyContent: "center",
        },
    },
    editWrapper: {
        display: "flex",
        justifyContent: "space-between",
        width: "180px",
        "@media(max-width: 767px)": {
            width: "100%",
            "& a": {
                width: "100%",
            },
        }
    },
    buyBtn: {
        fontWeight: 500,
        width: 180,
        padding: 15,
        lineHeight: "100%",
        borderRadius: 5,
        fontSize: 21,
        textAlign: "center",
        "@media(max-width: 767px)": {
            width: "100%",
            padding: 9,
        },
    },
    editBtn: {
        fontSize: 18,
        padding: 9,
        borderRadius: 3,
    },
    buyBtnContainer: {
        position: "relative",
    },
    buyBtnContainerr: {
        position: "relative",
        width: "100%",
    },
    priceIcon: { marginRight: 3, },
    priceLabel: {
        lineHeight: "1.16em",
        fontSize: 20,
        fontWeight: 700,
        marginTop: 2,
    },
    
    attrContainer: {
        display: "flex",
        width: '100%',
        marginTop: 12,
    },
    attrLabel: {
        fontSize: 16,
        fontWeight: 500,
        lineHeight: "1.4em",
        color: "var(--text-color-secondary)",
        width: 120,
        paddingRight: 9,
        display: "inline-block",
        "@media(max-width: 767px)": {
            width: "45%",
        },
    },
    attrVal: {
        "@media(max-width: 767px)": {
            width: "55%",
        },
    },
    detailTxt: {
        fontSize: 12,
        fontWeight: 500,
        "@media(max-width: 767px)": {
            textAlign: "center",
            marginTop: 6,
            "&.top": {
                marginTop: 0,
                marginBottom: 6,
            }
        },
    },
    
    restrictionsContainer: {
        position: 'absolute',
        bottom: -3,
        left: -3,
        overflow: 'hidden',
    },
    
    itemInteractionContainer: {
        "@media(max-width: 767px)": {
            padding: "5px 0",
            borderTop: "1px solid #e3e3e3",
            marginTop: 12,
            marginLeft: -12,
            marginRight: -12,
        },
    },
    
    itemStatusContainer: {
        position: 'absolute',
        top: 0,
        right: 0,
        margin: 10,
    },
    itemStatusSale: {
        display: 'inline-block',
        fontSize: 18,
        fontWeight: 500,
        backgroundColor: "#E2231A",
        color: "#fff",
        padding: 6,
        borderRadius: 5,
        lineHeight: "1em",
        marginLeft: 6,
    },
    itemStatusSaleIcon: {
        backgroundPosition: "-20px -720px",
        width: 20,
        height: 20,
        backgroundSize: "40px auto",
        "&:hover": {
            backgroundPosition: "-20px -720px",
        }
    },
    itemStatusSaleNew: {
        fontSize: 15,
        lineHeight: "1em",
        fontWeight: 500,
        marginLeft: 3,
        color: "#fff",
    },
    saleClockContainer: {
        fontSize: 14,
        color: "#191919",
        textAlign: "center",
        fontWeight: 300,
        lineHeight: '1.4em',
        // marginTop: 12,
        position: "absolute",
        width: 180,
    },
    saleClock: {
        fontSize: 14,
        color: "#191919",
        textAlign: "center",
        fontWeight: 300,
        lineHeight: '1.4em',
        "& span": {
            color: "#E2231A",
            marginLeft: 3,
        },
    },
    dropdownContainer: {
        position: "absolute",
        top: 0,
        right: 0,
        display: "flex",
    },
    offsaleLabel: {
        width: "calc(100% - 180px)!important",
        color: "#191919",
        fontWeight: 400,
        lineHeight: "1.4em",
        fontSize: 16,
        paddingRight: 12,
    },
    availableInventoryParent: {
        "@media(max-width: 767px)": {
            flexDirection: "column",
            alignItems: "center",
            marginBottom: -6,
        },
    },
    availableInventoryLabel: {
        width: "calc(100% - 180px)!important",
        color: "#191919",
        fontWeight: 500,
        lineHeight: "1.4em",
        fontSize: 16,
        "@media(max-width: 767px)": {
            width: "fit-content!important",
            marginBottom: 6,
        },
    },
    bannerAdContainer: {
        maxHeight: 90,
        marginBottom: 15,
    },
    skyscraperAdContainer: {
        maxWidth: 160,
        "@media(max-width: 1154px)": {
            display: "none",
        },
    },
    ownedStat: {
        verticalAlign: 'text-bottom',
        display: 'inline-block',
        fontSize: '10px',
        backgroundColor: 'var(--success-color)',
        color: '#fff',
        padding: '3px',
        borderRadius: '50%',
        lineHeight: 1,
        aspectRatio: "1 / 1",
        height: "100%",
        width: "auto",
        marginLeft: 12,
        marginRight: 3,
        maxHeight: 18,
        "@media(max-width: 576px)": {
            marginLeft: 6,
        },
    },
    
    favBtnContainer: {
        marginTop: 6,
        "@media(max-width: 767px)": {
            margin: "0!important",
            justifyContent: "center",
        }
    },
    noPriceContainer: {
        "@media(max-width: 767px)": {
            width: "100%",
            display: "flex",
            alignItems: "center",
            flexDirection: "column",
        },
    },
});

/**
 * @param {AssetDetailsEntry} itemDetails
 * @returns {JSX.Element}
 * @constructor
 */
function AssetDetailsPage({ itemDetails }) {
    const s = useStyles();
    const store = AssetDetailsStore.useContainer();
    const modal = AssetDetailsModalStore.useContainer();
    const auth = Authentication.useContainer();
    const buttonStyles = useButtonStyles();
    const [detailOptions, setDetailOptions] = useState(/** @type DetailOptionEntry[] */([]));
    const [isNew, setNew] = useState(false);
    const deb = useRef(false);
    // 767px or lower
    const isWindow767 = useWindowQuery("max-width: 767px");
    
    useEffect(async () => {
        await store.setDetails(itemDetails);
        await setNew(IsISOWithinDays(itemDetails.createdAt, 3));
        if (!store.details || !store.resellers || deb.current) return;
        let purchaseInfo = store.getPurchaseInfo();
        await setDetailOptions([
            (itemDetails.itemRestrictions.includes("Limited") || itemDetails.itemRestrictions.includes("LimitedUnique")) && !itemDetails.isForSale && store.resellers.length > 0 && (purchaseInfo.sellerId === auth.userId) ? {
                label: "Best Price",
                field: <div className={s.priceWrapper}>
                    <div className={`flex flex-column justify-content-between ${s.priceInfo}`}>
                        <div className={`${s.priceContainer} flex`}>
                            <span className={`icon-robux ${s.priceIcon}`}/>
                            <span className={s.priceLabel}
                                  style={{ color: "var(--robux-color)" }}>{formatNum(purchaseInfo?.price || 0)}</span>
                        </div>
                        <span className={`${s.detailTxt} invis767`}>You are selling this item.</span>
                    </div>
                    <ActionButton className={s.buyBtn} label="Buy" buttonStyle={buttonStyles.newBuyButton}
                                  disabled={true}/>
                    <span className={`${s.detailTxt} invis-767`}>You are selling this item.</span>
                </div>,
                labelClass: `${s.attrLabel} invis767`,
            } : (itemDetails.itemRestrictions.includes("Limited") || itemDetails.itemRestrictions.includes("LimitedUnique")) && !itemDetails.isForSale && store.resellers.length > 0 && (store.resellers.length !== 1 || store.resellers[0].seller.id !== auth.userId) ? {
                label: "Best Price",
                field: <div className={s.priceWrapper}>
                    <div className={`flex flex-column justify-content-between ${s.priceInfo}`}>
                        <div className={`${s.priceContainer} flex`}>
                            <span className={`icon-robux ${s.priceIcon}`}/>
                            <span className={s.priceLabel}
                                  style={{ color: "var(--robux-color)" }}>{formatNum(purchaseInfo?.price || 0)}</span>
                        </div>
                        <span className={`${s.detailTxt} invis767`}>See more <a className={`link2018`} style={{
                            fontSize: "inherit",
                            fontWeight: "inherit"
                        }} onClick={() => {
                            document.getElementById("itemResellerHeader").scrollIntoView({ behavior: "smooth" });
                        }}>Resellers</a></span>
                    </div>
                    <ActionButton className={s.buyBtn} label="Buy" buttonStyle={buttonStyles.newBuyButton}
                                  onClick={() => {
                                      const purch = store.getPurchaseInfo();
                                      if (!purch) return;
                                      modal.setBuyingUIAD(purch.userAssetId);
                                      modal.setBuyModalOpen(true);
                                  }}/>
                    <span className={`${s.detailTxt} invis-767`}>See more <a className={`link2018`} style={{
                        fontSize: "inherit",
                        fontWeight: "inherit"
                    }} onClick={() => {
                        document.getElementById("itemResellerHeader").scrollIntoView({ behavior: "smooth" });
                    }}>Resellers</a></span>
                </div>,
                labelClass: `${s.attrLabel} invis767`,
            } : (itemDetails.itemRestrictions.includes("Limited") || itemDetails.itemRestrictions.includes("LimitedUnique")) && !itemDetails.isForSale ? {
                label: "No one is selling this item currently.",
                field: <div className={s.noPriceContainer}>
                    <span className={`${s.detailTxt} top invis-767`}>No one is selling this item currently.</span>
                    <ActionButton className={s.buyBtn} label="Buy" buttonStyle={buttonStyles.newBuyButton}
                                  divClassName={s.buyBtnContainerr} disabled={true}/>
                </div>,
                labelClass: `${s.offsaleLabel} invis767`,
            } : store.isOwned && AssetTypeCategory.All.includes(itemDetails.assetType) ? {
                label: "This item is available in your inventory.",
                field: <div className={s.editWrapper}>
                    <Link href={getAvPageStyle() === avPageStyleType.Legacy ? '/My/Character.aspx' : '/My/Avatar'}>
                        <a href={getAvPageStyle() === avPageStyleType.Legacy ? '/My/Character.aspx' : '/My/Avatar'}>
                            <ActionButton
                                className={`${s.editBtn} ${s.buyBtn}`}
                                divClassName={s.buyBtnContainer}
                                label="Edit Avatar"
                                buttonStyle={buttonStyles.newCancelButton}
                            />
                        </a>
                    </Link>
                </div>,
                labelClass: s.availableInventoryLabel,
                parentClass: s.availableInventoryParent
            } : itemDetails.isForSale ? {
                label: "Price",
                field: <div className={s.priceWrapper}>
                    <div className={`${s.priceContainer} flex`}>
                        {
                            // this should be a <Currency/>
                            itemDetails.priceTickets
                            ?
                            <>
                                <span className={`icon-tix ${s.priceIcon}`} style={itemDetails.priceTickets === 0 ? { display: "none" } : {}}/>
                                <span className={s.priceLabel}
                                      style={{ color: "var(--tix-color)" }}>{itemDetails.priceTickets === 0 && "Free" || formatNum(itemDetails.priceTickets)}</span>
                            </>
                            :
                            <>
                                <span className={`icon-robux ${s.priceIcon}`} style={itemDetails.price === 0 ? { display: "none" } : {}}/>
                                <span className={s.priceLabel}
                                      style={{ color: "var(--robux-color)" }}>{itemDetails.price === 0 && "Free" || formatNum(itemDetails.price)}</span>
                            </>
                        }
                    </div>
                    <ActionButton className={s.buyBtn} divClassName={s.buyBtnContainer} divChildren={
                        itemDetails.offsaleDeadline && new Date() < new Date(itemDetails.offsaleDeadline)
                        ?
                        <div className={`${s.saleClockContainer}`} offsaleBy={itemDetails.offsaleDeadline}>
                            Offsale in
                            <Countdown className={s.saleClock} timestamp={itemDetails.offsaleDeadline}/>
                        </div>
                        :
                        null
                    } label={`${purchaseInfo?.expectedPrice === 0 ? "Get" : "Buy"}`} buttonStyle={`${buttonStyles.newBuyButton} ${itemDetails.priceTickets ? "tix" : ""}`} onClick={() => modal.setBuyModalOpen(true)}/>
                </div>,
                labelClass: `${s.attrLabel} invis767`,
            } : {
                label: "This item is not currently for sale.",
                field: <div className={s.noPriceContainer}>
                    <span className={`${s.detailTxt} top invis-767`}>This item is not currently for sale.</span>
                    <ActionButton className={s.buyBtn} label="Buy" buttonStyle={buttonStyles.newBuyButton}
                                  divClassName={s.buyBtnContainerr} disabled={true}/>
                </div>,
                labelClass: `${s.offsaleLabel} invis767`,
            },
            {
                label: ' ',
                field: <pre
                    className={s.itemDescription}>{itemDetails.description?.replace(/\\n/g, "\n") || "No description available"}</pre>,
                labelClass: `display-none`,
                parentClass: `invis-767`,
            },
            {
                label: "Type",
                field: getTypeStrFromTypeNum(itemDetails.assetType),
            },
            {
                label: "Sales",
                field: formatNum(itemDetails.saleCount),
            },
            {
                label: "Created",
                field: dayjs(itemDetails.createdAt).format('M/D/YYYY'),
            },
            {
                label: "Genres",
                field: <div>
                    {
                        itemDetails.genres.map((genre, ind) => <>
                            <Link href={`/catalog?genres=${genresList.find(d => d.name === genre).genre}`}>
                                <a
                                    href={`/catalog?genres=${genresList.find(d => d.name === genre).genre}`}
                                    className={`${s.genreLabel} link2018`}
                                >{genre}</a>
                            </Link>
                            {
                                itemDetails.genres.length - 1 !== ind ? <span>, </span> : null
                            }
                        </>)
                    }
                </div>,
            },
            // itemDetails.assetType === AssetType.Gear ? {
            //     label: "Attributes",
            //     field: <span>
            //         {
            //             itemDetails.genres.map(genre => <>
            //                 <Link href={`/catalog?genres=${genresList.find(d => d.name === genre).genre}`}>
            //                     <a
            //                         href={`/catalog?genres=${genresList.find(d => d.name === genre).genre}`}
            //                         className={s.genreLabel}
            //                     >{genre}</a>
            //                 </Link>
            //                 <span>, </span>
            //             </>)
            //         }
            //     </span>,
            // } : null,
            {
                label: "Description",
                field: <pre
                    className={s.itemDescription}>{itemDetails.description?.replace(/\\n/g, "\n") || "No description available"}</pre>,
                parentClass: `invis767`,
            }
        ].filter(d => !!d));
    }, [itemDetails, store.details, store.resellers, store.isOwned]);
    
    useEffect(() => {
        document.body.setAttribute("style", "background: #e3e3e3;");
    }, [itemDetails]);
    
    if (!itemDetails || !store.details) {
        return <span className="spinner" style={{ height: "100%", backgroundSize: "auto 36px" }}/>
    }
    
    const ItemThumb = ({ isVisible }) => <div
        className={`${s.itemThumbContainer} flex flex-column ${!isVisible ? "invisible-1" : null}`}>
        <div
            className={`w-fit-content position-relative ${s.itemThumb} ${itemDetails.assetType === AssetType.Badge || itemDetails.assetType === AssetType.GamePass ? s.smallImg : ""}`}>
            <ItemImage name={itemDetails.name} id={itemDetails.id} className={`${s.img}`}/>
            <div className={s.itemStatusContainer}>
                {
                    isNew
                    ?
                    <div className={s.itemStatusSale}>
                        <span className={`${s.itemStatusSaleIcon} icon-clock`}/>
                        <span className={s.itemStatusSaleNew}>New</span>
                    </div>
                    :
                    null
                }
                {
                    itemDetails.offsaleDeadline && new Date() < new Date(itemDetails.offsaleDeadline)
                    ?
                    <div className={s.itemStatusSale}>
                        <span className={`${s.itemStatusSaleIcon} icon-clock`}/>
                    </div>
                    :
                    null
                }
            </div>
            <div className={s.restrictionsContainer}>
                {
                    itemDetails.itemRestrictions.includes("LimitedUnique")
                    ?
                    <span className="icon-labels LimitedUnique"/>
                    :
                    itemDetails.itemRestrictions.includes("Limited")
                    ?
                    <span className="icon-labels Limited"/>
                    :
                    null
                }
            </div>
            {
                itemDetails.assetType === AssetType.Badge || itemDetails.assetType === AssetType.GamePass
                ?
                <RelatedGame/>
                :
                null
            }
            {
                itemDetails.assetType === AssetType.Audio
                ?
                <AudioPlayButton audioId={itemDetails.id}/>
                :
                null
            }
        </div>
        <div className={`${s.itemInteractionContainer} ${isWindow767 ? "display-none" : ""}`}>
            <div id="desktopInteractionContainer" className={`flex ${s.favBtnContainer}`}>
                <FavouriteButton assetId={itemDetails.id} initFavCount={itemDetails.favoriteCount}/>
            </div>
        </div>
    </div>
    
    return <div className={s.container}>
        {modal.isBuyModalOpen ? <BuyModal/> : null}
        {modal.isSellItemModalOpen ? <SellItemModal/> : null}
        {modal.isConfirmSellModalOpen ? <ConfirmSellModal/> : null}
        {modal.isRemoveInvModalOpen ? <RemoveItemModal/> : null}
        <div className={`section-content noShadow ${s.itemContainer}`}>
            <ItemThumb isVisible={!isWindow767}/>
            <div className={s.itemDetailsContainer}>
                <div className={`${s.itemHeaderContainer} flex w-100 flex-column`}>
                    <h2>{itemDetails.name}</h2>
                    <div className={`flex align-items-center ${s.itemHeaderInfo}`}>
                        <span style={{ color: "var(--text-color-secondary)" }}>By <CreatorLink
                            type={itemDetails.creatorType} id={itemDetails.creatorTargetId}
                            name={itemDetails.creatorName}/></span>
                        {
                            store.isOwned ? <>
                                <div className={s.ownedStat}>
                                    <span className='icon-checkmark-white-hold'/>
                                </div>
                                <span style={{
                                    fontWeight: 500,
                                    fontSize: 14
                                }}>Item Owned {store.isResellable() && store.ownedCopies.length > 0 ? `(${store.ownedCopies.length})` : ""}</span>
                            </> : null
                        }
                    </div>
                    <div className={s.dropdownContainer}>
                        <AssetDropdown/>
                    </div>
                </div>
                <ItemThumb isVisible={isWindow767} />
                <div className={`${s.itemDetails} flex w-100 flex-column`}>
                    {
                        detailOptions.map((item, ind) => {
                            // are we first, and if not, is previous one price?
                            const isAfterBuy =
                                detailOptions[0] !== item && detailOptions[ind - 1].label.includes("Price");
                            return <div
                                className={`${isAfterBuy ? "margin-none" : ''} ${s.attrContainer} ${item.parentClass}`}>
                                <span
                                    className={`${!item?.labelClass ? s.attrLabel : item.labelClass}`}
                                >{item?.label}</span>
                                {
                                    typeof item.field === 'string'
                                    ?
                                    <span className={s.attrVal}>{item.field}</span>
                                    :
                                    item.field
                                }
                            </div>
                        })
                    }
                    <div className={`${s.itemInteractionContainer} ${!isWindow767 ? "display-none" : ""}`}>
                        <div id="mobileInteractionContainer" className={`flex ${s.favBtnContainer}`}>
                            <FavouriteButton assetId={itemDetails.id} initFavCount={itemDetails.favoriteCount}/>
                        </div>
                    </div>
                    {
                        detailOptions.length === 0
                        ?
                        <span className="spinner" style={{ backgroundSize: "auto 36px" }}/>
                        :
                        null
                    }
                </div>
            </div>
        </div>
        {
            !isWindow767 && store.isResellable() ? <>
                <PriceChart/>
                <Resellers/>
                <Owners/>
            </> : store.isResellable() ? <>
                <HorizontalTabs
                    options={[
                        {
                            name: "Price Chart",
                            element: <PriceChart isLabelHidden />,
                        },
                        {
                            name: "Resellers",
                            element: <Resellers isLabelHidden />,
                        },
                        {
                            name: "Owners",
                            element: <Owners isLabelHidden />,
                        },
                    ]}
                />
            </> : null
        }
        <AssetRecommendations/>
    </div>
}

export default function DetailsPageContainer({ details }) {
    const s = useStyles();
    
    return <Theme2016>
        <FavouriteButtonStore.Provider>
            <div className="container big noPad767">
                <UserAdvertisement type={UserAdvertisementType.Banner728x90} wrapperClass={s.bannerAdContainer}/>
                <div className={s.pageWrapper}>
                    <AssetDetailsPage itemDetails={details}/>
                    <UserAdvertisement type={UserAdvertisementType.SkyScraper160x600}
                                       wrapperClass={s.skyscraperAdContainer}
                                       backupWidth="160px"/>
                </div>
            </div>
        </FavouriteButtonStore.Provider>
    </Theme2016>
}

/**
 * @param {number} type
 * @param {boolean} [simple]
 * @returns {string}
 */
export function getTypeStrFromTypeNum(type, simple = false) {
    switch (type) {
        case AssetType.Hat:
            return (simple ? "" : "Accessory | ") + "Hat";
        case AssetType.HairAccessory:
            return (simple ? "" : "Accessory | ") + "Hair";
        case AssetType.FaceAccessory:
            return (simple ? "" : "Accessory | ") + "Face";
        case AssetType.NeckAccessory:
            return (simple ? "" : "Accessory | ") + "Neck";
        case AssetType.ShoulderAccessory:
            return (simple ? "" : "Accessory | ") + "Shoulder";
        case AssetType.FrontAccessory:
            return (simple ? "" : "Accessory | ") + "Front";
        case AssetType.BackAccessory:
            return (simple ? "" : "Accessory | ") + "Back";
        case AssetType.WaistAccessory:
            return (simple ? "" : "Accessory | ") + "Waist";
        case AssetType.Animation:
            return "Animation";
        case AssetType.ClimbAnimation:
            return "Climb Animation";
        case AssetType.DeathAnimation:
            return "Death Animation";
        case AssetType.FallAnimation:
            return "Fall Animation";
        case AssetType.IdleAnimation:
            return "Idle Animation";
        case AssetType.JumpAnimation:
            return "Jump Animation";
        case AssetType.RunAnimation:
            return "Run Animation";
        case AssetType.SwimAnimation:
            return "Swim Animation";
        case AssetType.WalkAnimation:
            return "Walk Animation";
        case AssetType.PoseAnimation:
            return "Pose Animation";
        case AssetType.EmoteAnimation:
            return "Emote";
        case AssetType.Shirt:
            return "Shirt";
        case AssetType.Pants:
            return "Pants";
        case AssetType.TeeShirt:
        case AssetType.TShirt:
            return "T-Shirt";
        case AssetType.Model:
            return "Model";
        case AssetType.Plugin:
            return "Plugin";
        case AssetType.Mesh:
            return "Mesh";
        case AssetType.MeshPart:
            return "Mesh Part";
        case AssetType.Decal:
            return "Decal";
        case AssetType.Image:
            return "Image";
        case AssetType.Audio:
            return "Audio";
        case AssetType.Video:
            return "Video";
        case AssetType.Package:
            return "Package";
        case AssetType.GamePass:
            return "Game Pass";
        case AssetType.Badge:
            return "Badge";
        case AssetType.Gear:
            return "Gear";
        case AssetType.Place:
            return "Place";
        case AssetType.SolidModel:
            return "Solid Model";
        case AssetType.Head:
            return "Head";
        case AssetType.Torso:
            return "Torso";
        case AssetType.LeftArm:
            return "Left Arm";
        case AssetType.LeftLeg:
            return "Left Leg";
        case AssetType.RightArm:
            return "Right Arm";
        case AssetType.RightLeg:
            return "Right Leg";
        case AssetType.Face:
            return "Face";
        default:
            return "Unknown";
    }
}

/**
 * @param {string} iso
 * @param {number} days
 * @returns {boolean}
 */
function IsISOWithinDays(iso, days) {
    iso = iso.replace(/\.\d{6}Z$/, "Z");
    const then = new Date(iso);
    const now = new Date();
    const dayNum = days * 24 * 60 * 60 * 1000;
    return (now - then) < dayNum;
}

/**
 * @param {number|string} x
 * @returns {string}
 */
export function formatNum(x) {
    if (typeof x !== "number" && typeof x !== "string") return x; // return x or ""?
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

/**
 * @typedef DetailOptionEntry
 * @property {string|JSX.Element} label
 * @property {string|JSX.Element} field
 * @property {string?} labelClass
 * @property {string?} parentClass
 */

const PortalComponent = ({ children, target }) => {
    return target ? ReactDOM.createPortal(children, target) : null;
};