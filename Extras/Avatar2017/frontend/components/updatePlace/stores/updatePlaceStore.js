import {createContainer} from "unstated-next";
import {useState} from "react";
import {getGameMedia, multiGetPlaceDetails} from "../../../services/games";

const UpdatePlaceStore = createContainer(() => {
  const [tab, setTab] = useState(null);
  const [details, setDetails] = useState(null);
  const [locked, setLocked] = useState(false);
  const [placeId, setPlaceId] = useState(null);
  const [media, setMedia] = useState(null);
  
  const refreshPlaceDetails = (pId) => {
    multiGetPlaceDetails({placeIds: [pId]}).then(data => {
      setDetails(data[0]);
      refreshPlaceMedia(data[0].universeId);
    });
  }
  
  const refreshPlaceMedia = (universeId) => {
    getGameMedia({ universeId }).then(d1 => setMedia(d1));
  }
  
  return {
    locked,
    setLocked,

    tab,
    setTab,

    details,
    setDetails,

    placeId,
    setPlaceId,
    
    media,
    setMedia,
    
    refreshPlaceDetails,
    refreshPlaceMedia
  }
});

export default UpdatePlaceStore;