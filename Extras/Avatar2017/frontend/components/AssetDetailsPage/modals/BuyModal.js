import {createUseStyles} from "react-jss";
import AssetDetailsModalStore, { PurchaseState } from "../stores/AssetDetailsModalStore";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import NewModal from "../../newModal";
import ActionButton from "../../actionButton";
import { getTypeStrFromTypeNum } from "../index";
import Currency from "../../Currency";
import { CurrencySize, CurrencyType, GetEnum } from "../../../models/enums";
import ItemImage from "../../itemImage";
import { wait } from "../../../lib/utils";
import FeedbackStore from "../../../stores/feedback";
import AuthenticationStore from "../../../stores/authentication";
import { FeedbackType } from "../../../models/feedback";
import useButtonStyles from "../../../styles/buttonStyles";
import { useEffect } from "react";

const useStyles = createUseStyles({
    footerClass: {
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        flexDirection: "column",
    },
    modalBtn: {
        padding: 9,
        margin: "0 5px",
        minWidth: 90,
        fontSize: "18px!important",
        fontWeight: "500!important",
        lineHeight: "100%!important",
    },
    containerClass: {
        "& h5": {
            fontSize: 18,
            fontWeight: 400,
            lineHeight: "1em",
        }
    },
    currencyContainer: {
        color: "#b8b8b8",
        "& *": {
            filter: "",
            color: "#b8b8b8"
        }
    },
    imgContainer: {
        width: "100%",
        display: "flex",
        justifyContent: "center",
    },
    img: {
        width: "55%",
        padding: 0,
    },
    robuxContainer: {
        height: "100%",
        aspectRatio: "840 / 540",
    },
    spanText: {
        display: "flex",
        flexDirection: "row",
        flexWrap: "wrap",
        alignItems: "center",
    },
    ffffff: { marginLeft: 2, },
    fffffff: { marginRight: 2, },
    labelClass: { margin: 0, fontSize: "inherit", fontWeight: 500, },
    iconClass: { marginRight: 2, },
});

/**
 * @returns {JSX.Element}
 * @constructor
 */
