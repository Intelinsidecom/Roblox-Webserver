import {createUseStyles} from "react-jss";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import React, { useEffect, useState } from "react";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";
import Link from "../../link";
import { ThumbnailFromState } from "../../AvatarEditorPage/components/avatarCardList";
import CreatorLink from "../../creatorLink";
import { abbreviateNumber } from "../../../lib/numberUtils";
import Authentication from "../../../stores/authentication";
import AssetDetailsModalStore from "../stores/AssetDetailsModalStore";

const useStyles = createUseStyles({
    containerHeader: {
        margin: "3px 0 6px",
        "& h3": {
            fontSize: 20,
            fontWeight: 700,
            margin: 0,
        },
    },
    resellersWrapper: {
        paddingTop: 0,
        paddingBottom: 0,
        display: "flex",
        flexDirection: "column",
    },
    btnContainer: {
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        width: "100%",
        paddingTop: 12,
    },
    btnContainer2: {
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        marginLeft: 12,
    },
    loadMoreBtn: {
        padding: 4,
        fontSize: 14,
        lineHeight: "100%",
    },
    buyBtn: {
        minWidth: 90,
        color: "#757575!important",
        fontSize: 18,
        lineHeight: '100%',
        padding: 9,
        "&:hover": {
            backgroundColor: "#3fc679",
            borderColor: "#3fc679!important",
            color: "#fff!important",
        }
    },
    priceIcon: {
        marginRight: 2,
        width: 20,
        height: 20,
        backgroundSize: "40px auto",
        backgroundPosition: "0 -80px",
    },
    priceLabel: {
        lineHeight: "1.4em",
        fontSize: 16,
        fontWeight: 500,
        marginTop: 2,
    },
    
    resellerContainer: {
        display: 'flex',
        paddingTop: 12,
        marginBottom: 12,
        borderTop: "1px solid #e3e3e3",
        width: "100%",
        "&:first-child": {
            borderTop: "none",
        },
    },
    resellerImg: {
        marginRight: 24,
        backgroundColor: "#d1d1d1",
        width: 60,
        height: 60,
        padding: 0,
        borderRadius: "50%",
        boxShadow: "0 1px 4px 0 rgba(25,25,25,0.3)",
        transition: "box-shadow 200ms ease",
        display: "inline-block",
        "& img": {
            width: "100%",
            height: "100%",
            borderRadius: "50%",
        },
        "&:hover": {
            boxShadow: "0 1px 6px 0 rgba(25, 25, 25, 0.75)",
        },
        "@media(max-width: 576px)": {
            marginRight: 12,
        },
    },
    priceContainer: {
        marginLeft: "auto",
        alignItems: "center",
        "@media(max-width: 576px)": {
            margin: 0,
        },
    },
    serialText: {
        fontSize: 12,
        fontWeight: 400,
        "@media(max-width: 576px)": {
            marginTop: 2,
        },
    },
    resellerInfo: {
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        width: "calc(100% - 186px)",
        "@media(max-width: 576px)": {
            flexDirection: "column",
            alignItems: "start",
        },
    },
    separator: {
        padding: "0 5px",
        "@media(max-width: 576px)": {
            display: "none",
        },
    },
});

/**
 * @param {boolean} [isLabelHidden]
 * @returns {Element}
 * @constructor
 */
function Resellers({ isLabelHidden = false }) {
    const s = useStyles();
    const store = AssetDetailsStore.useContainer();
    const modal = AssetDetailsModalStore.useContainer();
    const auth = Authentication.useContainer();
    const btnStyles = useButtonStyles();
    const [resellers, setResellers] = useState(/** @type ResellerDataThumb[] */([]));
    
    useEffect(() => {
        setResellers(store.resellers.slice(0, 10).sort((a, b) => a.price - b.price));
    }, [store.resellers]);
    
    const loadMoreResellers = () => setResellers(prev => {
        let newResellers = [
            ...prev,
            ...store.resellers.slice(resellers.length, resellers.length + 10)
        ];
        return newResellers.sort((a, b) => a.price - b.price);
    })
    
    if (resellers.length === 0) {
        return <div>
            {
                !isLabelHidden ? <div className={`flex ${s.containerHeader}`}>
                    <h3>Resellers</h3>
                </div> : null
            }
            <div className="section-content-off" style={{ marginBottom: 18, }}>
                No one is reselling this item currently.
            </div>
        </div>
    }
    
    return <div>
        {
            !isLabelHidden ? <div className={`flex ${s.containerHeader}`}>
                <h3>Resellers</h3>
            </div> : null
        }
        <div className={`section-content noShadow ${s.resellersWrapper}`}>
            {
                resellers.map(reseller => {
                    const href = `/users/${reseller.seller.id}/profile`;
                    return <div className={s.resellerContainer}>
                        <Link href={href}>
                            <a className={s.resellerImg} href={href}>
                                <img src={ThumbnailFromState(reseller.imageUrl, reseller.state)} alt={reseller.seller.name} />
                            </a>
                        </Link>
                        <div className={s.resellerInfo}>
                            <CreatorLink newWeight={true} id={reseller.seller.id} name={reseller.seller.name} type={reseller.seller.type} />
                            <span className={s.separator}>-</span>
                            <span className={s.serialText}>Serial {reseller.serialNumber ? `#${reseller.serialNumber} of ${store.resaleData.sales}` : "N/A"}</span>
                            <div className={`${s.priceContainer} flex`}>
                                <span className={`icon-robux ${s.priceIcon}`}/>
                                <span className={s.priceLabel}
                                      style={{ color: "var(--robux-color)" }}>{abbreviateNumber(reseller.price)}</span>
                            </div>
                        </div>
                        <div className={s.btnContainer2}>
                            {
                                reseller.seller.id !== auth.userId
                                ?
                                <ActionButton
                                    label="Buy"
                                    onClick={() => {
                                        modal.setBuyingUIAD(reseller.userAssetId);
                                        modal.setBuyModalOpen(true);
                                    }}
                                    buttonStyle={btnStyles.newCancelButton}
                                    className={s.buyBtn}
                                />
                                :
                                <ActionButton
                                    label="Remove"
                                    onClick={() => {
                                        modal.setDelistingUAID(reseller.userAssetId);
                                        modal.setDelistItemModalOpen(true);
                                    }}
                                    buttonStyle={btnStyles.newCancelButton}
                                    className={s.buyBtn}
                                />
                            }
                        </div>
                    </div>
                })
            }
            {
                store.resellers.length !== resellers.length
                ?
                <div className={s.btnContainer}>
                    <ActionButton
                        label="See More"
                        onClick={() => loadMoreResellers()}
                        buttonStyle={btnStyles.newCancelButton}
                        className={s.loadMoreBtn}
                    />
                </div>
                :
                null
            }
            {/*<span className="spinner" style={{ width: "100%", backgroundSize: "auto 36px" }}/>*/}
        </div>
    </div>
}

export default Resellers;
