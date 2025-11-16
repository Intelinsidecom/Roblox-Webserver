import React, { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { followUser, unfollowUser } from "../../../services/friends";
import { multiGetPresence } from "../../../services/presence";
import { getMembershipType, updateStatus } from "../../../services/users";
import AuthenticationStore from "../../../stores/authentication";
import Dropdown2016 from "../../dropdown2016";
import PlayerHeadshot from "../../playerHeadshot";
import Activity from "../../userActivity";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";
import FriendButton from "./friendButton";
import MessageButton from "./messageButton";
import RelationshipStatistics from "./relationshipStatistics";
import RAPStats from "./RAPStats";
import ChatButton from "./chatButton";
import JoinButton from "./joinButton";

const useHeaderStyles = createUseStyles({
  iconWrapper: {
    border: '1px solid #B8B8B8',
    margin: '0 auto',
    maxWidth: '110px',
  },
  username: {
    fontSize: '32px',
    fontWeight: 800,
    margin: '0',
    '@media(max-width: 767px)': {
      display: 'flex',
      justifyContent: 'center',
      position: 'relative',
    },
  },
  userStatus: {
    margin: 0,
    fontSize: '16px',
    fontWeight: 400,
    //marginTop: '5px',
    lineHeight: '1.4em',
    '@media(max-width: 767px)': {
      textAlign: 'center',
    },
  },
  userStatusInvisible: {
    '@media(max-width: 767px)': {
      display: 'none',
    },
  },
  userStatusMargined: {
    marginBottom: '8px',
  },
  dropdown: {
    float: 'right',
    fontWeight: '400',
    display: 'flex',
    '@media(max-width: 767px)': {
      position: 'absolute',
      right: 0,
    },
  },
  dropdownWrapper: {
    display: 'flex',
  },
  dropdownClass: {
    marginTop: '30px',
  },
  updateStatusInput: {
    width: 'calc(100% - 140px)',
    border: '1px solid #c3c3c3',
    borderRadius: '4px',
    '@media(max-width: 992px)': {
      width: '100%',
    }
  },
  bcIcon: {
    position: 'relative',
    bottom: '4px',
    '@media(max-width: 767px)': {
      bottom: '-3px',
      marginLeft: '3px',
    },
  },
  verifiedIcon: {
    position: 'relative',
    bottom: '2.5px',
  },
  updateStatusButton: {
    cursor: 'pointer',
    marginTop: '2px',
    fontSize: '12px',
  },
  activityWrapper: {
    //position: "relative",
    float: 'right',
    //marginRight: '6px',
    //marginTop: '-32px',
    margin: 0,
    position: 'absolute',
    bottom: '4%',
    right: '5%',
  },
  profileHeaderContainer: {
    marginBottom: '0px',
    '& h1, & h2, & h3, & h4, & h5': {
      padding: '5px 0',
      lineHeight: '1em'
    }
  },
  cardBody: { padding: '15px!important' },
  thumbnailContainer: {
    padding: 0,
    float: 'left',
    '@media(max-width: 767px)': {
      alignItems: 'center',
      justifyContent: 'center',
      display: 'flex',
    },
  },
  userInfoContainer: {
    width: 'calc(100% - 128px - 12px)!important',
    '@media(max-width: 767px)': {
      display: 'flex',
      flexDirection: 'column',
      width: '100%!important',
    },
  },
  avatarHeadshotContainer: {
    position: 'relative',
    marginRight: '12px',
    width: '128px',
    height: '128px',
    border: '1px solid #b8b8b8',
    backgroundColor: '#d1d1d1',
    borderRadius: '50%',
    verticalAlign: 'bottom',
    padding: 0,
    '@media(max-width: 767px)': {
      height: '100px',
      width: '100px',
      margin: 0,
    },
  },
  image: {
    borderRadius: '50%',
    verticalAlign: 'bottom',
  },
  
  relationshipContainer: {
    justifyContent: 'space-between',
    '@media(max-width: 767px)': {
      marginTop: '6px',
    },
  },
  relationshipList: {
    float: 'left',
    height: '54px',
    listStyle: 'none',
    width: '55%',
    margin: 0,
    padding: 0,
    '@media(max-width: 767px)': {
      width: '100%',
    },
  },
  
  actionContainer: {
    margin: 0,
    padding: 0,
    float: 'right',
    display: 'flex',
    justifyContent: 'flex-end',
    alignItems: 'center',
    width: "45%",
    '@media(max-width: 767px)': {
      width: '100%',
      flexDirection: 'row',
      justifyContent: 'center',
      marginTop: '11px',
    },
  },
  buttonContainer: {
    padding: '0 5px',
    float: 'right',
  },
  
  container: {
    '@media(max-width: 767px)': {
      flexDirection: "column",
    },
  },
});

const ProfileHeader = props => {
  const auth = AuthenticationStore.useContainer();
  const store = UserProfileStore.useContainer();
  
  const statusInput = useRef(null);
  
  const [dropdownOptions, setDropdownOptions] = useState(null);
  const [editStatus, setEditStatus] = useState(false);
  const [status, setStatus] = useState(null);
  const [bcLevel, setBcLevel] = useState(0);
  const [verified, setVerified] = useState(false);

  useEffect(() => {
    // reset
    setStatus(null);
    setBcLevel(0);
    setEditStatus(false);
    setDropdownOptions(null);
    setVerified(false);
  }, [store.userId]);
  
  useEffect(() => {
    if (auth.isPending) return;
    
    multiGetPresence({ userIds: [store.userId] }).then((d) => {
      setStatus(d[0]);
    });
    getMembershipType({ userId: store.userId }).then(d => {
      if (d === 4) {
        d = 3
      }
      setBcLevel(d);
    }).catch(e => {
      // can fail when not logged in :(
    })
    setVerified(store.userInfo.hasVerifiedBadge);
    const buttons = [];
    // Don't make it ===, because store.userId is a string and auth.userId is a number
    const isOwnProfile = auth.userId == store.userId;
    if (store.friendStatus === "Friends" || status && status?.userPresenceType === "InGame") {
      buttons.push({
        name: 'Message',
        onClick: (e) => {
          e.preventDefault();
          window.location.href = "/messages/compose?recipientId=" + store.userId;
        }
      });
    }
    if (isOwnProfile) {
      // Exclusive to your own profile
      buttons.push({
        name: 'Update Status',
        onClick: e => {
          e.preventDefault();
          setEditStatus(!editStatus);
        },
      })
    } else {
      // Exclusive to profiles other than your own
      buttons.push({
        name: store.isFollowing ? 'Unfollow' : 'Follow',
        onClick: (e) => {
          e.preventDefault();
          store.setIsFollowing(!store.isFollowing);
          if (store.isFollowing) {
            store.setFollowersCount(store.followersCount - 1);
            unfollowUser({ userId: store.userId });
          } else {
            store.setFollowersCount(store.followersCount + 1);
            followUser({ userId: store.userId });
          }
        },
      });
    }
    // TODO: wait for accountsettings to add "get blocked status" endpoint
    /*
    arr.push({
      name: 'Block User',
      onClick: () => {
        console.log('Block user!');
      },
    });
    */
    buttons.push({
      name: 'Inventory',
      url: `/users/${store.userId}/inventory`,
    });
    buttons.push({
      name: 'View RAP',
      url: `/internal/collectibles?userId=${store.userId}`,
    });
    if (!isOwnProfile) {
      buttons.push({
        name: 'Trade',
        onClick: (e) => {
          e.preventDefault();
          window.open("/Trade/TradeWindow.aspx?TradePartnerID=" + store.userId, "_blank", "scrollbars=0, height=608, width=914");
        }
      });
    }
    setDropdownOptions(buttons);
  }, [auth.userId, auth.isPending, store.isFollowing, editStatus, store.userId]);
  
  const s = useHeaderStyles();
  const cardStyles = useCardStyles();
  const statusClass = status ? s.userStatusMargined : '';
  
  const showButtons = auth.userId != store.userId && !auth.isPending;
  
  const BcIcon = () => {
    if (bcLevel === 0) {
      return null;
    }
    // If the user is verified we should overwrite the bcIcon with the verified icon
    if (verified) {
      return <span className={`icon-verified ${s.verifiedIcon}`} />
    }

    // 1 = BC
    // 2 = TBC
    // 3 = OBC
    // 4 = Premium
    // 0 = None
    switch (bcLevel) {
      case 1:
      case 4:
        return <span className={`icon-bc ${s.bcIcon}`} />
      case 2:
        return <span className={`icon-tbc ${s.bcIcon}`} />
      case 3:
        return <span className={`icon-obc ${s.bcIcon}`} />
      default:
        return null;
    }
  }
  
  return <div className={`flex ${s.profileHeaderContainer}`}>
    <div className='col-12'>
      <div className={`card ${cardStyles.card}`}>
        <div className={`card-body ${s.cardBody}`}>
          <div className={`flex ${s.container}`}>
            {/*<div className='col-12 col-lg-2 pe-0'>
              <div className={s.iconWrapper}>
                <PlayerHeadshot id={store.userId} name={store.username}/>
                {status && <div className={s.activityWrapper}><Activity relative={false} {...status}></Activity></div>}
              </div>
            </div>*/}
            <div className={`${s.thumbnailContainer}`}>
              <div className={s.avatarHeadshotContainer}>
                <PlayerHeadshot className={s.image} id={store.userId} name={store.username} />
                {status && <div className={s.activityWrapper}><Activity relative={false} {...status}></Activity></div>}
              </div>
            </div>
            <div className={`col-12 col-lg-10 ps-0 ${s.userInfoContainer}`}>
              <h2 className={s.username}>{store.username} {<BcIcon />} <div className={s.dropdown}>
                {dropdownOptions && <Dropdown2016 options={dropdownOptions} dropdownClass={s.dropdownClass} wrapperClass={s.dropdownWrapper} />}
              </div></h2>
              {editStatus ? <div>
                <input ref={statusInput} type='text' className={s.updateStatusInput} maxLength={255} defaultValue={store.status?.status || ''} />
                <p className={s.updateStatusButton} onClick={() => {
                  let v = statusInput.current.value;
                  store.setStatus({
                    status: v,
                  });
                  setEditStatus(false);
                  updateStatus({
                    newStatus: v,
                    userId: auth.userId,
                  })
                }}>Update Status</p>
              </div> : (!store.status || !store.status.status) ? <p className={`${statusClass} ${s.userStatus} ${s.userStatusInvisible}`}>&emsp;</p> : <p className={s.userStatus}>&quot;{store.status.status}&quot;</p>
              }
              <div className={`flex ${s.relationshipContainer}`}>
                <ul className={`${s.relationshipList}`}>
                  <RelationshipStatistics id='friends' label='Friends' value={store.friends?.length} userId={store.userId} />
                  <RelationshipStatistics id='followers' label='Followers' value={store.followersCount} userId={store.userId} />
                  <RelationshipStatistics id='followings' label='Following' value={store.followingsCount} userId={store.userId} />
                  <RelationshipStatistics id='rap' label='RAP' value={store.userInfo.inventory_rap} userId={store.userId} />
                </ul>
                {/*
                  showButtons && <>
                    <div className='col-6 col-lg-2 offset-lg-2 pe-1'>
                      <MessageButton />
                    </div>
                    <div className='col-6 col-lg-2 ps-1'>
                      <FriendButton />
                    </div>
                  </>
                */}
                {
                    showButtons && <ul className={s.actionContainer}>
                      
                      <div style={{ order: '4' }} className={s.buttonContainer}>
                        <FriendButton />
                      </div>
                      
                      {(store.friendStatus != "Friends" || status && status?.userPresenceType !== "InGame") &&
                          <div style={{ order: '3' }} className={s.buttonContainer}>
                            <MessageButton />
                          </div>
                      }
                      {store.friendStatus == "Friends" &&
                          <div style={{ order: '2' }} className={s.buttonContainer}>
                            <ChatButton />
                          </div>
                      }
                      {status && status?.userPresenceType === "InGame" &&
                          <div style={{ order: '1' }} className={s.buttonContainer}>
                            <JoinButton placeId={status.rootPlaceId} />
                          </div>
                      }
                    </ul>
                }
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div >
}

export default ProfileHeader;