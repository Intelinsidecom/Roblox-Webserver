import {createContainer} from "unstated-next";
import {useState} from "react";
import {setAssetPrice, updateAsset} from "../../../services/develop";
import getFlag from "../../../lib/getFlag";
import {getBadgeInfoBasicByID, updateBadge} from "../../../services/badges";
import FeedbackStore from "../../../stores/feedback";

const ConfigureItemStore = createContainer(() => {
  const [assetId, setAssetId] = useState(null);
  const [details,setDetails] = useState(null);
  const [error, setError] = useState(null);
  const [locked, setLocked] = useState(false);
  
  const [name, setName] = useState(null);
  const [description, setDescription] = useState(null);
  // 0 = free, null = unset
  const [price, setPrice] = useState(null);
  const [priceTickets, setPriceTickets] = useState(null);
  const [isForSale, setIsForSale] = useState(false);
  const [commentsEnabled, setCommentsEnabled] = useState(false);
  const [genres, setGenres] = useState(null);
  
  const [badgeInfo, setBadgeInfo] = useState(null);
  const [enabled, setEnabled] = useState(false);
  const feedback = FeedbackStore.useContainer();
  
  const priceChanged = () => {
    return details.price !== price;
  }
  
  const assetChanged = () => {
    return details.description !== description ||
        details.name !== name ||
        details.commentsEnabled !== commentsEnabled ||
        details.genres !== genres ||
        details.isForSale !== isForSale;
  }
  
  const save = () => {
    if (locked)
      return;
    setLocked(true);
    let values = [];
    
    if (priceChanged()) {
      values = [...values, setAssetPrice({
        assetId,
        priceInRobux: Number.isSafeInteger(price) ? price : null,
        priceInTickets: Number.isSafeInteger(parseInt(priceTickets, 10)) ? priceTickets : null,
      })]
    }
    if (assetChanged()) {
      values = [...values, updateAsset({
        assetId,
        name,
        description,
        enableComments: commentsEnabled,
        genres,
        isForSale,
        // TODO: everything below this comment
        isCopyingAllowed: false,
      })]
    }
    if (badgeInfo && badgeInfo.enabled !== enabled) {
      values = [...values, updateBadge({ badgeId: assetId, enabled })]
    }
    
    if (values.length > 0) {
      Promise.all(values).then(() => setLocked(false)).catch(e => {
        feedback.addFeedback(e.message);
      })
    } else {
      setLocked(false);
    }
  }
  
  return {
    assetId,
    setAssetId,
    
    details,
    setDetails: (newDetails) => {
      setDetails(newDetails);
      if (!newDetails)
        return;
      setName(newDetails.name);
      setDescription(newDetails.description);
      setIsForSale(newDetails.isForSale);
      setPrice(newDetails.price);
      setCommentsEnabled(newDetails.commentsEnabled);
      setGenres(newDetails.genres);
      if (newDetails.assetType === 21) {
        getBadgeInfoBasicByID({ badgeId: newDetails.id }).then(res => {
          setBadgeInfo(res);
          setEnabled(res.enabled);
        })
      }
      if (getFlag('sellItemForTickets', false)) {
        setPriceTickets(newDetails.priceTickets);
      }
    },
    
    error,
    setError,
    
    name,
    setName,
    
    description,
    setDescription,
    
    price,
    setPrice,
    
    priceTickets,
    setPriceTickets,
    
    isForSale,
    setIsForSale,
    
    commentsEnabled,
    setCommentsEnabled,
    
    locked,
    setLocked,
    
    genres,
    setGenres,
    
    enabled,
    setEnabled,
    
    badgeInfo,
    
    save
  }
});

export default ConfigureItemStore;