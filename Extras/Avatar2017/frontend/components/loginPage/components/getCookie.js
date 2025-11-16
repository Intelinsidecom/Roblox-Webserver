import styles from './getCookie.module.css';
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";

const GetCookie = props => {
  const {setVisible} = props;
  const btn = useButtonStyles();

  return <div className={styles.modal}>
    <div className='container'>
      <div className='row'>
        <div className='col-12 col-lg-8 offset-lg-2'>
          <div className='card card-body'>
            <ActionButton label='Dismiss' className={btn.cancelButton + ' w-auto'} onClick={() => {
              setVisible(false);
            }} />
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default GetCookie;