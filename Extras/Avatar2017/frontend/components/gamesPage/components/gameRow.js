import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import NewGameCard from "../../newGameCard";
import UserAdvertisement from "../../userAdvertisement";
import useDimensions from "../../../lib/useDimensions";
import Link from "../../link";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";

export const useStyles = createUseStyles({
  title: {
    fontWeight: 300,
    marginBottom: '10px',
    marginTop: '10px',
    color: 'var(--text-color-primary)',
    marginLeft: '10px',
  },
  gameRow: {
    display: 'flex',
    //alignContent: 'center',
    width: 'calc(100% - 53px - 53px)!important',
    clipPath: 'inset(-10px -15px -90px -10px)',
    padding: '0 6px',
    flexWrap: 'nowrap',
    overflow: 'visible',
    //overflowX: 'hidden',
    marginLeft: '-4px',
    '@media(max-width: 994px)': {
      overflow: 'scroll',
      overflowY: 'hidden',
      width: '100%!important',
    },
    //height: '295px',
    '&>div': {
      flex: '0 0 auto',
    }
  },
  gameCard: {
    width: '170px',
    padding: '5px',
    margin: 0,
  },
  pagerButton: {
    border: '1px solid var(--text-color-quinary)',
    width: '40px',
    height: 'calc(100% - 30px)',
    background: 'var(--white-color)',
    position: 'relative',
    cursor: 'pointer',
    color: 'var(--text-color-primary)',
    boxShadow: '0 0 3px 0 var(--text-color-secondary)',
    marginTop: '15px',
    display: 'flex',
    '@media(max-width: 994px)': {
      display: 'none',
    },
    // '&:hover': {
    //   color: 'black',
    // },
  },
  goBack: {
    //float: 'left',
    marginLeft: '10px',
  },
  goForward: {
    //float: 'right',
    marginLeft: 'auto',
  },
  pagerCaret: {
    textAlign: 'center',
    userSelect: 'none',
    fontSize: '40px',
    margin: 'auto',
  },
  caretLeft: {
    display: 'block',
    transform: 'rotate(90deg)',
    marginRight: '10px',
  },
  caretRight: {
    display: 'block',
    transform: 'rotate(-90deg)',
    marginLeft: '10px',
  },

  containerHeader: {
    fontSize: '16px',
    fontWeight: '700',
    lineHeight: '1.4em',
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'end',
    marginLeft: '10px',
    position: "absolute",
    zIndex: 1,
    marginTop: '15px',
    //width: 'calc(75% - 10px)',
    width: '65.75%',
    '@media(max-width: 994px)': {
      width: '100%',
      marginLeft: '6px',
    },
    '& h3': {
      fontSize: '24px',
      fontWeight: 300,
      float: 'left',
      margin: 0,
      lineHeight: '1.4em',
      '@media (max-width: 767px)': {
        fontSize: '21px',
      }
    }
  },
  seeAllButton: {
    borderColor: 'var(--primary-color)',
    width: '90px',
    padding: '4px',
    fontSize: '14px',
    fontWeight: 400,
    lineHeight: '100%',
    transition: 'box-shadow 200ms ease-in-out',
    boxShadow: 'none',
    '&:hover': {
      borderColor: '#32B5FF',
      boxShadow: '0 1px 3px rgba(150,150,150,0.74)'
    }
  },
  uselessFuckingClass: {
    display: 'flex',
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: '43px',
    '@media(max-width: 994px)': {
      paddingRight: 0,
    },
  },
  advertisementContainer: {
    '@media(max-width: 994px)': {
      marginLeft: '6px',
    },
  },
  sort: {
    '@media(max-width: 994px)': {
      paddingRight: 0,
    },
  },
});

/**
 * A game row
 * @param {{
 * title: string;
 * games: any[];
 * icons: any;
 * token: string;
 * ads?: boolean;
 * }} props
 */
const GameRow = props => {
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const [offset, setOffset] = useState(0);
  const [limit, setLimit] = useState(1);
  const [offsetComp, setOffsetComp] = useState(0);
  const rowRef = useRef(null);
  const gameRowRef = useRef(null);
  const [rowHeight, setRowHeight] = useState(0);

  const [dimensions] = useDimensions();
  useEffect(() => {
    if (!rowRef.current) {
      return
    }
    // width = 170px
    // sub 80 for pagination buttons
    let windowWidth = rowRef.current.clientWidth;
    // breakpoints: 992, 1300 for side nav
    let offsetNotRounded = (windowWidth - 80) / 170;
    let newLimit = Math.floor(offsetNotRounded);

    setLimit(newLimit);
    if (offsetNotRounded !== newLimit) {
      setOffsetComp(1);
    } else {
      setOffsetComp(0);
    }
  }, [dimensions, props.games, props.icons]);

  useEffect(() => {
    if (!gameRowRef.current)
      return;
    const newHeight = Math.max(236, gameRowRef.current.clientHeight);
    if (newHeight === rowHeight) return;

    setRowHeight(newHeight);
  });
  if (!props.games) return null;

  const remainingGames = props.games.length - (offset - offsetComp);
  const showForward = remainingGames >= limit;
  return <div className={`${s.sort} row`}>
    {/*<div className='col-12'>
      <h3 className={s.title}>{props.title.toUpperCase()}</h3>
    </div>*/}
    <div className={s.containerHeader}>
      <h3>{String(props.title).charAt(0).toUpperCase() + String(props.title).slice(1)}</h3>
      <span>
        <Link href={`/games?sortFilter=${props.token}`}>
          <a href={`/games?sortFilter=${props.token}`}>
            <ActionButton buttonStyle={buttonStyles.newContinueButton} className={s.seeAllButton} label='See All' />
          </a>
        </Link>
      </span>
    </div>
    <div className={`${props.ads ? 'col-12 col-lg-9' : 'col-12'} ${s.uselessFuckingClass}`} ref={rowRef}>
      <div className={s.goBack + ' ' + s.pagerButton + ' ' + (offset === 0 ? 'opacity-25' : '')} onClick={() => {
        if (offset === 0)
          return;
        setOffset(offset - limit);
      }} style={{ height: rowHeight }}>
        <p className={s.pagerCaret}><span className={s.caretRight}>^</span></p>
      </div>
      <div className={'row ' + s.gameRow} ref={gameRowRef}>
        {
          props.games.slice(offset, offset + 100).map((v, i) => {
            return <NewGameCard
              key={i}
              className={s.gameCard}
              placeId={v.placeId}
              creatorId={v.creatorId}
              creatorType={v.creatorType}
              creatorName={v.creatorName}
              iconUrl={props.icons[v.universeId]}
              year={v.year}
              likes={v.totalUpVotes}
              dislikes={v.totalDownVotes}
              name={v.name}
              playerCount={v.playerCount}
            />
          })
        }
      </div>
      {showForward ? <div className={`${s.goForward} ${s.pagerButton}`} onClick={() => {
        let newOffset = ((offset) + (limit));
        setOffset(newOffset);
      }} style={{ height: rowHeight }}>
        <p className={s.pagerCaret}><span className={s.caretLeft}>^</span></p>
      </div> : null
      }
    </div>
    {props.ads ? <div className={`col-12 col-lg-3 ${s.advertisementContainer}`}><UserAdvertisement type={3} /></div> : null}
  </div>
}

export default GameRow;