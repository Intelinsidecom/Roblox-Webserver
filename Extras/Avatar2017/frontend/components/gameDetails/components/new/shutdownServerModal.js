import React, { useState } from "react";
import { createUseStyles } from "react-jss";
import OldModal from "../../../oldModal";
import ActionButton from "../../../actionButton";
import useButtonStyles from "../../../../styles/buttonStyles";
import { shutdownPlaceServers } from "../../../../services/games";
import NewModal from "../../../newModal";

const useStyles = createUseStyles({
    buttonRow: {
        display: 'flex',
        marginTop: '20px'
    },

    modalTopBody: {
        color: 'var(--text-color-primary)',
    },
    modalMessage: {
        fontSize: '16px',
        fontWeight: '400',
        lineHeight: '1.4em'
    },
    modalBtns: {
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        gap: '20px',
        margin: '6px 0 0',
        textAlign: 'center',
        '& *': {
            minWidth: '90px',
            margin: '0 5px'
        }
    },
})

const shutdownServerModal = props => {
    const s = useStyles();
    const buttonStyles = useButtonStyles();
    const [error, setError] = useState(null);
    const [locked, setLocked] = useState(false);

    return <NewModal title="Shut Down All Servers">
        {error && <div className='row'><div className='col-12 text-danger mb-0'>{error}</div></div>}

        <div className={s.modalTopBody}>
            <div className={s.modalMessage}>Are you sure you want to shut down all servers for this place?</div>
        </div>
        <div className={s.modalBtns}>
            <ActionButton disabled={locked} label='Yes' className={buttonStyles.buyButton} onClick={(e) => {
                e.preventDefault();
                setLocked(true);
                shutdownPlaceServers({
                    placeId: props.placeId,
                }).then(() => {
                    props.onClose();
                    //window.location.reload(); TODO: Close modal
                }).catch(e => {
                    setLocked(false);
                    setError(e.message);
                })
            }}></ActionButton>

            <ActionButton disabled={locked} label='No' className={buttonStyles.cancelButton} onClick={(e) => {
                e.preventDefault();
                props.onClose();
            }}></ActionButton>
        </div>
        
    </NewModal>

    /*return <OldModal title="Shut Down All Servers">
        {error && <div className='row'><div className='col-12 text-danger mb-0'>{error}</div></div>}

        <div className={s.modalTopBody}>
            <div className={s.modalMessage}>Are you sure you want to shut down all servers for this place?</div>
        </div>
        <div className={s.modalBtns}>
            <ActionButton disabled={locked} label='Yes' className={buttonStyles.buyButton} onClick={(e) => {
                e.preventDefault();
                setLocked(true);
                shutdownPlaceServers({
                    placeId: props.placeId,
                }).then(() => {
                    props.onClose();
                    //window.location.reload(); TODO: Close modal
                }).catch(e => {
                    setLocked(false);
                    setError(e.message);
                })
            }}></ActionButton>

            <ActionButton disabled={locked} label='No' className={buttonStyles.cancelButton} onClick={(e) => {
                e.preventDefault();
                props.onClose();
            }}></ActionButton>
        </div>
    </OldModal>*/
}

export default shutdownServerModal;