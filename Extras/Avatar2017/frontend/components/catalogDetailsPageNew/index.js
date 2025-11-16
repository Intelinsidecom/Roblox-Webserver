import { useState, useEffect } from 'react'
import { createUseStyles } from "react-jss";
import AuthenticationStore from "../../stores/authentication";
import CatalogDetailsPage from "./stores/catalogDetailsPage";

import AdBanner from "../ad/adBanner";
import AdSkyscraper from "../ad/adSkyscraper";

import { addOrRemoveFromCollections } from "../../services/catalog";
import { getCollections } from "../../services/inventory";
import Thumbnail from './components/thumbnail';
import ItemDetails from './components/itemDetails';
import Favorite from '../gameDetails/components/new/favorite';

const filterTextForEmpty = (str, assetType) => {
  if (!str) return assetType;
  if (str.trim().length === 0) {
    return assetType;
  }
  if (!str.match(/[a-z0-9A-Z]+/g)) {
    return assetType;
  }
  return str;
}

const useStyles = createUseStyles({
  catalogItemContainer: {
    maxWidth: '970px',
    float: 'left',
  },
  itemDetailsWrapper: {
    padding: '15px',
    backgroundColor: '#fff',
    margin: '0 0 18px',
    display: 'flex',
    flexDirection: 'row',
  },
  itemDetailsContainer: {
    float: 'right',
    paddingLeft: '12px',
    width: 'calc(100% - 432px)',
  },
  itemNameContainer: {
    marginBottom: '12px',
    paddingBottom: '12px',
    float: 'right',
    borderBottom: '1px solid #e3e3e3',
    width: '100%',
    '& h2': {
      fontSize: '32px',
      fontWeight: 800,
      textOverflow: 'ellipsis',
      overflow: 'hidden',
      padding: '5px 0',
      lineHeight: '1em',
      margin: 0
    }
  },
  textSpan: {
    fontSize: '16px',
    fontWeight: '500',
    color: '#b8b8b8',
  },
  textName: {
    color: '#00a2ff',
    fontWeight: '500',
    fontSize: '16px',
    textDecoration: 'none!important',
    '&:hover': {
      textDecoration: 'underline!important',
    }
  },

  thumbAndLikesContainer: {
    display: 'flex',
    flexDirection: 'column',
    width: 'auto',
    height: 'auto',
  },
})

/**
 * CatalogDetails page
 * @param {{details: AssetDetailsEntry}} props
 * @returns 
 */
const CatalogDetails = (props) => {
  const { details } = props;
  const authStore = AuthenticationStore.useContainer();
  const s = useStyles();
  const isLimited = details.itemRestrictions.includes('Limited');
  const isLimitedUnique = details.itemRestrictions.includes('LimitedUnique');
  const store = CatalogDetailsPage.useContainer();

  useEffect(() => {
    store.setDetails(props.details);
    if (props.details.saleCount) {
      store.setSaleCount(props.details.saleCount);
    } else {
      store.setSaleCount(0);
    }
    if (props.details.offsaleDeadline) {
      store.setOffsaleDeadline(props.details.offsaleDeadline);
    } else {
      store.setOffsaleDeadline(null);
    }
  }, [props]);

  useEffect(() => {
    if (!authStore.userId || !store.details) {
      return;
    }
    store.loadOwnedCopies(authStore.userId);
    if (store.isResellable) {
      store.loadResellers();
    }
    getCollections({
      userId: authStore.userId,
    }).then(col => {
      let inCollection = col.find(v => {
        return v.Id === store.details.id;
      });
      store.setInCollection(inCollection !== undefined);
    })
  }, [store.details, authStore.userId]);

  if (!store.details) return null;
  const subTitle = `${store.subCategoryDisplayName}${(isLimited || isLimitedUnique) ? ' / Collectible Item' : ''}${isLimitedUnique ? ' / Limited Edition' : ''}`;
  const url = (details.creatorType === 'User' ? '/User.aspx?ID=' + details.creatorTargetId : '/My/Groups.aspx?gid=' + details.creatorTargetId);

  return <div className='container' style={{ maxWidth: '1154px' }}>
    <div style={{ marginBottom: 10 }}>
      <AdBanner />
    </div>
    <div className={s.catalogItemContainer}>
      <div className={s.itemDetailsWrapper}>
        <div className={s.thumbAndLikesContainer}>
          <Thumbnail name={details.name} id={details.id} isLimited={isLimited} isLimitedU={isLimitedUnique} />
          <div style={{ marginTop: '6px' }}>
            <Favorite assetId={details.id} favoriteCount={details.favoriteCount} />
          </div>
        </div>
        <div className={s.itemDetailsContainer}>
          <div className={s.itemNameContainer}>
            <h2>{details.name}</h2>
            <span className={s.textSpan}>By <a href={url} className={s.textName}>{details.creatorName}</a></span>
          </div>
          <ItemDetails details={details} />
        </div>
      </div>
      {/*<div className={s.recommendedContainer}>

      </div>
      <div className={s.itemDetailsContainer}>

      </div>*/}
    </div>
    <AdSkyscraper />
  </div>
}

export default CatalogDetails;