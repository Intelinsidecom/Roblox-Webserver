import {createUseStyles} from "react-jss";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import React, { useEffect, useState } from "react";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";
import Link from "../../link";
import { ThumbnailFromState } from "../../AvatarEditorPage/components/avatarCardList";
import CreatorLink from "../../creatorLink";
import Authentication from "../../../stores/authentication";
import dayjs from "dayjs";

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
        color: "#b8b8b8",
        "@media(max-width: 576px)": {
            fontSize: 12,
        }
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
function Owners({ isLabelHidden = false }) {
    const s = useStyles();
    const store = AssetDetailsStore.useContainer();
    const auth = Authentication.useContainer();
    const btnStyles = useButtonStyles();
    const [owners, setOwners] = useState(/** @type OwnerEntryThumb[] */([]));
    
    useEffect(() => setOwners(store.owners.slice(0, 10)),
        [store.owners]);
    
    const loadMoreOwners = () => setOwners(prev => {
        let newOwners = [
            ...prev,
            ...store.owners.slice(owners.length, owners.length + 10)
        ];
        newOwners.sort((a, b) => new Date(a.created) - new Date(b.created))
        return newOwners;
    })
    
    if (owners.length === 0) return <div>
        {
            !isLabelHidden ? <div id="itemResellerHeader" className={`flex ${s.containerHeader}`}>
                <h3 style={{ margin: 0, }}>Owners</h3>
            </div> : null
        }
        <div className={`section-content-off noShadow ${s.resellersWrapper}`} style={{ padding: 15 }}>
            No one owns this item currently.
        </div>
    </div>;
    
    return <div>
        {
            !isLabelHidden ? <div id="itemResellerHeader" className={`flex ${s.containerHeader}`}>
                <h3 style={{ margin: 0, }}>Owners</h3>
            </div> : null
        }
        <div className={`section-content noShadow ${s.resellersWrapper}`}>
            {
                owners.map(owner => {
                    const href = owner.owner ? `/users/${owner.owner.id}/profile` : "#";
                    return <div className={s.resellerContainer}>
                        <Link href={href}>
                            <a className={s.resellerImg} href={href}>
                                <img src={ThumbnailFromState(owner.imageUrl || "", owner.state || "")}
                                     alt={owner.owner?.name || "Deleted / Private"}/>
                            </a>
                        </Link>
                        <div className={s.resellerInfo}>
                            {
                                owner.owner
                                ?
                                <CreatorLink newWeight={true} id={owner.owner.id} name={owner.owner.name}
                                             type={owner.owner.type}/>
                                :
                                <span>Deleted / Private</span>
                            }
                            <span className={s.separator}>-</span>
                            <span className={s.serialText}>Serial {owner.serialNumber ? `#${owner.serialNumber} of ${store.resaleData.sales}` : "N/A"}</span>
                            <div className={`${s.priceContainer} flex`}>
                                <span className={s.priceLabel}>{dayjs(owner.updated).fromNow()}</span>
                            </div>
                        </div>
                        <div className={s.btnContainer2}>
                            {
                                !owner.owner || !owner?.owner?.name || (owner.owner.id !== auth.userId && owner.owner.id !== 1 && owner.owner.id !== 68)
                                ?
                                <ActionButton
                                    label="Trade"
                                    onClick={() => {
                                        window.open("/Trade/TradeWindow.aspx?TradePartnerID=" + owner.owner.id, "_blank", "scrollbars=0, height=608, width=914");
                                    }}
                                    buttonStyle={btnStyles.newCancelButton}
                                    className={s.buyBtn}
                                />
                                :
                                null
                            }
                        </div>
                    </div>
                })
            }
            {
                store.owners.length !== owners.length
                ?
                <div className={s.btnContainer}>
                    <ActionButton
                        label="See More"
                        onClick={loadMoreOwners}
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

export default Owners;
