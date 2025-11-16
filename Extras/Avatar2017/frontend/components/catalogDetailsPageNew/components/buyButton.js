import { useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import { launchGame } from "../../../services/games";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import { decode } from "jsonwebtoken";

const useStyles = createUseStyles({
    buttonWrapper: {
        maxWidth: '190px',
        float: 'right',
    },
    button: {
        width: '100%',
    },
    innerButton: {
        fontWeight: 500,
        padding: '15px',
        borderRadius: '5px',
        fontSize: '21px',
        width: '180px',
        lineHeight: '100%',
        height: 'auto',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        textAlign: 'center',
    }
})

/**
 * Buy button
 * @param {{onClick: any; disabled: boolean; isRobux: boolean; }} props 
 * @returns 
 */
const BuyButton = props => {
    const s = useStyles();
    const buttonStyles = useButtonStyles();

    const bgColor = props.isRobux === true ? '#CC9E71' : '#02b757'
    return (
        <div className={`${s.buttonWrapper} `}>
            <ActionButton label='Buy' style={{backgroundColor: bgColor}} disabled={props.disabled} className={s.innerButton} divClassName={s.button} buttonStyle={buttonStyles.newBuyButton} onClick={props.onClick} />
        </div>
    );
}

export default BuyButton;
