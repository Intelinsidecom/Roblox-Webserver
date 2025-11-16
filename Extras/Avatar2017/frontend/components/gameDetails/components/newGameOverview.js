import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import AuthenticationStore from "../../../stores/authentication";
import GameDetailsStore from "../stores/gameDetailsStore";
import { multiGetPlaceDetails } from "../../../services/games";

import GameThumbnails from "./new/gameThumbnails";
import { PlayButton } from "./newPlayButton";
import Follow from "./new/follow";
import Vote from "./new/vote";
import Favorite from "./new/favorite";
import Dropdown2016 from "../../dropdown2016";
import useButtonStyles from "../../../styles/buttonStyles";

import ShutdownServerModal from "./new/shutdownServerModal";

const useStyles = createUseStyles({
  background: {
    height: '384px',
    minHeight: '345px',
    padding: '12px',
    margin: '0 0 18px',
    marginBottom: 0,
    backgroundColor: 'var(--white-color)',
    lineHeight: '1.4em',
    fontSize: '16px',
    fontWeight: '400',
    '@media (max-width: 991px)': {
      display: 'flex',
      flexDirection: 'column',
      height: '100%',
    },
  },
  descriptionText: {
    whiteSpace: 'break-spaces',
  },
  thumbContainer: {
    float: 'left',
  },
  carouselGameDetails: {
    //width: '640px',
    //height: '360px',
    aspectRatio: '640/360',
    width: '640px',
    height: 'auto',
    maxWidth: '640px',
    position: 'relative',
    margin: '0 auto',
    '@media(max-width: 991px)': {
      width: '100%',
    },
    '&:hover': {
      carouselControl: {
        display: 'block'
      }
    }
  },
  carouselControl: {
    textDecoration: 'none',
    userSelect: 'none',
    transition: 'all,.2s,ease-in-out',
    backgroundColor: 'rgba(25,25,25,.3)',
    display: 'none',
    position: 'absolute',
    top: 'calc(50% - 24px)',
    height: '48px',
    width: '48px',
    border: '2px solid var(--white-color)',
    borderRadius: '50%',
    textAlign: 'center',
    zIndex: '1000'
  },
  left: {
    left: '18px'
  },
  right: {
    right: '18px'
  },


  callsToAction: {
    padding: '0 12px 0 18px',
    position: 'relative',
    width: 'calc(100% - 640px)',
    float: 'right',
    height: '100%',
    '@media (max-width: 991px)': {
      width: '100%',
      display: 'flex',
      flexDirection: 'column',
      padding: '0',
    },
  },
  playButtonContainer: {
    borderTop: '0 none',
    paddingBottom: '5px',
    textAlign: 'center',
    paddingTop: '20px',
    borderBottom: '1px solid var(--background-color)',
    '@media (max-width: 991px)': {
      paddingTop: '15px',
    },
  },
  actionButtonsContainer: {
    position: 'relative',
    width: '100%',
    paddingTop: '10px',
    listStyle: 'none',
    margin: 0,
    padding: 0,
    '& li': {
      margin: 0,
      padding: 0,
      listStyle: 'none'
    }
  },
  votingSection: {
    width: '110px',
    bottom: '-24px',
    float: 'left',
    right: 0,
    position: 'relative',
  },
  buttonSection: {
    float: 'left',
    width: 'calc(((100% - 110px) / 2) - 1px)',
    overflow: 'hidden',
    whiteSpace: 'nowrap'
  },

  gameButtonsContainer: {
    padding: '0 12px 0 18px',
    width: '100%',
    position: 'absolute',
    bottom: 0,
    right: 0,
  },


  buttonsContainer: {
    padding: '0 12px 0 18px',
    width: '100%',
    position: 'absolute',
    bottom: 0,
    right: 0,
    '@media (max-width: 991px)': {
      position: 'relative',
      padding: 0,
    },
  },
  gameTitleContainer: {
    padding: '0 12px 0 0',
    overflow: 'hidden',
    maxHeight: '100%',
    '@media (max-width: 991px)': {
      padding: 0,
    },
  },
  gameName: {
    overflow: 'hidden',
    fontSize: '32px',
    fontWeight: '800',
    lineHeight: '1em',
    margin: 0,
    padding: '5px 0',
    wordWrap: 'break-word',
    whiteSpace: 'pre-wrap',
  },
  gameCreator: {
    fontSize: '16px',
    fontWeight: '400',
    lineHeight: '1.4em'
  },
  creatorLabel: {
    fontSize: '16px',
    fontWeight: '500',
    color: 'var(--text-color-secondary)',
    display: 'inline',
  },
  creatorName: {
    fontSize: '16px',
    fontWeight: '500',
    color: 'var(--primary-color)',
    textDecoration: 'none',
    display: 'inline',
    '&:link': { textDecoration: 'none!important' },
    '&:visited': { textDecoration: 'none!important' },
    '&:hover': { textDecoration: 'underline!important', color: 'var(--primary-color)' },
    '&:active': { textDecoration: 'underline!important', color: 'var(--primary-color)' },
  },
  dropdownContainer: {
    right: 0,
    top: '6px',
    position: 'absolute',
    zIndex: '2',
    '@media (max-width: 991px)': {
      right: '16px',
    },
  },
  verifiedIcon: {
    position: 'relative',
    bottom: '2.5px',
    width: '16px', 
    height: '16px',
    left: '5px'
  },
})

