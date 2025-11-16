import React, { useState } from "react";
import { createUseStyles } from "react-jss";
import OldModal from "../../../oldModal";
import useButtonStyles from "../../../../styles/buttonStyles";
import ActionButton from "../../../actionButton";

const useStyles = createUseStyles({
    buttonRow: {
        display: 'flex',
        marginTop: '20px'
    },

    modal:{
        width: '475px',
        height: 'auto',
        aspectRatio: '612 / 385'
    },
    modalTopBody: {
        //color: 'var(--text-color-primary)',
    },
    modalMessage: {
        fontSize: '16px',
        fontWeight: '400',
        lineHeight: '1.4em',
        marginTop: '15px',
    },
    modalBtns: {
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        gap: '15px',
        margin: '6px 0 0',
        textAlign: 'center',
        '& *': {
            minWidth: '90px',
            margin: 0,
        }
    },
    button: {
        lineHeight: '2em',
        fontSize: '20px',
        padding: '4px 8px'
    },
    row: {
        margin: 0,
        display: 'flex',
        height: '100%',
    },
})

const shutdownServerModal = props => {
    const s = useStyles();
    const buttonStyles = useButtonStyles();

    return <OldModal title={props.title} rowClass={s.row} className={s.modal}>
        <div className={s.modalTopBody}>
            <div className={s.modalMessage}>{props.message}</div>
        </div>
        <div className={s.modalBtns}>
            <ActionButton label='Submit' className={`${s.button} ${buttonStyles.continueButton}`} onClick={(e) => {
                e.preventDefault();
                props.onConfirm();
            }}></ActionButton>

            <ActionButton label='Cancel' className={`${s.button} ${buttonStyles.cancelButton}`} onClick={(e) => {
                e.preventDefault();
                props.onClose();
            }}></ActionButton>
        </div>
        
    </OldModal>
}

export default shutdownServerModal;