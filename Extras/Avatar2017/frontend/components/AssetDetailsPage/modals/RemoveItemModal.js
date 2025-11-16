import {createUseStyles} from "react-jss";
import AssetDetailsModalStore, { PurchaseState } from "../stores/AssetDetailsModalStore";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import NewModal from "../../newModal";
import ActionButton from "../../actionButton";
import ItemImage from "../../itemImage";
import { wait } from "../../../lib/utils";
import FeedbackStore from "../../../stores/feedback";
import AuthenticationStore from "../../../stores/authentication";
import { FeedbackType } from "../../../models/feedback";
import useButtonStyles from "../../../styles/buttonStyles";
import { deleteAssetFromInventory } from "../../../services/inventory";
import { useState } from "react";

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
const RemoveItemModal = () => {
    const s = useStyles();
    const btnStyles = useButtonStyles();
    const feedback = FeedbackStore.useContainer();
    const auth = AuthenticationStore.useContainer();
    const store = AssetDetailsStore.useContainer();
    const modals = AssetDetailsModalStore.useContainer();
    const [errored, setErrored] = useState(false);
    
    const closeModal = () => {
        modals.setRemoveInvModalOpen(false);
        setErrored(false);
    };
    
    if (!auth?.userId) {
        window.location.href = "/auth/application";
        return null;
    }
    
    if (errored) return <NewModal
        title="Error"
        children={<>
            <p>An error occured while processing this removal. No items have been removed from your account. Please try again later.</p>
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
    
    return <NewModal
        title={"Remove Item from Inventory"}
        offset={200}
        children={<>
            <span className={s.spanText} style={{ marginBottom: 12, }}>
                Are you sure you want to PERMANENTLY REMOVE this item from your inventory?
                <br/>You will not receive a refund if you bought this item.
            </span>
            <div className={s.imgContainer}>
                <ItemImage id={store.details.id} name={store.details.name} className={s.img} />
            </div>
        </>}
        footerElements={<>
            <div className="flex">
                <ActionButton
                    label="Yes"
                    buttonStyle={`${btnStyles.newWarningButton}`}
                    onClick={async () => {
                        closeModal();
                        let feedbacked = false
                        /** @type AxiosResponse */
                        let purchase;
                        try {
                            purchase = await deleteAssetFromInventory({ assetId: store.details.id });
                        } catch (e) {
                            feedbacked = true
                            purchase = e;
                            console.error("Purchase request failed due to the following error");
                            console.dir(e);
                            setErrored(true);
                        }
                        if (!feedbacked && purchase?.status === 200) {
                            feedback.addFeedback("Removed from Inventory", FeedbackType.SUCCESS, true);
                        } else {
                            // if (purchase) feedback.addFeedback(`Removal failed. Your item has not been removed from your inventory. Code: ${purchase.status}`, FeedbackType.ERROR, true);
                            if (purchase) feedback.addFeedback(purchase?.response?.data?.errors?.length > 0 ? purchase.response.data.errors[0].message : `Removal failed. Your item has not been removed from your inventory. Code: ${purchase.status}`, FeedbackType.ERROR, true);
                            setErrored(true);
                            console.log("Failed to remove item, details:");
                            console.dir(purchase);
                        }
                        await wait(3);
                        window.location.reload();
                    }}
                    className={s.modalBtn}
                />
                <ActionButton
                    label="No"
                    buttonStyle={btnStyles.newCancelButton}
                    onClick={closeModal}
                    className={s.modalBtn}
                />
            </div>
        </>}
        footerClass={s.footerClass}
        containerClass={s.containerClass}
        exitFunction={closeModal}
        headerBorder={true}
    />
}

export default RemoveItemModal;
