import React from "react";
import AuthenticationStore from "../../../stores/authentication";
import UserProfileStore from "../stores/UserProfileStore";
import Button from "./button";
import { createUseStyles } from "react-jss";
import useButtonStyles from "../../../styles/buttonStyles";

const useStyles = createUseStyles({
  
})

const ChatButton = props => {
  const store = UserProfileStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const s = useStyles()
  const s2 = useButtonStyles()
  // TODO: check if we can message user, disable if user is not messageable
  return <Button className={` ${s2.newDisabledCancelButton}`} style={{ width: 'auto' }}>
    Chat
  </Button>
}

export default ChatButton;