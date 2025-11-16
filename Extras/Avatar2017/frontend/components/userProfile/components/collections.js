import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getBaseUrl } from "../../../lib/request";
import {getItemUrl} from "../../../services/catalog";
import { getCollections } from "../../../services/inventory";
import useCardStyles from "../styles/card";
import Subtitle from "./subtitle";
import SmallTextLink from "./smallTextLink";
import useButtonWrapperStyle from '../styles/buttonWrapper'
import Link from "../../link";
import ItemImage from "../../itemImage";
import {CringeRobloxHats} from "../../../stores/catalogPage";

const useCollectionStyles = createUseStyles({
  imageWrapper: {
    border: '1px solid var(--text-color-quinary)',
    //borderRadius: '4px',
    borderRadius: 0,
    position: 'relative',
  },
  image: {
    width: '100%',
    margin: '0 auto',
    display: 'block',
    padding: 0
  },
  itemLabel: {
    fontWeight: 500,
    fontSize: '16px',
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    whiteSpace: 'nowrap',
  },
  labelWrapper: {
    width: '100%',
    // marginTop: '-27px',
    overflow: 'hidden',
    position: 'absolute',
    bottom: -2,
    left: -2,
  },
  overlayLimited: {
    height: '28px',
    marginLeft: '-14px',
  },
  overlayLimitedUnique: {
    height: '28px',
    marginLeft: '-14px',
  },
});

const Collections = props => {
  const { userId } = props;
  const s = useCollectionStyles();
  const s2 =useButtonWrapperStyle();
  const cardStyles = useCardStyles();
  const [collections, setCollections] = useState(null);
  useEffect(() => {
    getCollections({ userId }).then(d => {
      setCollections(d.filter(r => !CringeRobloxHats.includes(r.Name)));
    })
  }, [userId]);

  if (!collections || collections.length === 0) {
    return null;
  }
  return <div className='flex'>
    <div className='col-10'>
      <Subtitle>Collections</Subtitle>
    </div>
    <div className='col-2'>
      <div className={`${s2.buttonWrapper}`}>
        <SmallTextLink href={`/users/${userId}/inventory`}>Inventory</SmallTextLink>
      </div>
    </div>
    <div className='col-12'>
      <div className={`marginStuff ${cardStyles.card}`}>
        <div className='flex ps-4 pe-4 pt-4 pb-4'>
          {
            collections.map((v, i) => {
              const assetId = v.Id;
              const url = assetId && getItemUrl({assetId: assetId, name: v.Name}) || v.AssetSeoUrl;
              const isLimited = v.AssetRestrictionIcon && v.AssetRestrictionIcon.CssTag === "limited";
              const isLimitedUnique = v.AssetRestrictionIcon && v.AssetRestrictionIcon.CssTag === "limited-unique";
              const hasOverlay = isLimited || isLimitedUnique;

              return <div className='col-4 col-lg-2 ml-1 mr-1' key={i}>
                <Link href={url}>
                  <a href={url}>
                    <div className={s.imageWrapper}>
                      {/*<img src={v.Thumbnail.Url.startsWith('http') ? v.Thumbnail.Url : getBaseUrl() + v.Thumbnail.Url}*/}
                      {/*     className={s.image}/>*/}
                      <ItemImage id={assetId} className={s.image} />
                      {hasOverlay ? <div className={s.labelWrapper}>
                        {/*{*/}
                        {/*  isLimited ? <img className={s.overlayLimited} src='/img/limitedOverlay_itemPage.png'/>*/}
                        {/*      : isLimitedUnique ? <img className={s.overlayLimitedUnique}*/}
                        {/*                               src='/img/limitedUniqueOverlay_itemPage.png'/> : null*/}
                        {/*}*/}
                        {
                          isLimited ?
                              <span className="icon-limited-label" />
                          : isLimitedUnique ?
                              <span className="icon-limited-unique-label" />
                          : null
                        }
                      </div> : null}
                    </div>
                    <p className={`mb-0 link2019 ${s.itemLabel}`}>{v.Name}</p>
                  </a>
                </Link>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}

export default Collections;