import { createUseStyles } from "react-jss";
import AssetDetailsModalStore, { PurchaseState } from "../stores/AssetDetailsModalStore";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import NewModal from "../../newModal";
import ActionButton from "../../actionButton";
import Currency from "../../Currency";
import { CurrencySize, CurrencyType } from "../../../models/enums";
import { wait } from "../../../lib/utils";
import FeedbackStore from "../../../stores/feedback";
import AuthenticationStore from "../../../stores/authentication";
import { FeedbackType } from "../../../models/feedback";
import useButtonStyles from "../../../styles/buttonStyles";
import { setResellableAssetPrice } from "../../../services/economy";

const useStyles = createUseStyles({
    footerClass: {
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
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
    labelClass: { margin: 0, fontSize: "inherit", fontWeight: 500, },
    iconClass: { marginRight: 2, },
});

/**
 * @returns {JSX.Element}
 * @constructor
 */
const ConfirmSellModal = () => {
    const s = useStyles();
    const btnStyles = useButtonStyles();
    const feedback = FeedbackStore.useContainer();
    const auth = AuthenticationStore.useContainer();
    const store = AssetDetailsStore.useContainer();
    const { resaleData } = store;
    const modals = AssetDetailsModalStore.useContainer();
    const { resalePrice, setResalePrice, sellingSerial } = modals;
    
    const closeModal = () => {
        setResalePrice(1);
        modals.setConfirmSellModalOpen(false)
    };
    
    if (!auth?.userId) {
        window.location.href = "/auth/application";
        return null;
    }
    
    if (!resaleData) return null;
    
    if (modals.purchaseState === PurchaseState.PurchaseError) return <NewModal
        title="Error"
        children={<>
            <p>An error occured while processing this sale. No items have been removed from your
                account or put on sale. Please try again later.</p>
            <div className={s.imgContainer}>
                <span className={`icon-warning-orange-150x150 ${s.robuxContainer}`}/>
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
    
    return <NewModal
        containerWidth={500}
        title="Sell Your Collectible Item"
        children={<>
            <span style={{ display: "flex", alignItems: 'center', gap: 2, }}>Are you sure you want to sell {store.details.name} for <Currency labelClass={s.labelClass}
                                                                                   iconClass={s.iconClass}
                                                                                   currencyType={CurrencyType.Robux}
                                                                                   price={Math.trunc(resalePrice)}
                                                                                   size={CurrencySize["16x16"]}/></span>
            <span style={{ display: "flex", alignItems: 'center', gap: 2, }}>{store.details.name}'s recent average price is <Currency labelClass={s.labelClass}
                                                                           iconClass={s.iconClass}
                                                                           currencyType={CurrencyType.Robux}
                                                                           price={resaleData.recentAveragePrice}
                                                                           size={CurrencySize["16x16"]}/></span>
        </>}
        footerElements={<>
            <ActionButton
                label="Yes"
                buttonStyle={btnStyles.newContinueButton}
                onClick={async () => {
                    closeModal();
                    try {
                        await setResellableAssetPrice({ assetId: store.details.id, price: resalePrice, userAssetId: sellingSerial.userAssetId });
                        feedback.addFeedback("Resold item", FeedbackType.SUCCESS, true);
                    } catch (e) {
                        console.error("Resell request failed due to the following error");
                        console.dir(e);
                        feedback.addFeedback(e.message, FeedbackType.ERROR, true);
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
        </>}
        footerClass={s.footerClass}
        containerClass={s.containerClass}
        exitFunction={closeModal}
        headerBorder={true}
    />
}

export default ConfirmSellModal;
