import React, {useEffect, useState} from "react";
import { createUseStyles } from "react-jss";
import AuthenticationStore from "../../stores/authentication";
import GearDropdown from "../gearDropdown";
import OldVerticalTabs from "../oldVerticalTabs";
import ReportAbuse from "../reportAbuse";
import BuyButton from "./components/buyButton";
import BuyItemModal from "./components/buyItemModal";
import Comments from "./components/comments";
import CreatorDetails from "./components/creatorDetails";
import DelistItemModal from "./components/delistItemModal";
import ItemImage from "../itemImage";
import { LimitedOverlay, LimitedUniqueOverlay } from "./components/limitedOverlay";
import Recommendations from "./components/recommendations";
import Resellers from "./components/resellers";
import SaleHistory from "./components/saleHistory";
import SellItemModal from "./components/sellItemModal";
import CatalogDetailsPage from "./stores/catalogDetailsPage";
import AdBanner from "../ad/adBanner";
import AdSkyscraper from "../ad/adSkyscraper";
import { addOrRemoveFromCollections } from "../../services/catalog";
import { getCollections } from "../../services/inventory";
import getFlag from "../../lib/getFlag";
import Owners from "./components/owners";
import AudioPlayButton from './components/audioPlayButton';
import Link from "../link";
import {getGamePassRootPlace, getGameUrl} from "../../services/games";
import {getBadgeInfoByID} from "../../services/badges";
import RemoveItemModal from "./components/removeItemModal";

const emptyDescriptionMessage = 'No description available.';
const filterTextForEmpty = str => {
  if (!str) return emptyDescriptionMessage;
  if (str.trim().length === 0) {
    return emptyDescriptionMessage;
  }
  if (!str.match(/[a-z0-9A-Z]+/g)) {
    return emptyDescriptionMessage;
  }
  return str;
}

const useStyles = createUseStyles({
  title: {
    fontWeight: 650,
    fontSize: '32px',
    color: 'var(--text-color-primary)',
  },
  subtitle: {
    fontWeight: 600,
    fontSize: '14px',
  },
  description: {
    marginBottom: 0,
    fontSize: '14px',
    whiteSpace: 'break-spaces'
  },
  catalogItemContainer: {
    background: 'var(--white-color)',
    padding: '2px 8px',
    overflow: 'hidden',
  },
})

/**
 * CatalogDetails page
 * @param {{details: AssetDetailsEntry}} props
 * @returns
 */
