import { createUseStyles } from "react-jss";
import CatalogDetailsPage from '../stores/catalogDetailsPage';
import Currency from './currency';
import BuyButton from './buyButton'

const useStyles = createUseStyles({
    itemDetails: {
        width: '100%',
        float: 'right',
    },
    detailContainer: {
        marginBottom: '12px',
    },
    priceAndBuyContainer: {
        marginBottom: '12px',
    },
    fieldContainer: {
        marginBottom: '12px',
        width: 'calc(100% - 190px)',
        float: 'left',
    },
    fieldLabel: {
        fontSize: '16px',
        fontWeight: '500',
        lineHeight: '1.4em',
        display: 'block',
        width: '120px',
        paddingRight: '9px',
        float: 'left',
        color: '#b8b8b8',
    },

    offSaleText: {
        paddingBottom: '12px',
    },
})

/**
 * Item Details component
 * @param {{details: AssetDetailsEntry}} props
 * @returns 
 */
const Thumbnail = (props) => {
    const s = useStyles();
    const store = CatalogDetailsPage.useContainer();
    const isResellAsset = store.isResellable;

    const showBuyButton = (() => {
        if (isResellAsset) return true;

        if (store?.details?.priceTickets) {
            if (store?.details?.price === null) {
                return false;
            }
        }
        return true;
    })();
    
    const buyButtonClick = e => {
        e.preventDefault();
        console.log("PRINTING!!")
    }

    const showBuyTicketsButton = store.details.priceTickets !== null && !isResellAsset;
    const showOrTab = !isResellAsset && showBuyButton && showBuyTicketsButton;
    const hasOffsaleLabel = store.offsaleDeadline !== null && !isResellAsset && (showBuyButton || showBuyTicketsButton);


    return <div className={s.itemDetails}>
        <div className={`${s.detailContainer}`}>
            {
                !hasOffsaleLabel ?
                    <div className={s.fieldContainer}>
                        <span className={s.fieldLabel}>Price</span>
                        <Currency isRobux={!showBuyTicketsButton} />
                    </div> :
                    <div className={s.offSaleText}>This item is not currently for sale.</div>
            }
            <BuyButton isRobux={!showBuyTicketsButton} onClick={buyButtonClick} disabled={showBuyButton} />
        </div>
        <div className={`${s.detailContainer}`}>
            <div className={s.fieldContainer}>
                <span className={s.fieldLabel}>Price</span>
            </div>
        </div>
    </div>
}

export default Thumbnail;