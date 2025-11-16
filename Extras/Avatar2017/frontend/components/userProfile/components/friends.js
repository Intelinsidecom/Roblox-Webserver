import React from "react";
import { createUseStyles } from "react-jss";
import PlayerHeadshot from "../../playerHeadshot";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";
import SmallButtonLink from "./smallButtonLink";
import Subtitle from "./subtitle"
import Link from "../../link";
import SmallTextLink from "./smallTextLink";
import useButtonWrapperStyle from '../styles/buttonWrapper'
import PlayerImage from "../../playerImage";

const useFriendStyles = createUseStyles({
  friendCol: {
    width: '12%',
    paddingLeft: 0,
    paddingRight: 0,
    minWidth: '100px',
  },
  imageWrapper: {
    border: '1px solid var(--text-color-quinary)',
  },
  username: {
    fontSize: '16px',
    marginBottom: 0,
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    fontWeight: 300,
    color: '#666',
  },
  cardWrapper: {
    margin: '0 auto',
    paddingLeft: '8px',
    paddingRight: '8px',
  },
  sideRow: {
    //flexFlow: 'row',
    //overflow: 'auto',
    maxHeight: '150px',
    overflowX: 'auto',
    overflowY: 'hidden',
    display: 'flex',
    flexDirection: 'row',
    flexFlow: 'row',
    justifyContent: 'flex-start',
    alignItems: 'center',
    padding: '15px!important',
    marginBottom: '0',
  },

  listItemFriend: {
    width: '11.11111%',
    height: '120px',
    position: 'relative',
    float: 'left',
    listStyle: 'none',
    minWidth: '100px',
  },
  avatarContainer: {
    margin: 'auto',
    fontSize: '16px',
    fontWeight: '400',
    lineHeight: '1.4em',
    width: '90px',
    position: 'relative',
  },
  friendLink: {
    margin: '0 auto',
    position: 'relative',
    textDecoration: 'none',
  },
  avatarWrapper: {
    backgroundColor: '#d1d1d1',
    textAlign: 'center',
    border: 0,
    borderRadius: '50%',
    width: '100%',
    height: '100%',
    display: 'block',
    transition: "box-shadow 200ms ease",
    boxShadow: "0 1px 4px 0 rgba(25,25,25,0.3)",
    //margin: '3px auto',
    "&:hover": {
      boxShadow: "0 1px 6px 0 rgba(25, 25, 25, 0.75)",
    },
  },
  playerName: {
    margin: '3px 0 0',
    textAlign: 'center',
    fontSize: '12px',
    fontWeight: '500',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
    textDecoration: 'none',
    color: '#000',
    display: 'block',
    lineHeight: '1.867em',
    '&:hover': {
      color: 'var(--primary-color)'
    }
  },
  image: {
    borderRadius: '50%',
    verticalAlign: 'bottom',
    backgroundColor: '#d1d1d1',
  },
})

const Friends = props => {
  const store = UserProfileStore.useContainer();
  const cardStyles = useCardStyles();
  const s = useFriendStyles();
  const s2 = useButtonWrapperStyle();
  /*if (!store.friends || store.friends.length === 0) {
    return null;
  }*/
  return <div className='flex'>
    <div className='col-10'>
      <Subtitle>Friends ({store.friends?.length || 0})</Subtitle>
    </div>
    <div className='col-2'>
      <div className={`${s2.buttonWrapper}`}>
        <SmallTextLink href={`/users/${store.userId}/friends`}>See All</SmallTextLink>
      </div>
    </div>
    <div className='col-12'>
      <div className={`marginStuff ${cardStyles.card}`}>
        <ul className={'flex pt-3 pb-3 pe-3 ps-3 me-0 ms-0 ' + s.sideRow}>
          {
            store.friends && store.friends.slice(0, 10).map(v => {
              return <li className={s.listItemFriend}>
                <div className={s.avatarContainer}>
                  <Link href={`/users/${v.id}/profile`}>
                    <a className={s.friendLink}>
                      <span className={s.avatarWrapper}>
                        <PlayerImage className={s.image} id={v.id} name={s.username} />
                      </span>
                      <span className={s.playerName}>{v.name}</span>
                    </a>
                  </Link>
                </div>
              </li>

              /*<div className={`col-1 ${s.friendCol}`} key={v.id}>
                <div className={s.cardWrapper}>
                  <Link href={`/users/${v.id}/profile`}>
                    <a>
                      <div className={s.imageWrapper}>
                        <PlayerHeadshot id={v.id} name={s.username}/>
                      </div>
                      <p className={s.username}>{v.name}</p>
                    </a>
                  </Link>
                </div>
              </div>*/
            })
          }
        </ul>
      </div>
    </div>
  </div>
}

export default Friends;