const CatalogDetails = props => {
  const { details } = props;
  const authStore = AuthenticationStore.useContainer();
  const s = useStyles();
  const isLimited = details.itemRestrictions.includes('Limited');
  const isLimitedUnique = details.itemRestrictions.includes('LimitedUnique');
  const store = CatalogDetailsPage.useContainer();
  const [gamePassPlace, setGamePassPlace] = useState(null);
  const [badgeInfo, setBadgeInfo] = useState(null);

  useEffect(() => {
    store.setDetails(props.details);
    if (props.details.saleCount) {
      store.setSaleCount(props.details.saleCount);
    }else{
      store.setSaleCount(0);
    }
    if (props.details.offsaleDeadline) {
      store.setOffsaleDeadline(props.details.offsaleDeadline);
    }else{
      store.setOffsaleDeadline(null);
    }
  }, [props]);

  useEffect(() => {
    if (!authStore.userId || !store.details) return;
    
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
    
    if (details.assetType === 34) {
      getGamePassRootPlace({ assetId: details.id }).then(setGamePassPlace);
    }
    if (details.assetType === 21) {
      getBadgeInfoByID({ badgeId: details.id }).then(setBadgeInfo);
    }
  }, [store.details, authStore.userId]);

  const hasItemToDeList = store.isResellable && store.allResellers && store.allResellers.find(v => v.seller.id === authStore.userId) !== undefined;
  const hasItemToSell = store.isResellable && store.ownedCopies && store.ownedCopies.filter(v => v.price === null || v.price === 0).length > 0;
  const isCreator = store.details && store.details.creatorType === 'User' && store.details.creatorTargetId === authStore.userId; // todo: group support
  const showGear = hasItemToDeList ||
    hasItemToSell ||
    isCreator ||
    (store.ownedCopies && store.ownedCopies.length > 0) // Collection stuff

  if (!store.details) return null;

  const subTitle = `${store.subCategoryDisplayName}${(isLimited || isLimitedUnique) ? ' / Collectible Item' : ''}${isLimitedUnique ? ' / Limited Edition' : ''}`;
  
  return <div className='container ssp'>
    <AdBanner />
    <div className={s.catalogItemContainer}>
      <BuyItemModal/>
      <SellItemModal/>
      <DelistItemModal/>
      <RemoveItemModal/>
      <div className='row mt-4'>
        <div className='col-12 col-lg-10'>
          <div className='row'>
            <div className='col-10'>
              <h1 className={s.title}>{details.name}</h1>
              <h3 className={s.subtitle}>{subTitle}</h3>
            </div>
            <div className='col-2'>
              {
                showGear && <GearDropdown options={[
                  hasItemToDeList && {
                    name: 'Take Off Sale',
                    onClick: (e) => {
                      e.preventDefault();
                      store.setUnlistModalOpen(true);
                    },
                  },
                  hasItemToSell && {
                    name: 'Sell Item',
                    onClick: (e) => {
                      e.preventDefault();
                      store.setResaleModalOpen(true);
                    },
                  },
                  isCreator && {
                    name: 'Configure',
                    url: `/My/Item.aspx?id=${props.details.id}`,
                  },
                  isCreator && {
                    name: 'Advertise',
                    url: 'My/CreateUserAd.aspx?targetId=' + props.details.id + '&targetType=asset',
                  },
                  // store.ownedCopies > 0 && store.details.assetType === 21 && {
                  //   name: 'Remove Badge from Inventory',
                  //   onClick: (e) => {
                  //     e.preventDefault();
                  //     store.setRemoveItemModalOpen(true);
                  //   }
                  // },
                  store.inCollection ? {
                    name: 'Remove From Collection',
                    onClick: e => {
                      e.preventDefault();
                      store.setInCollection(false);
                      addOrRemoveFromCollections({
                        assetId: store.details.id,
                        addToProfile: false,
                      });
                    },
                  } : store.ownedCopies && store.ownedCopies.length > 0 ? {
                    name: 'Add To Collection',
                    onClick: e => {
                      e.preventDefault();
                      store.setInCollection(true);
                      addOrRemoveFromCollections({
                        assetId: store.details.id,
                        addToProfile: true,
                      });
                    },
                  } : null,
                ].filter(v => !!v)} />
              }
            </div>
          </div>
          <div className='col-12'>
            <div className='row'>
              <div className='col-12 col-md-6 col-lg-5'>
                <ItemImage id={details.id} name={details.name}/>
                {store.details.assetType === 3 ? <AudioPlayButton audioId={details.id} /> : null}
                {isLimitedUnique && <LimitedUniqueOverlay/> || isLimited && <LimitedOverlay/> || null}
                {
                  gamePassPlace ?
                    <span style={{ textAlign: 'center', width: '100%', display: 'inline-block', marginTop: 5 }} >Use this Game Pass in
                      <Link href={getGameUrl({ placeId: gamePassPlace.rootPlaceId, name: gamePassPlace.name })}>
                        <a style={{ marginLeft: 5 }} className='link2018' href={getGameUrl({ placeId: gamePassPlace.rootPlaceId, name: gamePassPlace.name })}>
                          {gamePassPlace.name}
                        </a>
                      </Link>
                    </span>
                    : null
                }
                {
                  badgeInfo ? (() => {
                    /** @type BadgeEntry */
                    const badge = badgeInfo;
                    return <span style={{textAlign: 'center', width: '100%', display: 'inline-block', marginTop: 5}}>Earn this Badge in
                      <Link href={getGameUrl({placeId: badge.awardingUniverse.rootPlaceId, name: badge.awardingUniverse.name})}>
                        <a style={{marginLeft: 3}} className='link2018'
                           href={getGameUrl({placeId: badge.awardingUniverse.rootPlaceId, name: badge.awardingUniverse.name})}>
                          {badge.awardingUniverse.name}
                        </a>
                      </Link>
                    </span>
                  })() : null
                }
              </div>
              <div className='col-12 col-md-6 col-lg-4'>
                <CreatorDetails
                    //gamePassPlace={gamePassPlace}
                    id={details.creatorTargetId}
                    name={details.creatorName}
                    type={details.creatorType}
                    createdAt={details.createdAt}
                    updatedAt={details.updatedAt}
                    owned={store.ownedCopies && store.ownedCopies.length > 0}
                />
                <p className={s.description}>{filterTextForEmpty(details.description)}</p>
                <ReportAbuse assetId={details.id}/>
              </div>
              <div className='col-12 col-lg-3'>
                <BuyButton/>
              </div>
            </div>
            {store.isResellable &&
              <div className='row'>
                <div className='col-12'>
                  <div className='divider-top mt-2'/>
                </div>
                <div className='col-6'>
                  <Resellers/>
                </div>
                <div className='col-6'>
                  <SaleHistory/>
                </div>
              </div>
            }
            <div className='row'>
              <div className='col-12 mt-4'>
                <OldVerticalTabs options={[
                  {
                    name: 'Recommendations',
                    element: <Recommendations assetId={details.id} assetType={details.assetType}/>,
                  },
                  {
                    name: 'Commentary',
                    element: <Comments assetId={details.id}/>
                  },
                  (isLimited || isLimitedUnique) && getFlag('catalogDetailsPageOwnersTabEnabled', false) && {
                    name: 'Owners',
                    element: <Owners assetId={details.id}/>,
                  },
                ].filter(v => !!v)} />
              </div>
            </div>
          </div>
        </div>
        <div className='col-12 col-lg-2'>
          <AdSkyscraper/>
        </div>
      </div>
    </div>
  </div>
}

export default CatalogDetails;
