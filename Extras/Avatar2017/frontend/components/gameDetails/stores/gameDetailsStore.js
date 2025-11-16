import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import { getGameMedia, multiGetPlaceDetails, multiGetUniverseDetails } from "../../../services/games";
import {getUniverseBadges} from "../../../services/badges";

const GameDetailsStore = createContainer(() => {
  const [details, setDetails] = useState(null);
  const [media, setMedia] = useState(null);
  const [placeDetails, setPlaceDetails] = useState(null);
  const [universeDetails, setUniverseDetails] = useState(null);
  const [servers, setServers] = useState(null);
  const [badges, setBadges] = useState(null);
  const [badgeLock, setBadgeLock] = useState(false);
  const [year, setYear] = useState(null);

  useEffect(() => {
    // reset our states, then get new details
    setMedia(null);
    setPlaceDetails(null);
    setUniverseDetails(null);

    if (!details) return;

    multiGetPlaceDetails({
      placeIds: [details.id],
    }).then(d => setPlaceDetails(d[0]));
  }, [details]);

  useEffect(() => {
    if (!placeDetails) return;
    multiGetUniverseDetails({
      universeIds: [placeDetails.universeId],
    }).then(d => {
      setUniverseDetails(d[0]);
      getGameMedia({
        universeId: d[0].id,
      }).then(setMedia);
      getUniverseBadges({ universeId: d[0].id, limit: 3 }).then(setBadges);
    })
  }, [placeDetails]);
  
  // Loads 25 more badges.
  const loadBadges = () => {
    if (!badges?.data || badgeLock) return;
    setBadgeLock(true);
    getUniverseBadges({ universeId: universeDetails.id, limit: 25, cursor: badges.nextPageCursor }).then(d => {
      setBadges(prevBadges => ({
        data: [...(prevBadges?.data || []), ...d.data],
        nextPageCursor: d.nextPageCursor,
        previousPageCursor: d.previousPageCursor
      }));
      setTimeout(() => setBadgeLock(false), 2500);
    });
  };

  return {
    /**
     * @type AssetDetailsEntry
     */
    details,
    setDetails,

    servers,
    setServers,
    
    /**
     * @type PlaceDetails
     */
    placeDetails,
    
    /**
     * @type UniverseDetails
     */
    universeDetails,
    setUniverseDetails,

    media,
    setMedia,
    
    /**
     * @type PekoraCollectionPaginated<BadgeEntry>
     */
    badges,
    setBadges,
    loadBadges
  }
});

export default GameDetailsStore;