// Most of this is just guess work. I don't know what the server list looked like, and I can't find any photos/videos.
import { createUseStyles } from "react-jss";
import { getServers, shutdownSpecificServer } from "../../../../services/games";
import { launchGameFromJobId } from "../../../../services/games";
import useButtonStyles from "../../../../styles/buttonStyles";
import ActionButton from "../../../actionButton";
import CreatorLink from "../../../creatorLink";
import PlayerImage from "../../../playerImage";
import GameDetailsStore from "../../stores/gameDetailsStore";
import { useEffect, useState } from "react";
import { getFriends } from "../../../../services/friends";
import AuthenticationStore from "../../../../stores/authentication";
import PlayerHeadshot from "../../../playerHeadshot";
import Link from "../../../link";
import Dropdown2016 from "../../../dropdown2016";

const useEntryStyles = createUseStyles({
  serverContainer: {
    display: 'flex',
    flexDirection: 'row',
    padding: '12px',
    background: 'var(--white-color)',
    marginBottom: '6px',
    '@media (max-width: 576px)': {
      flexDirection: 'column',
      position: 'relative',
    },
  },
  callsToAction: {
    borderRight: '1px solid var(--text-color-secondary)',
    paddingRight: '12px',
    float: 'left',
    width: '20%',
    '@media (max-width: 576px)': {
      width: '100%',
      padding: '0 0 12px',
      borderRight: '0',
      borderBottom: '1px solid var(--text-color-secondary)',
    },
  },
  gameStatus: {
    fontSize: '16px',
    fontWeight: '400',
    lineHeight: '1.4em',
    color: 'var(--text-color-tertiary)',
  },
  joinButton: {
    width: '100%',
    fontSize: '14px',
    margin: '12px auto 0',
    padding: '4px',
  },
  playersContainer: {
    display: 'flex',
    flexDirection: 'row',
    justifyContent: 'start',
    alignItems: 'center',
    flexWrap: 'wrap',
    gap: '6px',
    paddingLeft: '6px',
    width: '75%',
    '@media (max-width: 576px)': {
      width: '100%',
      padding: 0,
      paddingTop: '6px',
    },
  },
  imageContainer: {
    // could also be width auto and height 100% but idk
    aspectRatio: '1/1',
    borderRadius: '100%',
    backgroundColor: '#d1d1d1',
    width: '48px',
    height: '48px',
    overflow: 'hidden',
  },
  image: {
    verticalAlign: 'middle',
    overflow: 'clip',
    height: '100%',
    width: '100%',
    aspectRatio: '1/1',
    cursor: 'pointer',
  },
  dropdownContainer: {
    width: '5%',
    '@media (max-width: 576px)': {
      position: 'absolute',
      // account for padding
      top: '12px',
      right: '12px',
    },
  },
})

