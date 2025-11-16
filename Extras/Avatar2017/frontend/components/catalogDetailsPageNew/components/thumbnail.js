import { createUseStyles } from "react-jss";
import ItemImage from '../../itemImage';

const useStyles = createUseStyles({
  thumbContainer: {
    height: '420px',
    width: '420px',
    position: 'relative',
    '& img': {
      maxWidth: '420px',
      padding: 0,
    }
  },
  img: {
    width: 'inherit',
    height: 'inherit',
  },
  assetRestrictions: {
    position: 'absolute',
    bottom: '-2px',
    left: '-2px'
  },
  iconLabel: {
    backgroundSize: '100% auto',
    width: '164px',
    height: '24px',
    backgroundImage: 'url(/img/itemlabel.svg)',
    backgroundRepeat: 'no-repeat',
    display: 'inline-block',
    verticalAlign: 'middle',
  },
  iconLimited: {
    backgroundPosition: '0 -72px',
  },
  iconLimitedU: {
    backgroundPosition: '0 -168px',
  },
})

/**
 * Thumbnail component
 * @param {{name: string; id: number; isLimited: boolean; isLimitedU: boolean;}} props
 * @returns 
 */
const Thumbnail = (props) => {
  const { name, id: itemId, isLimited, isLimitedU } = props;
  const s = useStyles();

  return <div className={s.thumbContainer}>
    <ItemImage className={s.img} id={itemId} name={name} />
    <div className={s.assetRestrictions}>
      {isLimited && <span className={`${s.iconLabel} ${s.iconLimited}`} />}
      {isLimitedU && <span className={`${s.iconLabel} ${s.iconLimitedU}`} />}
    </div>
  </div>
}

export default Thumbnail;