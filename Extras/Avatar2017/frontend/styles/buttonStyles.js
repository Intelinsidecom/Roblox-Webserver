import { createUseStyles } from "react-jss";

/**
 * @typedef ButtonStyles
 * @property {string} buyButton
 * @property {string} normal
 * @property {string} continueButton
 * @property {string} cancelButton
 * @property {string} badPurchaseRow
 * @property {string} newBuyButton
 * @property {string} newContinueButton
 * @property {string} newDisabledContinueButton
 * @property {string} newCancelButton
 * @property {string} newDisabledCancelButton
 * @property {string} newWarningButton
 */

/**
 * @type {() => ButtonStyles}
 */
const useButtonStyles = createUseStyles({
    buyButton: {
        //width: '100%',
        fontFamily: 'Source Sans Pro, Arial',
        paddingTop: '5px',
        paddingBottom: '5px',
        borderColor: '#007001!important',
        background: 'linear-gradient(0deg, rgba(0,113,0,1) 0%, rgba(64,193,64,1) 100%)', // 40c140 #007100
        '&:hover': {
            background: 'linear-gradient(0deg, rgba(71,232,71,1) 0%, rgba(71,232,71,1) 100%)', // 47e847 02a101
        },
    },
    normal: {
        width: 'auto!important',
    },
    continueButton: {
        fontFamily: 'Source Sans Pro, Arial',
        //width: '100%',
        paddingTop: '5px',
        paddingBottom: '5px',
        borderColor: '#0852b7!important',
        background: 'linear-gradient(0deg, rgba(8,79,192,1) 0%, rgba(5,103,234,1) 100%)', // #0567ea #084fc0
        border: '1px solid #084ea6',
        '&:hover': {
            background: 'linear-gradient(0deg, rgba(2,73,198,1) 0%, rgba(7,147,253,1) 100%); ',
        },
    },
    cancelButton: {
        fontFamily: 'Source Sans Pro, Arial',
        //width: '100%',
        paddingTop: '5px',
        paddingBottom: '5px',
        borderColor: '#565656!important',
        background: 'linear-gradient(0deg, rgba(69,69,69,1) 0%, rgba(140,140,140,1) 100%)', // top #8c8c8c bottom #454545
        border: '1px solid #404041',
        '&:hover': {
            'background': 'grey!important',
        },
    },
    badPurchaseRow: {
        marginTop: '70px',
    },
    
    newBuyButton: {
        background: 'var(--success-color)',
        border: '1px solid var(--success-color)',
        borderColor: 'var(--success-color)!important',
        borderRadius: '3px',
        fontWeight: '500',
        color: '#fff!important',
        fontSize: '18px',
        userSelect: 'none',
        display: 'inline-block',
        height: 'auto',
        textAlign: 'center',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        padding: '9px',
        lineHeight: '100%',
        '&:hover': {
            background: 'var(--success-color-hover)!important',
            borderColor: 'var(--success-color-hover)!important',
            cursor: 'pointer',
        },
        "&:disabled": {
            opacity: .5,
            backgroundColor: "#a3e2bd",
            borderColor: "#a3e2bd!important",
            cursor: "not-allowed",
            pointerEvents: "none",
        },
        "&.tix": {
            background: 'var(--tix-color)',
            borderColor: 'var(--tix-color)!important',
            '&:hover': {
                background: 'var(--tix-color-hover)!important',
                borderColor: 'var(--tix-color-hover)!important',
            },
            "&:disabled": {
                backgroundColor: "#E3C7A1",
                borderColor: "#E3C7A1!important",
            },
        },
    },
    newContinueButton: {
        background: 'var(--primary-color)',
        border: '1px solid var(--primary-color)',
        borderColor: 'var(--primary-color)!important',
        borderRadius: '3px',
        fontWeight: '500',
        color: '#fff!important',
        fontSize: '18px',
        
        userSelect: 'none',
        display: 'inline-block',
        height: 'auto',
        textAlign: 'center',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        padding: '9px',
        lineHeight: '100%',
        '&:hover': {
            background: 'var(--primary-color-hover)!important',
            borderColor: 'var(--primary-color-hover)!important',
            color: '#fff',
            cursor: 'pointer',
        },
        '&:disabled': {
            opacity: .5,
            cursor: "not-allowed",
            pointerEvents: "none",
        },
    },
    newDisabledContinueButton: {
        background: '#99DAFF',
        border: '1px solid #99DAFF',
        borderColor: '#99DAFF!important',
        borderRadius: '3px',
        fontWeight: '500',
        color: '#fff!important',
        fontSize: '18px',
        cursor: "default",
        
        userSelect: 'none',
        display: 'inline-block',
        height: 'auto',
        textAlign: 'center',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        padding: '9px',
        lineHeight: '100%',
    },
    newCancelButton: {
        background: '#fff',
        border: '1px solid var(--text-color-secondary)',
        borderColor: 'var(--text-color-secondary)!important',
        borderRadius: '3px',
        fontWeight: '500',
        color: 'var(--text-color-primary)!important',
        fontSize: '18px',
        userSelect: 'none',
        display: 'inline-block',
        height: 'auto',
        textAlign: 'center',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        padding: '9px',
        lineHeight: '100%',
        transition: "box-shadow 200ms ease-in-out",
        "-webkit-transition": "box-shadow 200ms ease-in-out",
        '&:hover': {
            background: '#fff',
            color: '#000',
            cursor: 'pointer',
            boxShadow: '0 1px 3px rgb(150 150 150 / 74%)'
        },
        '&:visited': {
            color: 'rgba(0,0,0,.6)'
        }
    },
    newDisabledCancelButton: {
        '&, &:hover, &:visited': {
            opacity: '.5',
            backgroundColor: '#fff',
            borderColor: 'var(--text-color-secondary)!important',
            color: 'var(--text-color-secondary)!important',
            cursor: 'not-allowed',
            pointerEvents: 'none',
            boxShadow: 'none'
        }
    },
    newWarningButton: {
        background: '#D86868',
        border: '1px solid #D86868',
        borderColor: '#D86868!important',
        borderRadius: '3px',
        fontWeight: '500',
        color: '#fff!important',
        fontSize: '18px',
        
        userSelect: 'none',
        display: 'inline-block',
        height: 'auto',
        textAlign: 'center',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        padding: '9px',
        lineHeight: '100%',
        transition: "box-shadow 200ms ease-in-out",
        "-webkit-transition": "box-shadow 200ms ease-in-out",
        '&:hover': {
            background: '#E27676!important',
            borderColor: '#E27676!important',
            boxShadow: "0 1px 3px rgba(150,150,150,0.74)",
            color: '#fff',
            cursor: 'pointer',
        },
    },
});

export default useButtonStyles