const BuyModal = () => {
    const s = useStyles();
    const btnStyles = useButtonStyles();
    const feedback = FeedbackStore.useContainer();
    const auth = AuthenticationStore.useContainer();
    const store = AssetDetailsStore.useContainer();
    const modals = AssetDetailsModalStore.useContainer();
    const { newBalance, purchaseInfo } = modals;
    
    const closeModal = () => modals.setBuyModalOpen(false);
    
    if (!auth?.userId) {
        window.location.href = "/auth/application";
        return null;
    }
    
    if (!purchaseInfo) return null;
    
    if (modals.purchaseState === PurchaseState.PurchaseError) return <NewModal
        title="Error"
        children={<>
            <p>An error occured while processing this transaction. No ROBUX or Tickets have been removed from your account. Please try again later.</p>
            <div className={s.imgContainer}>
                <span className={`icon-warning-orange-150x150 ${s.robuxContainer}`} />
            </div>
        </>}
        footerElements={<ActionButton
            label="OK"
            buttonStyle={btnStyles.newCancelButton}
            onClick={closeModal}
            className={s.modalBtn}
        />}
        footerClass={s.footerClass}
        containerClass={s.containerClass}
        exitFunction={closeModal}
        headerBorder={true}
    />
    
    if (newBalance < 0 || modals.purchaseState === PurchaseState.InsufficientFunds) return <NewModal
        title="Insufficient Funds"
        // children={<>
        //     <p>You need <Currency currencyType={purchaseInfo.currency} price={purchaseInfo.expectedPrice} /> more to purchase this item.</p>
        //     <div className={s.imgContainer}>
        //             <span className={s.robuxContainer}>
        //                 <img alt="ROBUX" src="/img/ROBUX.webp" style={{ display: 'inline-block', width: '100%', height: '100%', verticalAlign: 'middle' }} />
        //             </span>
        //     </div>
        // </>}
        children={<>
            <span className={s.spanText} style={{ marginBottom: 12, }}>
                You need <Currency canBeFree currencyType={purchaseInfo.currency} size={CurrencySize["16x16"]} price={newBalance * -1 || "NaN"} divClass={s.ffffff + ' ' + s.fffffff} labelClass={s.labelClass} iconClass={s.iconClass}/> more to purchase this item.
            </span>
            <div className={s.imgContainer}>
                <img alt={purchaseInfo.currency === 2 ? "TIX" : "ROBUX"} className={s.img} src={`/img/${purchaseInfo.currency === 2 ? "TIX" : "ROBUX"}.webp`} style={{
                    display: 'inline-block',
                    width: purchaseInfo.currency === 2 ? '45%' : '100%',
                    height: '100%',
                    verticalAlign: 'middle'
                }}/>
            </div>
        </>}
        footerElements={<div className="flex">
            <ActionButton
                label={`Buy ${purchaseInfo.currency === 2 ? "Tickets" : "Robux"}`}
                buttonStyle={`${btnStyles.newBuyButton} ${purchaseInfo.currency === 2 ? "tix" : ""}`}
                onClick={async () => {
                    closeModal();
                    window.location.href = "/help/earn";
                }}
                className={s.modalBtn}
            />
            <ActionButton
                label="Cancel"
                buttonStyle={btnStyles.newCancelButton}
                onClick={closeModal}
                className={s.modalBtn}
            />
        </div>}
        footerClass={s.footerClass}
        containerClass={s.containerClass}
        exitFunction={closeModal}
        headerBorder={true}
    />
    console.dir(purchaseInfo);
    return <NewModal
        title={purchaseInfo.expectedPrice === 0 ? "Get Item" : "Buy Item"}
        offset={200}
        children={<>
            <span className={s.spanText} style={{ marginBottom: 12, }}>
                Would you like to buy the {getTypeStrFromTypeNum(store.details.assetType, true)}: <b style={{padding: "0 3px"}}>{store.details.name}</b> from {store.details.creatorName} for <Currency canBeFree currencyType={purchaseInfo.currency} size={CurrencySize["16x16"]} price={purchaseInfo.expectedPrice || (typeof purchaseInfo.expectedPrice === 'number' ? 0 : "NaN")} divClass={s.ffffff} labelClass={s.labelClass} iconClass={s.iconClass}/>?
            </span>
            <div className={s.imgContainer}>
                <ItemImage id={store.details.id} name={store.details.name} className={s.img} />
            </div>
        </>}
        footerElements={<>
            <div className="flex">
                <ActionButton
                    label={`${purchaseInfo.expectedPrice === 0 ? "Get" : "Buy"} Now`}
                    buttonStyle={`${btnStyles.newBuyButton} ${purchaseInfo.currency === 2 ? "tix" : ""}`}
                    onClick={async () => {
                        closeModal();
                        let feedbacked = false
                        let purchase;
                        try {
                            purchase = await modals.PurchaseAsset()
                        } catch (e) {
                            console.error("Purchase request failed due to the following error");
                            console.dir(e);
                            if (e.message.toLowerCase().includes("already owned")) {
                                feedbacked = true;
                                feedback.addFeedback(e.message, FeedbackType.ERROR, true);
                            }
                        }
                        if (purchase) {
                            feedback.addFeedback("Purchase completed", FeedbackType.SUCCESS, true);
                        } else {
                            if (!feedbacked) feedback.addFeedback("Purchase failed. You have not been charged", FeedbackType.ERROR, true);
                        }
                        await wait(3);
                        window.location.reload();
                    }}
                    className={s.modalBtn}
                />
                <ActionButton
                    label="Cancel"
                    buttonStyle={btnStyles.newCancelButton}
                    onClick={closeModal}
                    className={s.modalBtn}
                />
            </div>
            {purchaseInfo.expectedPrice === 0
             ?
             <span className={`flex flex-wrap align-items-center`} style={{ marginTop: 12, color: "#b8b8b8", }}>Your balance after this transaction will remain the same.</span>
             :
             <span className={`flex flex-wrap align-items-center`} style={{ marginTop: 12, color: "#b8b8b8", }}>Your balance after this transaction will be <Currency canBeFree
                 currencyType={purchaseInfo.currency} price={newBalance} size={CurrencySize["16x16"]} grayed={true}
                 divClass={s.ffffff} labelClass={`color-text-secondary mt-44444 ${s.labelClass}`}
                 iconClass={`bg-pos${purchaseInfo.currency === 2 ? "-tix" : ""} ${s.iconClass}`}/></span>
                
            }
        </>}
        footerClass={s.footerClass}
        containerClass={s.containerClass}
        exitFunction={closeModal}
        headerBorder={true}
    />
}

export default BuyModal;
