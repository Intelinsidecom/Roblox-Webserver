import { useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import { launchGame } from "../../../services/games";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import { decode } from "jsonwebtoken";
import RobloxLoadingModal from "./gameModals/robloxLoadingModal";

const useStyles = createUseStyles({
  buttonWrapper: {
    width: '100%',
  },
  button: {
    width: '100%',
    paddingTop: '2px',
    paddingBottom: '4px',
    borderRadius: '5px',
  },
  modalContainer: {
    position: 'fixed',
    zIndex: 3453,
    height: '322px',
    width: '400px',
    top: '50%',
    left: '50%',
    boxSizing: 'content-box'
  },
  iconPlay: {
    backgroundSize: '72px auto',
    width: '36px',
    height: '36px',
    backgroundPosition: '0 0',
    backgroundImage: 'url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNzIiIGhlaWdodD0iNzIiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiPjxkZWZzPjxwYXRoIGQ9Ik0xOSA4LjVsMTEuOTI1IDE2LjAzNmMuODUxIDEgLjc0NSAyLjUxMi0uMjM3IDMuMzc5YTIuMzMgMi4zMyAwIDAgMS0xLjU0Mi41ODVIOC44NTRjLTEuMyAwLTIuMzU0LTEuMDczLTIuMzU0LTIuMzk1IDAtLjU3Ny4yMDQtMS4xMzQuNTc1LTEuNTdMMTkgOC41eiIgaWQ9ImEiLz48L2RlZnM+PGcgZmlsbD0ibm9uZSIgZmlsbC1ydWxlPSJldmVub2RkIj48dXNlIGZpbGw9IiNGRkYiIHRyYW5zZm9ybT0icm90YXRlKDkwIDE5IDE4LjUpIiB4bGluazpocmVmPSIjYSIvPjxwYXRoIGQ9Ik0xOCA3MkM4LjA1OSA3MiAwIDYzLjk0MSAwIDU0czguMDU5LTE4IDE4LTE4IDE4IDguMDU5IDE4IDE4LTguMDU5IDE4LTE4IDE4em03LjYtMTcuNjk1bC05LjQ2LTcuMTZhMS40MTMgMS40MTMgMCAwIDAtMi4zNCAxLjA2OHYxMi4xODRhMS40MTMgMS40MTMgMCAwIDAgMi4zNCAxLjA2OGw5LjQ2LTcuMTZ6IiBmaWxsLW9wYWNpdHk9Ii45IiBmaWxsPSIjRkZGIi8+PC9nPjwvc3ZnPg==)',
    backgroundRepeat: 'no-repeat',
    display: 'inline-block',
    verticalAlign: 'middle'
  },
  btn: {
    minWidth: '100%',
    width: '100%',
    padding: '4px 16px',
    backgroundColor: '#02b757',
    borderColor: '#02b757',
    '&:hover': {
      backgroundColor: '#3fc679',
      borderColor: '#3fc679',
    },
  },
  actionBtn: {
    borderColor: '#02b757',
    borderRadius: '5px',
    width: '100%',
    padding: '1px 13px 3px 13px',
    '&:hover': {
      borderColor: '#3fc679'
    }
  },
})

export const joinGame = (e, placeId, auth, setError) => {
  if (navigator.userAgent.includes("ROBLOX Android App") || navigator.userAgent.includes("ROBLOX iOS App")) {
    window.location.href = 'games/start?placeid=' + placeId;
  } else if (getFlag('launchUsingEsURI', false)) {
    if (!auth.isAuthenticated) {
      window.location.href = '/Login';
      return;
    }
    e && e.preventDefault && e.preventDefault();
    //modalActivate(); should be done elsewhere
    launchGame({
      placeId: placeId,
    }).catch(e => {
      // todo: modal ????
      setError(e.message);
    });
  } else if (getFlag('launchUsingEsWeb', false)) {
    window.location.href = '/RobloxApp/Play?placeId=' + placeId;
  } else {
    // TODO: Roblox URI handling here (is this even possible?)
    alert('Support for joining ROBLOX games is not implemented. You will be redirected to ROBLOX to play this game.');
    window.location.href = 'https://www.roblox.com/games/' + placeId + '/--';
  }
}

/**
 * Play button
 * @param {{placeId: number}} props 
 * @returns 
 */
export const PlayButton = props => {
  const [error, setError] = useState(null);
  const auth = AuthenticationStore.useContainer();
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const [isModalClosed, setModalClosed] = useState(true);
  const playSpan = <span className={useStyles().iconPlay}></span>;

  return (
    <div className='row'>
      <div className={'col-12 mx-auto ' + s.buttonWrapper}>
        {error && <p className='text-danger mb-1 mt-1'>{error}</p>}
        {!isModalClosed ? <RobloxLoadingModal closeModals={isModalClosed} exitFunction={() => setModalClosed(true)} /> : null}
        <ActionButton className={s.actionBtn} label={playSpan} divClassName={s.button} buttonStyle={buttonStyles.newBuyButton} onClick={(e) => {
          joinGame(e, props.placeId, auth, setError);
          if (isModalClosed === false) { return } // if modal already exists
          setModalClosed(false);
        }}></ActionButton>
      </div>
    </div>
  );
}