const ServerEntry = props => {
  const store = GameDetailsStore.useContainer();
  const s = useEntryStyles();
  const buttonStyles = useButtonStyles();
  const [error, setError] = useState(null);
  const [feedbackType, setFeedbackType] = useState('alert-warning');
  const [feedbackVisible, setFeedbackVisible] = useState(false);
  const onClass = feedbackVisible ? 'on' : ''

  const onJoinButtonClick = () => {
    launchGameFromJobId({
      placeId: props.placeId,
      jobId: props.jobId
    }).catch(e => {
      // todo: modal
      setError(e.message);
      setFeedbackType('alert-warning');
      setFeedbackVisible(true);
    });
  }

  useEffect(() => {
    if (error !== null && feedbackVisible == true) {
      new Promise((res, rej) => {
        setTimeout(() => {
          setFeedbackVisible(false);
        }, 4000)
      })
    }
  }, [feedbackVisible, error])

  return <div className={s.serverContainer}>
    <div className={`alert-pjx ${feedbackType} ${onClass}`}>
      <span className="alert-text">{error || ''}</span>
    </div>
    <div className={s.callsToAction}>
      <div className={s.gameStatus}>{props.PlayersCapacity} players max</div>
      <ActionButton label='Join' buttonStyle={buttonStyles.newCancelButton} className={s.joinButton} onClick={onJoinButtonClick} />
    </div>
    <div className={s.playersContainer}>
      {
        props.CurrentPlayers.map(v => {
          //<CreatorLink id={v.Id} name={v.Username} type='User'></CreatorLink>
          //<PlayerImage id={v.Id} name={v.Username}></PlayerImage>
          return <span className={s.imageContainer}>
            <Link href={`/users/${v.Id}/profile`}>
              <a href={`/users/${v.Id}/profile`}>
                <PlayerHeadshot id={v.Id} name={v.Username} className={s.image} />
              </a>
            </Link>
          </span>
        })
      }
    </div>
    <div className={s.dropdownContainer}>
      {
        props.ShowShutdownButton &&
        <Dropdown2016 options={[
          {
            name: 'Shut Down This Server',
            onClick: () => {
              shutdownSpecificServer({ placeId: props.placeId, jobId: props.jobId }).then(() => {
                setFeedbackType('alert-success');
                setError('Server shutdown successfully!');
                setFeedbackVisible(true);
                store.setServers(null);
              }).catch(error => {
                setFeedbackType('alert-warning');
                setError('Server could not be shutdown: ' + error);
                setFeedbackVisible(true);
                //store.setServers(null);
              })
            },
          }
        ]} />}
    </div>
  </div>
  /*return <div className='row'>
    <div className='col-12 col-lg-2'>
      <ActionButton label='Join' className={buttonStyles.continueButton + ' pt-1 pb-1'} onClick={() => {
        launchGameFromJobId({
          placeId: props.placeId,
          jobId: props.jobId
        }).catch(e => {
          // todo: modal
          setError(e.message);
        });
      }}></ActionButton>
    </div>
    <div className='col-12 col-lg-10'>
      <p className='mb-0 font-size-15 mt-1'>
        <span>
          <span className='fw-600'>Player Count: </span>{props.CurrentPlayers.length} / {store.universeDetails.maxPlayers} Players
        </span>
        <span>
          <span className='fw-600 ps-4'>Ping: </span> {props.Ping}
        </span>
      </p>
    </div>
    <div className='col-12 mt-2 mb-2'>
      <div className='row'>
        {
          props.CurrentPlayers.map(v => {
            return <div className='col-lg-2 col-md-3 col-4' key={v.Id}>
              <div className='row'>
                <div className='col-6'>
                  <PlayerImage id={v.Id} name={v.Username}></PlayerImage>
                </div>
                <div className='col-6 mt-3'>
                  <CreatorLink id={v.Id} name={v.Username} type='User'></CreatorLink>
                </div>
              </div>
            </div>
          })
        }
      </div>
    </div>
    <div className='col-12 mb-4'>
      <div className='divider-top mt-1'></div>
    </div>
  </div>*/
}

const useStyles = createUseStyles({
  buttonWrapper: {
    margin: '0 auto',
    width: '100%',
    maxWidth: '200px',
  },
  subSectionContainer: {
    padding: 0,
    margin: '0 0 18px',
    width: '100%',
    float: 'left',
    position: 'relative',
    minHeight: '1px',
    display: 'flex',
    flexDirection: 'column',
    '& div, & ul': {
      '&::before,&::after': {
        content: " ",
        display: 'table',
      }
    }
  },
  containerHeader: {
    margin: '3px 0 6px',
    padding: 0,
    justifyContent: 'space-between',
    display: 'flex',
    '& h3': {
      float: 'left',
      margin: 0,
      textAlign: 'center',
      fontSize: '20px',
      fontWeight: '700',
      lineHeight: '1em',
      padding: '5px 0',
    },
    '& span': {
      float: 'right',
      fontSize: '16px',
      fontWeight: '500',
      lineHeight: '100%',
      padding: '4px',
    }
  },
  refreshButton: {
    cursor: 'pointer',
  },
})

