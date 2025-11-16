import { createUseStyles } from "react-jss";
import AssetDetailsModalStore, { PurchaseState } from "../stores/AssetDetailsModalStore";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import NewModal from "../../newModal";
import ActionButton from "../../actionButton";
import Currency from "../../Currency";
import { CurrencySize, CurrencyType } from "../../../models/enums";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import Selector from "../../selector";
import { formatNum } from "../index";

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
    
    nameError: {
        margin: 0,
        height: 24,
        whiteSpace: "pre-line",
        lineHeight: "1.5em!important",
        color: "#D86868",
        wordWrap: "break-word",
        hyphens: "auto",
    },
    padder: {
        padding: "0 10px",
    },
    childrenClass: {
        "& > span": {
            display: "flex",
            alignItems: "center",
            gap: 2,
        }
    },
});

/**
 * @returns {JSX.Element}
 * @constructor
 */
const SellItemModal = () => {
    const s = useStyles();
    const btnStyles = useButtonStyles();
    const auth = AuthenticationStore.useContainer();
    const store = AssetDetailsStore.useContainer();
    const { resaleData } = store;
    const modals = AssetDetailsModalStore.useContainer();
    const { resalePrice, setResalePrice, setSellingSerial, minSalePrice } = modals;
    
    const closeModal = () => {
        setResalePrice(1);
        modals.setSellItemModalOpen(false);
    };
    
    if (!auth?.userId) {
        window.location.href = "/auth/application";
        return null;
    }
    
    if (!resaleData) return null;
    
    // if (modals.purchaseState === PurchaseState.PurchaseError) return <NewModal
    //     title="Error"
    //     children={<>
    //         <p>An error occured while processing this sale. No items have been removed from your
    //             account or put on sale. Please try again later.</p>
    //         <div className={s.imgContainer}>
    //             <span className={`icon-warning-orange-150x150 ${s.robuxContainer}`}/>
    //         </div>
    //     </>}
    //     footerElements={<ActionButton
    //         label="OK"
    //         buttonStyle={btnStyles.newCancelButton}
    //         onClick={closeModal}
    //         className={s.modalBtn}
    //     />}
    //     footerClass={s.footerClass}
    //     containerClass={s.containerClass}
    //     exitFunction={closeModal}
    //     headerBorder={true}
    // />
    
    return <NewModal
        title="Sell Your Collectible Item"
        children={<div className={s.childrenClass}>
            <p style={{ margin: 0, color: "#b8b8b8" }}>Price (minimum {formatNum(Math.ceil(minSalePrice))})</p>
            <div>
                <input
                    type="number"
                    onChange={e => setResalePrice(Number(e.target.value))}
                    className={`inputTextStyle ${resalePrice < minSalePrice || resalePrice > 999999 ? "hasError" : ""} ${s.inputStyle}`}
                    max={999999}
                    min={minSalePrice}
                />
            </div>
            {
                store.ownedCopies.filter(d => d.serialNumber != null).length > 0
                ?
                <span style={{ marginTop: 3, }}>Selling serial <Selector options={store.ownedCopies.filter(d => d.serialNumber != null).map(d => {
                    return {
                        name: d.serialNumber,
                        value: d,
                    }
                })} shadow={true} className={s.padder} onChange={v => setSellingSerial(v.value)}/></span>
                :
                null
            }
            <span>Marketplace fee (at 30%) <Currency labelClass={s.labelClass} iconClass={s.iconClass}
                                                     currencyType={CurrencyType.Robux}
                                                     price={Math.ceil(resalePrice * 0.3)}
                                                     size={CurrencySize["16x16"]}/></span>
            <span>You get <Currency labelClass={s.labelClass} iconClass={s.iconClass} currencyType={CurrencyType.Robux} price={Math.trunc(resalePrice * 0.7)}
                                                     size={CurrencySize["16x16"]}/></span>
        </div>}
        footerElements={<>
            <ActionButton
                label="Sell Now"
                buttonStyle={btnStyles.newContinueButton}
                onClick={async () => {
                    if (resalePrice < minSalePrice || resalePrice > 999999) return;
                    modals.setSellItemModalOpen(false);
                    modals.setConfirmSellModalOpen(true);
                }}
                className={s.modalBtn}
                disabled={resalePrice < minSalePrice || resalePrice > 999999}
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

export default SellItemModal;
