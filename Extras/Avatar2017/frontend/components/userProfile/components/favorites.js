import { useEffect, useState } from "react";
import { getFavorites } from "../../../services/inventory";
import Subtitle from "./subtitle";
import SmallButtonLink from "./smallButtonLink";
import { createUseStyles } from "react-jss";
import GameRow from "./GameRow";
import SmallGameCard from "../../smallGameCard";
import { multiGetUniverseIcons } from "../../../services/thumbnails";
import SmallTextLink from "./smallTextLink";
import useButtonWrapperStyle from '../styles/buttonWrapper'
import { multiGetPlaceDetails, multiGetUniverseDetails } from "../../../services/games";

const Favorites = props => {
  const { userId } = props;
  const [favorites, setFavorites] = useState(null);
  const [icons, setIcons] = useState({});
  const s = useButtonWrapperStyle();
  useEffect(() => {
    setFavorites(null);
    getFavorites({
      userId: userId,
      limit: 6,
      assetTypeId: 9,
      cursor: '',
    }).then(data => {
      //setFavorites(data.Data.Items);
      multiGetPlaceDetails({
        placeIds: data.Data.Items.map(v => v.Item.AssetId)
      }).then(d => {
        setFavorites(d)
      })

      /*multiGetUniverseIcons({
        universeIds: data.Data.Items.map(v => v.Item.UniverseId),
      }).then(d => {
        let newObj = {}
        for (const item of d) {
          newObj[item.targetId] = item.imageUrl;
        }
        setIcons(newObj);
      })*/
    })
  }, [userId]);

  //if (favorites === null || favorites.length === 0) return null;
  return <div className='flex marginStuff'>
    <div className='col-10'>
      <Subtitle>Favorite Games</Subtitle>
    </div>
    <div className='col-2'>
      <div className={`${s.buttonWrapper}`}>
        <SmallTextLink href={`/users/${userId}/favorites`}>Favorites</SmallTextLink>
      </div>
    </div>
    <div className='col-12'>
      {
        favorites && favorites.length > 0 ? <GameRow games={favorites} />
          : <div className={`section-content-off`} >User has no favourited games.</div>
        /*favorites.slice(0,6).map(v => {
          return <SmallGameCard
        key={v.Item.AssetId}
        name={v.Item.Name}
        creatorId={v.Creator.Id}
        creatorName={v.Creator.Name}
        creatorType={v.Creator.Type}
        placeId={v.Item.AssetId}
        iconUrl={icons[v.Item.UniverseId]}
        year={v.year}
        hideVoting={true}
        playerCount={0}
      />
        })*/
      }
    </div>
  </div>
}

export default Favorites;