const About = props => {
  const store = GameDetailsStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const s = useStyles();
  const creatorName = store.details.creatorName;
  const gameName = store.details.name;
  const isVerified = store.universeDetails?.creator?.hasVerifiedBadge ?? false;
  const creatorType = store.details.creatorType;
  const creatorId = store.details.creatorTargetId;
  const placeId = store.details.id;
  const [universeId, setUniverseId] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const url = (creatorType === 'User' || creatorType === 1) ? '/User.aspx?ID=' + creatorId : '/My/Groups.aspx?gid=' + creatorId;
  const showDropdown = store.details.creatorType === 'User' && store.details.creatorTargetId === auth.userId;

  useEffect(async () => {
    try {
      multiGetPlaceDetails({ placeIds: [placeId] }).then(async data => {
        let uniId = await data[0].universeId;
        setUniverseId(uniId);
      }, () => { })
    } catch (error) {
      console.error('Could not get universe id! Error: ' + error);
    }
  }, []);

  const dropdownOptions = [
    {
      name: 'Edit in Studio',
      url: '#',
    },
    {
      name: 'Sponsor this Game',
      url: `/My/CreateUserAd.aspx?targetId=${placeId}&targetType=asset`
    },
    {
      name: 'Configure this Place',
      url: `/places/${placeId}/update`
    },
    {
      name: 'Configure this Universe',
      url: `/universes/configure?id=${universeId}`
    },
    {
      name: 'Shut Down All Servers',
      onClick: (e) => {
        e.preventDefault();
        setModalOpen(true);
      }
    },
    {
      name: 'Download',
      onClick: (e) => {
        e.preventDefault();
        const a = document.createElement("a");
        a.href = `/asset/?id=${placeId}`;
        a.download = `${gameName}.rbxl`
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
      }
    }
  ]

  return <div className={s.background}>
    {modalOpen && <ShutdownServerModal placeId={placeId} onClose={() => {
      setModalOpen(false);
    }} />}
    <div className={s.thumbContainer}>
      <div className={s.carouselGameDetails}>
        <GameThumbnails />
      </div>
    </div>
    <div className={s.callsToAction}>
      {showDropdown && <div className={s.dropdownContainer}>
        <Dropdown2016 options={dropdownOptions} />
      </div>}
      <div className={s.gameTitleContainer}>
        <h2 className={s.gameName} title={gameName}>{gameName}</h2>
        <div className={s.gameCreator}>
          <span className={s.creatorLabel}>By </span>
          <a href={url} className={s.creatorName} title={creatorName}>
            {creatorName}
          </a>
          {isVerified && (
            <img
              src='/img/verified.svg'
              alt='Verified'
              className={s.verifiedIcon}
            />
          )}
        </div>
      </div>

      <div className={s.buttonsContainer}>
        <div className={s.playButtonContainer}>
          <PlayButton placeId={store.details.id}></PlayButton>
        </div>
        <ul className={s.actionButtonsContainer}>
          <li className={s.buttonSection}>
            <Favorite assetId={store.details.id} favoriteCount={0}></Favorite>
          </li>
          <li className={s.buttonSection}>
            <Follow />
          </li>
          <li className={s.votingSection}>
            <Vote />
          </li>
        </ul>
      </div>
    </div>
  </div>
}

export default About;