// should return same thing as /games/getgameinstancesjson but only servers with friends and an 
function serversWithFriends(servers, friends) {
  const friendsServers = []
  if (
      servers === null ||
      servers === undefined ||
      friends === null ||
      friends === undefined ||
      servers?.Collection === undefined ||
      servers?.Collection === null ||
      !Array.isArray(servers?.CurrentPlayers) ||
      !Array.isArray(friends)
    ){
    return [];
  }
  servers.Collection.forEach(server => {
    const friendInServer = server.CurrentPlayers.some(player =>
      friends.some(p => p.id === player.Id)
    )
    if (friendInServer)
      friendsServers.push(server);
  })
  return friendsServers;
}

const GameServers = props => {
  const store = GameDetailsStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const [friends, setFriends] = useState(null);
  const s = useStyles();
  const showButton = !store.servers || store.servers && store.servers.areMoreAvailable && !store.servers.loading;
  var friendServers = serversWithFriends(store.servers, friends) || [];

  useEffect(() => {
    if (store.servers && store.servers.loading || store.servers !== null) {
      return;
    }
    if (!store.servers) {
      store.setServers({
        loading: true,
      });
    }
    getServers({
      placeId: store.details.id,
      offset: store.servers && store.servers.offset || 0,
    }).then(servers => {
      store.setServers({
        ...servers,
        loading: false,
        areMoreAvailable: servers.Collection.length >= 10,
        offset: (store.servers && store.servers.offset || 0) + 10,
      })
    })
    getFriends({ userId: auth.userId }).then(d => {
      if (d.length > 0) {
        setFriends(d);
      } else {
        setFriends({});
      }
    })
  }, [store.servers])

  return <div className={`${s.subSectionContainer}`}>
    <div className={s.containerHeader}>
      <h3>VIP Servers</h3>
      <span className={`link2018 ${s.refreshButton}`} onClick={() => { store.setServers(null) }}>Refresh</span>
    </div>
    <div className={`col-xs-12 section-content-off`}>This game does not support VIP Servers.</div>

    <div className={s.containerHeader}>
      <h3>Servers My Friends Are In</h3>
    </div>
    {
      friendServers && friendServers.length > 0 && friendServers.map(v => {
        return <ServerEntry key={v.Guid} {...v} />
      })
      ||
      <div className={`col-xs-12 section-content-off`}>No Servers Found.</div>
    }

    <div className={s.containerHeader}>
      <h3>Other Servers</h3>
    </div>
    {
      store.servers && store.servers.Collection && store.servers.Collection.length === 0 &&
      <div className={`col-xs-12 section-content-off`}>There are currently no running games.</div>
      ||
      store.servers && store.servers.Collection && store.servers.Collection.length > 0 &&
      store.servers.Collection.map(v => {
        return <ServerEntry key={v.Guid} {...v} />
      })
      ||
      <div className={`col-xs-12 section-content-off`}>An error occurred while loading running games.</div>
    }
  </div>

  /*return <div className='row'>
    <div className='col-12 mt-4 mb-4'>
      {
        store.servers && store.servers.Collection && store.servers.Collection.map(v => {
          return <ServerEntry key={v.Guid} {...v}></ServerEntry>
        }) || null
      }
      {
        store.servers && store.servers.Collection && store.servers.Collection.length === 0 && <p>Nobody is playing this game.</p> || null
      }
      <div className={s.buttonWrapper}>
        {showButton && <ActionButton label='Load Games' onClick={(e) => {
          e.preventDefault();
          if (store.servers && store.servers.loading) {
            return;
          }
          if (!store.servers) {
            store.setServers({
              loading: true,
            });
          }
          getServers({
            placeId: store.details.id,
            offset: store.servers && store.servers.offset || 0,
          }).then(servers => {
            store.setServers({
              ...servers,
              loading: false,
              areMoreAvailable: servers.Collection.length >= 10,
              offset: (store.servers && store.servers.offset || 0) + 10,
            })
          })
        }}></ActionButton>}
      </div>
    </div>
  </div>*/
}

export default GameServers;