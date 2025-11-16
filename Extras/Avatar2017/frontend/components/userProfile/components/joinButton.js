import React, { useEffect, useState } from "react";
import AuthenticationStore from "../../../stores/authentication";
import UserProfileStore from "../stores/UserProfileStore";
import Button from "./button";
import { createUseStyles } from "react-jss";
import useButtonStyles from "../../../styles/buttonStyles";
import { joinGame } from "../../gameDetails/components/newPlayButton";

const useStyles = createUseStyles({
  button: {
  },
})

const JoinButton = props => {
  const [error, setError] = useState(null);
  const auth = AuthenticationStore.useContainer();
  const s = useStyles();
  const s2 = useButtonStyles();
  const onClass = error ? 'on' : '';

  // TODO: check if we can message user, disable if user is not messageable
  return <>
    <div className={`alert-pjx alert-warning ${onClass}`}>
      <span className="alert-text">{error || ''}</span>
    </div>
    <Button noStyling={true} className={`${s.button} ${s2.newBuyButton}`} onClick={e => {
      joinGame(e, props.placeId, auth, setError);
    }} style={{ width: 'auto' }}>
      Join Game
    </Button>
  </>
}

export default JoinButton;