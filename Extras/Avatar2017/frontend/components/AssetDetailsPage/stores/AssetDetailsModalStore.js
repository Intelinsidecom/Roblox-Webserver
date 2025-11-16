import { createContainer } from "unstated-next";
import { useEffect, useState } from "react";
import AssetDetailsStore from "./AssetDetailsStore";
import Authentication from "../../../stores/authentication";
import { CurrencyType } from "../../../models/enums";
import { purchaseItem, takeResellableAssetOffSale } from "../../../services/economy";
import PurchaseError from "../../catalogDetailsPage/purchaseError";
import Feedback from "../../../stores/feedback";
import { wait } from "../../../lib/utils";
import { FeedbackType } from "../../../models/feedback";

const AssetDetailsModalStore = createContainer(() => {
    const auth = Authentication.useContainer();
    const store = AssetDetailsStore.useContainer();
    const feedback = Feedback.useContainer();
    const { getPurchaseInfo } = store;
    const [isBuyModalOpen, setBuyModalOpen] = useState(false);
    const [isSellItemModalOpen, setSellItemModalOpen] = useState(false);
    const [isConfirmSellModalOpen, setConfirmSellModalOpen] = useState(false);
    const [isDelistItemModalOpen, setDelistItemModalOpen] = useState(false);
    const [isRemoveInvModalOpen, setRemoveInvModalOpen] = useState(false);
    const [isDelisting, setDelisting] = useState(false);
    
    const [purchaseInfo, setPurchaseInfo] = useState(/** @type {PurchaseDetails|null} */(null));
    const [buyingUIAD, setBuyingUIAD] = useState(0);
    const [delistingUAID, setDelistingUAID] = useState(0);
    const [newBalance, setNewBalance] = useState(0);
    const [purchaseState, setPurchaseState] = useState(PurchaseState.Purchasing);
    
    const [minSalePrice, setMinSalePrice] = useState(1);
    const [resalePrice, setResalePrice] = useState(1);
    const [sellingSerial, setSellingSerial] = useState(/** @type ResellerData */(store.ownedCopies[0]));
    
    useEffect(() => {
        if (!store.resaleData?.recentAveragePrice) return;
        // some padding for min resale price
        setMinSalePrice(store.resaleData.recentAveragePrice * 0.6);
    }, [store.resaleData]);
    
    useEffect(() => {
        setSellingSerial(store.ownedCopies[0]);
    }, [store.ownedCopies]);
    
    useEffect(() => {
        if (!isBuyModalOpen) return;
        
        const purchInfo = getPurchaseInfo(buyingUIAD === 0 ? null : buyingUIAD);
        if (!purchInfo) {
            if (store.isOwned) {
                setPurchaseState(PurchaseState.Owned);
            } else {
                setPurchaseState(PurchaseState.Purchasing);
            }
            return;
        }
        setPurchaseInfo(purchInfo);
        setNewBalance((purchInfo.currency === CurrencyType.Tickets ? auth.tix : auth.robux) - purchInfo.expectedPrice);
        setPurchaseState(newBalance < 0 ? PurchaseState.InsufficientFunds : PurchaseState.Purchasable);
    }, [isBuyModalOpen, buyingUIAD]);
    
    useEffect(async () => {
        if (delistingUAID === 0 || isDelisting) return;
        setDelisting(true);
        
        try {
            await takeResellableAssetOffSale({ assetId: store.details.id, userAssetId: delistingUAID });
            feedback.addFeedback(`Delisted UAID ${delistingUAID}`);
        } catch (e) {
            feedback.addFeedback(e.message, FeedbackType.ERROR);
        }
        
        await wait(3);
        window.location.reload();
    }, [delistingUAID]);
    
    /**
     * @returns {Promise<PurchaseDetailRequestModel|null>}
     * @constructor
     */
    async function PurchaseAsset() {
        if (purchaseState === PurchaseState.Purchasing || purchaseState === PurchaseState.Purchased || !purchaseInfo) return null;
        setPurchaseState(PurchaseState.Purchasing);
        let res = await purchaseItem({
            assetId: purchaseInfo.assetId,
            productId: purchaseInfo.productId,
            sellerId: purchaseInfo.sellerId,
            userAssetId: purchaseInfo.userAssetId,
            price: purchaseInfo.expectedPrice,
            expectedCurrency: purchaseInfo.currency,
        });
        if (!res?.purchased) {
            // const err = new PurchaseError("PURCHASE_ERROR");
            setPurchaseState(PurchaseState.PurchaseError);
            if (res.reason === 'InsufficientFunds') {
                // err.state = 'INSUFFICIENT_FUNDS';
                setPurchaseState(PurchaseState.InsufficientFunds);
            }
            return null;
            // throw err;
        }
        setPurchaseState(PurchaseState.Purchased)
        return res;
    }
    
    return {
        isBuyModalOpen,
        setBuyModalOpen,
        
        isSellItemModalOpen,
        setSellItemModalOpen,
        
        isConfirmSellModalOpen,
        setConfirmSellModalOpen,
        
        isDelistItemModalOpen,
        setDelistItemModalOpen,
        
        isRemoveInvModalOpen,
        setRemoveInvModalOpen,
        
        resalePrice,
        setResalePrice,
        minSalePrice,
        sellingSerial,
        setSellingSerial,
        
        purchaseState,
        newBalance,
        purchaseInfo,
        buyingUIAD,
        setBuyingUIAD,
        
        delistingUAID,
        setDelistingUAID,
        
        PurchaseAsset,
    }
});

export default AssetDetailsModalStore;

export const PurchaseState = Object.freeze({
    Purchasable: 1,
    Purchasing: 2,
    Purchased: 3,
    InsufficientFunds: 4,
    PurchaseError: 5,
    Owned: 6,
});
