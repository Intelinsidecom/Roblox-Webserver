import React from "react";
import { acceptFriendRequest, sendFriendRequest, unfriendUser } from "../../../services/friends";
import AuthenticationStore from "../../../stores/authentication";
import UserProfileStore from "../stores/UserProfileStore";
import Button from "./button";
import { createUseStyles } from "react-jss";
import useButtonStyles from "../../../styles/buttonStyles";

const useStyles = createUseStyles({
  buttonHover: {
    '&:hover':{
      border: '1px solid red!important',
      color: 'red!important'
    }
  }
})

const FriendButton = props => {
  const s = useButtonStyles()
  const s2 = useStyles()
  const store = UserProfileStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const isAlreadyFriend = store.friends && store.friends.find(v => v.id === auth.userId) !== undefined;
  const isOwnProfile = auth.userId == store.userId;
  const isAuthenticated = auth.userId !== null;
  const isRequestSent = store.friendStatus === 'RequestSent';
  const isRequestRecieved = store.friendStatus === 'RequestReceived';

  const text = isAlreadyFriend ? 'Unfriend' : isRequestRecieved ? 'Accept' : isRequestSent ? 'Pending' : 'Add Friend';
  const canFriend = isAlreadyFriend || !isOwnProfile && isAuthenticated && !isRequestSent || isRequestRecieved;
  const disabledClass = store.friendStatus == 'RequestSent' ? s.newDisabledCancelButton : null
  const hoverClass = isAlreadyFriend ? s2.buttonHover : null

  return <Button className={` ${disabledClass} ${hoverClass}`} disabled={!canFriend} 
  //style={isAlreadyFriend && { '&:hover': { border: '1px solid red', color: 'red' } } || undefined} 
  onClick={(e) => {
    e.preventDefault();
    if (isRequestRecieved) {
      // Accept request
      acceptFriendRequest({ userId: store.userId }).then(() => {
        store.setFriends([...store.friends, {
          id: auth.userId,
          name: auth.username,
        }]);
        store.setFriendStatus('Friends');
      })
    } else if (isAlreadyFriend) {
      // Unfriend
      unfriendUser({ userId: store.userId }).then(() => {
        store.setFriends(store.friends.filter(v => {
          return v.id !== auth.userId;
        }));
        store.setFriendStatus('NotFriends');
      })
    } else {
      // Send request
      sendFriendRequest({ userId: store.userId }).then(() => {
        store.setFriendStatus('RequestSent')
      })
    }
  }}>
    {text}
  </Button>
}

export default FriendButton;