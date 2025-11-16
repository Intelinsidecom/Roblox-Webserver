import { createUseStyles } from "react-jss";
import GameDetailsStore from "../stores/gameDetailsStore";
import {useEffect, useState} from "react";
import {getGamePassCreationUrl, getGameUrl, getUniverseGamePasses} from "../../../services/games";
import ItemImage from "../../itemImage";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";
import Link from "../../link";
import {getItemUrl} from "../../../services/catalog";
import ActionLink from "../../actionLink";
import Robux2 from "../../robux2";
import AuthenticationStore from "../../../stores/authentication";

const useStyles = createUseStyles({
  tabPane: {
    //backgroundColor: 'var(--white-color)',
    //padding: '12px',
    display: 'flex',
    flexDirection: 'column',
  },
  gamePassContainer:{
    overflow: 'hidden',
    margin: '0 0 6px',
    padding: '0'
  },
  containerHeader:{
    fontSize: '16px',
    fontWeight: '700',
    lineHeight: '1.4em',
    display: 'flex',
  },
  containerHeaderText:{
    fontSize: '20px',
    fontWeight: '700',
    float: 'left',
    margin: 0,
    lineHeight: '1.4em',
    paddingBottom: '5px'
  },
  noPasses:{
    color: 'var(--text-color-tertiary)',
    background: 'transparent !important',
    padding: '15px',
    textAlign: 'center',
    lineHeight: '1.5em',
    margin: 0,
    wordWrap: 'break-word',
    hyphens: 'none',
  },
  passesContainer:{
    display: 'flex',
    flexWrap: 'wrap',
    listStyle: 'none',
    margin: 0,
    padding: 0,
    //padding: 6, // accounts for box shadowing // disabled due to it looking weirder with this on than off (both weird)
  },
  listItem:{
    width: '16.6666666667%',
    marginBottom: '12px',
    float: 'left',
    display: 'list-item',
  },
  passCard:{
    border: '1px solid var(--background-color)',
    backgroundColor: 'var(--white-color)',
    position: 'relative',
    borderRadius: '3px',
    margin: '0 5% 0 0',
    maxWidth: '150px',
    padding: 0,
  },
  passPicture:{
    display: 'block',
    textAlign: 'center',
    '& img':{
      width: '150px',
      height: '150px',
      borderRadius: '3px',
      border: 0,
      verticalAlign: 'middle',
    }
  },
  passCaption:{
    borderTop: '1px solid var(--text-color-secondary)',
    padding: '0 6px 6px',
  },
  passName:{
    fontWeight: '500',
    margin: '3px auto',
    fontSize: '16px',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
  },
  passPriceContainer:{
    lineHeight: '1em',
  },
  passFooter: {
  },
  
  // new code because i dont trust the code above lol
  
  gPassWrapper: {
    width: '16.66667%',
    marginBottom: 12
  },
  gPassContainer: {
    display: 'flex',
    flexDirection: 'column',
    maxWidth: '150px',
    borderRadius: 3,
    margin: '0 5% 0 0',
    padding: 0,
    overflow: 'hidden'
  },
  gPassImg: {
    width: 150,
    height: 150,
    margin: 0,
    padding: 0,
    cursor: 'pointer',
  },
  gPassDetails: {
    display: 'flex',
    flexDirection: 'column',
    borderTop: '1px solid #b8b8b8',
    padding: '0 6px 6px'
  },
  gPassName: {
    fontWeight: 500,
    margin: '3px auto',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
    width: '100%',
    textAlign: 'start'
  },
  gPassPriceContainer: {},
  gPassBuyContainer: {
    marginTop: 6,
  },
  gPassBuyButton: {
    padding: 9,
    fontSize: 18,
    fontWeight: 500,
    lineHeight: '100%',
    '&:hover': {
      backgroundColor: '#3FC679',
      boxShadow: '0 1px 3px rgba(150,150,150,0.74)',
      borderColor: '#3FC679!important',
      color: '#fff!important',
      cursor: 'pointer',
    }
  },
  gPassOwnedButton: {
    padding: 11,
    fontSize: "16px",
    fontWeight: 500,
    lineHeight: "100%",
    margin: "0 auto",
    color: "var(--text-color-secondary)"
  },
  addPassContainer: {
    justifyContent: 'center',
    alignItems: 'center',
    height: '100%',
  },
  addPassIcon: {
    backgroundImage: 'url(/img/plus.png)',
    height: '44px',
    width: '44px',
  },
  addPassText: {
    color: '#494d5a',
    marginTop: 20
  },
})

