import { useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import useButtonStyles from "../../../../styles/buttonStyles";
import ActionButton from "../../../actionButton";
import OldModal from "../../../oldModal";

const useStyles = createUseStyles({
  feedbackMsg: {
  },
  buttonsWrapper: {
    marginTop: '20px',
  },
});

const VoteModal = props => {
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const [error, setError] = useState(null);

  return <OldModal title={props.modalTitle || 'Error'} height={error ? 120 : 110}>
    <div className={'col-12 ' + error ? 'mt-0' : 'mt-2'}>
      <div className={s.feedbackMsg}>
        <p className='mb-0'>{props.feedbackMsg || ''}</p>
      </div>
    </div>
    <div className='col-8 offset-2'>
      <div className={s.buttonsWrapper}>
        <div className='row' style={{justifyContent: 'center', alignItems: 'center'}}>
          <div className='col-4 ps-1 pe-0'>
            <ActionButton className={buttonStyles.cancelButton} onClick={() => {
              props.onClose();
            }} label='OK'></ActionButton>
          </div>
        </div>
      </div>
    </div>
  </OldModal>
}

export default VoteModal;