import GameDetailsStore from "../../stores/gameDetailsStore";
import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../../../lib/numberUtils";
import Badges from './components/badges'
import dayjs from "dayjs";

const useStyles = createUseStyles({
  descriptionText: {
    whiteSpace: 'pre-wrap',
    wordBreak: 'break-word',
    overflow: 'hidden',
    paddingBottom: '12px',
    fontSize: '16px',
    margin: 0,
    color: 'var(--text-color-primary)',
    display: 'block',
    lineHeight: '1.4em',
    textRendering: 'auto',
    fontWeight: '400',
    width: '100%',
    fontFamily: 'HCo Gotham SSm,Helvetica Neue,Helvetica,Arial,Lucida Grande,sans-serif'
  },
  allContainer: {
  },
  descriptionContainer: {
    position: 'relative',
    padding: 0,
    '& div': {
      '&:before': {
        content: ' ',
        display: 'table'
      },
      '&:after': {
        content: ' ',
        display: 'table'
      },
    }
  },
  descriptionHeaderContainer: {
    margin: '3px 0 6px',
    display: 'flex',
    justifyContent: 'space-between',
    '& h3': {
      fontSize: '20px',
      fontWeight: '700',
      lineHeight: '1em',
      float: 'left',
      margin: 0,
      padding: '5px 0',
      paddingTop: 0,
    }
  },
  contentContainer: {
    backgroundColor: 'var(--white-color)',
    padding: '15px',
    position: 'relative',
    marginBottom: '15px',
    display: 'flex',
    flexDirection: 'column',
  },
  gameStatsContainer: {
    padding: '12px 0',
    borderBottom: '1px solid var(--background-color)',
    borderTop: '1px solid var(--background-color)',
    listStyle: 'none',
    margin: 0,
    display: 'flex',
    '@media (max-width: 767px)': {
      flexDirection: 'column',
    },
  },
  gameStat: {
    width: '12.5%',
    float: 'left',
    textAlign: 'center',
    listStyle: 'none',
    margin: 0,
    padding: 0,
    '@media (max-width: 767px)': {
      margin: 0,
      width: '100%',
      display: 'flex',
      flexDirection: 'row',
    },
  },
  gameStatText: {
    '@media (max-width: 767px)': {
      width: '45%!important',
      textAlign: 'start!important',
      fontSize: '14px!important',
    },
  },
  gameStatLabel: {
    whiteSpace: 'nowrap',
    fontSize: '12px',
    fontWeight: '500',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    color: 'var(--text-color-secondary)',
    lineHeight: '1.5em',
    margin: 0,
    wordWrap: 'break-word',
    hyphens: 'none',
  },
  gameStatStat: {
    whiteSpace: 'nowrap',
    fontSize: '12px',
    fontWeight: '400',
    color: 'var(--text-color-primary)',
    lineHeight: '1.5em',
    margin: 0,
    wordWrap: 'break-word',
    hyphens: 'none',
  },
  statGears: {
    margin: '3px auto',
    '@media (max-width: 767px)': {
      margin: 0,
    },
  },
  iconNoGear: {
    width: '30px',
    display: 'inline-block',
    backgroundPosition: '0 -336px',
    backgroundImage: 'url(/img/games.svg)',
    backgroundRepeat: 'no-repeat',
    backgroundSize: 'auto auto',
    height: '28px',
    verticalAlign: 'middle'
  },
  reportAbuseContainer: {
    padding: '12px 0 0',
    display: 'flex',
    justifyContent: 'right',
  },
  reportAbuse: {
    float: 'right',
  },
  abuseLink: {
    fontSize: '12px',
    fontWeight: '500',
    color: '#d86868',
    textDecoration: 'none!important',
    background: 'transparent',
    '&:hover': {
      textDecoration: 'underline!important',
      color: '#d86868',
    },
    '&:active': {
      textDecoration: 'underline!important',
      color: '#d86868',
    },
  },
  refreshButton: {
    border: 0,
    backgroundColor: 'transparent',
    width: 'auto',
    textAlign: 'center',
    fontSize: '16px',
    fontWeight: '500',
    position: 'relative',
    float: 'right',
    margin: 0,
    display: 'inline-block',
    height: 'auto',
    whiteSpace: 'nowrap',
    verticalAlign: 'middle',
    padding: '4px',
    lineHeight: '100%',
    borderRadius: '3px',
  },
  textLink: {
    userSelect: 'none',
    cursor: 'pointer',
    textDecoration: 'none!important',
    color: 'var(--primary-color)',
    '&:hover': {
      textDecoration: 'underline!important',
      color: 'var(--primary-color)',
    }
  },
  createServerPanel: {
    float: 'left',
    margin: '6px 0',
  },
  vipServerDiv: {
    padding: '12px!important',
  },
});

const About = props => {
  const s = useStyles();
  const store = GameDetailsStore.useContainer();
  if (!store.placeDetails || !store.universeDetails) return <div>An error occurred.</div>;
  const stats = [
    {
      name: 'Playing',
      value: store.universeDetails.playing?.toLocaleString() || 0,
    },
    {
      name: 'Favorites',
      value: store.universeDetails.favoritedCount.toLocaleString(),
    },
    {
      name: 'Visits',
      value: abbreviateNumber(store.universeDetails.visits),
    },
    {
      name: 'Created',
      value: dayjs(store.universeDetails.created).format('M/DD/YYYY'),
    },
    {
      name: 'Updated',
      value: dayjs(store.universeDetails.updated).fromNow(),
    },
    {
      name: 'Max Players',
      value: store.universeDetails.maxPlayers.toLocaleString(),
    },
    {
      name: 'Year',
      value: store.placeDetails.year,
    },
    {
      name: 'Allowed Gear',
      value: <span className={s.iconNoGear}></span>,
    },
  ]

  return <div className={s.descriptionContainer}>
    <div className={s.descriptionHeaderContainer}>
      <h3>Description</h3>
    </div>
    <div className={s.contentContainer}>
      <pre className={s.descriptionText}>{store.details.description?.trim() || ''}</pre>
      <ul className={s.gameStatsContainer}>
        {
          stats.map(v => {
            var extraClass = '';
            var valueTitle = '';
            if (v.name == 'Allowed Gear') {
              extraClass = s.statGears;
            } else if (v.name == 'Visits') {
              valueTitle = store.universeDetails.visits.toLocaleString();
            };
            return <li className={s.gameStat}>
              <p className={`${s.gameStatLabel} ${s.gameStatText}`}>{v.name}</p>
              <p title={valueTitle} className={`${s.gameStatStat} ${s.gameStatText} ${extraClass}`}>{v.value || '????'}</p>
            </li>
          })
        }
      </ul>

      <div className={s.reportAbuseContainer}>
        <span className={s.reportAbuse}>
          <a href='/internal/report-abuse' className={s.abuseLink}>Report Abuse</a>
        </span>
      </div>
    </div>

    <div className={s.descriptionHeaderContainer}>
      <h3>VIP Servers</h3>
      <span className={s.refreshButton + ' ' + s.textLink}>Refresh</span>
    </div>
    <div className={s.contentContainer + ' ' + s.vipServerDiv}>
      <span className={s.createServerPanel}>
        Play this game with friends and other people you invite. <br />
        See all your VIP servers in the <a className={s.textLink} href={'#servers'}>Servers</a> tab.
      </span>
    </div>
    
    {
      store.badges && store.badges.data.length > 0 &&
        <>
          <div className={s.descriptionHeaderContainer}>
              <h3>Game Badges</h3>
          </div>
          <Badges />
        </>
    }
  </div>
}

export default About;