/**
 *
 * @param {{ id: number; name: string; price: number; isOwned: boolean; }} props
 * @constructor
 */
const GamePassEntry = ({ id, name, price, isOwned }) => {
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const url = getItemUrl({assetId: id, name});
  
  return <div className={s.gPassWrapper}>
    <div className={`section-content hoverShadow ${s.gPassContainer}`}>
      <Link href={url}>
        <a href={url}>
          <ItemImage className={s.gPassImg} id={id} name={name}/>
        </a>
      </Link>
      <div className={s.gPassDetails}>
        <span className={s.gPassName}>{name}</span>
        <span className={s.gPassPriceContainer}>
        <Robux2>{price}</Robux2>
      </span>
        <div className={`${s.gPassBuyContainer} ${isOwned ? "flex" : undefined}`}>
          {
            isOwned ?
                <span className={s.gPassOwnedButton}>Owned</span> :
                <ActionLink className={s.gPassBuyButton} label='Buy' buttonStyle={buttonStyles.newCancelButton}
                                            href={url}/>
          }
        </div>
      </div>
    </div>
  </div>
}

const AddPassEntry = ({ universeId, forceHeight }) => {
  const s = useStyles();
  const url = getGamePassCreationUrl({ universeId });
  
  return <div className={s.gPassWrapper} style={forceHeight ? { height: 245 } : null}>
    <Link href={url}>
      <a href={url} className={`section-content hoverShadow ${s.gPassContainer} ${s.addPassContainer}`}>
        <span className={s.addPassIcon}/>
        <span className={s.addPassText}>Add Pass</span>
      </a>
    </Link>
  </div>
}

const GameStore = props => {
  const s = useStyles();
  const auth = AuthenticationStore.useContainer();
  const store = GameDetailsStore.useContainer();
  if (!store.placeDetails || !store.universeDetails) return null;
  // 0 == loading, 1 == no passes, 2 == failed to load, array is success
  const [passes, setPasses] = useState(0);
  
  useEffect(() => {
    setPasses(0);
    getUniverseGamePasses({universeId: store.universeDetails.id, unfiltered: false}).then(d => {
      try { // accounts for d being null if for whatever rerason it is
        d = d.filter(pass => pass.price >= 2).filter(pass => pass.isForSale === true);
        if (d.length === 0) {
          setPasses(1);
          return;
        }
        setPasses(d);
      } catch (e) {
        console.error(e);
        setPasses(2);
      }
    })
  }, [store.universeDetails])
  
  return <div className={s.tabPane}>
    <div className={s.gamePassContainer}>
      <div className={s.containerHeader}>
        <h3 className={s.containerHeaderText}>Passes for this game</h3>
      </div>
      {Array.isArray(passes) ?
          <ul className={s.passesContainer}>
            {passes.map(pass => <GamePassEntry key={pass.id} id={pass.id} name={pass.name} price={pass.price} isOwned={pass.isOwned} />)}
            {/* TODO: will need to be changed if we ever add group games */}
            {
              store.universeDetails.creator.id === auth.userId && passes.length < 16 ?
                  <AddPassEntry universeId={store.universeDetails.id} />
                  : null
            }
          </ul>
          : <div>
            {
              store.universeDetails.creator.id === auth.userId ?
                  <AddPassEntry universeId={store.universeDetails.id} forceHeight />
                  : null
            }
            {passes === 0 ?
                <p className={s.noPasses}>Loading gamepasses...</p>
                : passes === 1 ?
                    <p className='section-content-off'>This game does not sell any virtual items or power-ups.</p>
                    :
                    <p className='section-content-off'>An error occurred while loading gamepasses.</p>}
          </div>
      }
    </div>
  </div>
}

export default GameStore;