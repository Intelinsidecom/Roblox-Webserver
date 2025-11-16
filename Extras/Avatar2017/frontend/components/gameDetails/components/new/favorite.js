import {useEffect, useState} from "react";
import {createFavorite, deleteFavorite, getIsFavorited} from "../../../../services/catalog";
import authentication from "../../../../stores/authentication";
import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  wrapper: {
  },
  favoriteStar: {
    display: 'inline-block',
    width: '68px',
    height: '16px',
    textAlign: 'center',
    cursor: 'pointer',
    whiteSpace: 'nowrap',
    background: 'url("/img/FavoriteStar.png")',
    marginBottom: '-2px',
  },
  favoriteCount: {
    textAlign: 'center',
  },
  favoriteContainer:{
    width: '68px',
    textAlign: 'center',
    cursor: 'pointer',
  },
  favoriteButton:{
    cursor: 'pointer',
    textDecoration: 'none',
    outline: 0,
    background: 'transparent',
  },
  favoriteIcon:{
    marginBottom: '4px',
    backgroundPosition: '0 -84px',
    backgroundImage: 'url(/img/branded.svg)',
    backgroundRepeat: 'no-repeat',
    backgroundSize: 'auto auto',
    width: '28px',
    height: '28px',
    display: 'inline-block',
    verticalAlign: 'middle',
  },
  favoriteIconSelected:{
    backgroundPosition: '-28px -84px'
  },
  favoriteLabel:{
    fontSize: '12px'
  },
});

const Favorite = props => {
  const {assetId} = props;
  const auth = authentication.useContainer();
  const s = useStyles();

  const [isFavorited, setIsFavorited] = useState(null);
  const [favoriteCount, setFavoriteCount] = useState(0);
  const [locked, setLocked] = useState(false);

  const [iconHovered, setIconHovered] = useState(false);
  const mouseEnter = () => setIconHovered(true);
  const mouseLeave = () => setIconHovered(false);

  const buttonClass = (isFavorited || iconHovered) ? s.favoriteIconSelected : '';

  useEffect(() => {
    setIsFavorited(null);
    setFavoriteCount(props.favoriteCount);
    setLocked(false);

    if (auth.userId) {
      getIsFavorited({assetId, userId: auth.userId}).then(data => {
        setIsFavorited(!!data);
      }).catch(e => {
        // undefined/null response causes axios to incorrectly return network error :)
        setIsFavorited(false);
      })
    }
  }, [props.favoriteCount, props.assetId, auth.userId]);


  return <div className={s.favoriteContainer}>
    <a className={`${s.favoriteButton}`} href="#" onClick={e => {
        e.preventDefault();
        if (locked) return;
        setLocked(true);
        setIsFavorited(!isFavorited);
        setFavoriteCount(isFavorited ? favoriteCount-1 : favoriteCount+1);
        if (isFavorited) {
          deleteFavorite({userId: auth.userId, assetId}).finally(() => {
            setLocked(false);
          })
        }else{
          createFavorite({userId: auth.userId, assetId}).finally(() => {
            setLocked(false);
          })
        }
    }} onMouseEnter={mouseEnter} onMouseLeave={mouseLeave}>
        <div className={`${s.favoriteIcon} ${buttonClass}`}></div>
        <div className={s.favoriteLabel}>{isFavorited ? 'Favorited' : 'Favorite'}</div>
    </a>
  </div>
  /*return <div className={s.wrapper}>
    <div className={s.favoriteCount}><div className={s.favoriteStar}/>
      {
        isFavorited !== null ? <span className='ms-1'>
          <a href="#" onClick={e => {
            e.preventDefault();
            if (locked) return;
            setLocked(true);
            setIsFavorited(!isFavorited);
            setFavoriteCount(isFavorited ? favoriteCount-1 : favoriteCount+1);
            if (isFavorited) {
              deleteFavorite({userId: auth.userId, assetId}).finally(() => {
                setLocked(false);
              })
            }else{
              createFavorite({userId: auth.userId, assetId}).finally(() => {
                setLocked(false);
              })
            }
          }}>{isFavorited ? 'Favorited' : 'FavouriteButton'}</a>
        </span> : null
      }
    </div>
  </div>*/
}

export default Favorite;