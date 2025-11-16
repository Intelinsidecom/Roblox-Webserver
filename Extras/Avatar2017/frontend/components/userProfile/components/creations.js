import { useEffect, useState } from "react";
import { multiGetUniverseIcons } from "../../../services/thumbnails";
import SmallGameCard from "../../smallGameCard";
import UserProfileStore from "../stores/UserProfileStore";
import Subtitle from "./subtitle";
import NewGameCard from "../../newGameCard";
import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  listItem: {
    padding: 0,
    width: 'calc(16.67% - 10px)',
    '@media(max-width: 767px)': {
      minWidth: '144px',
    },
  },
})

const Creations = props => {
  const store = UserProfileStore.useContainer();
  const s = useStyles();
  const [icons, setIcons] = useState({});
  useEffect(() => {
    if (!store.createdGames || store.createdGames.length === 0) return
    multiGetUniverseIcons({
      universeIds: store.createdGames.map(v => v.id),
      size: '150x150',
    }).then(data => {
      let obj = {};
      data.forEach(v => {
        obj[v.targetId] = v.imageUrl;
      });
      setIcons(obj);
    })
  }, []);
  if (!store.createdGames || store.createdGames.length === 0) {
    return null;
  }

  return <div className='flex'>
    <div className='col-12'>
      <Subtitle>Games</Subtitle>
    </div>
    <div className='col-12'>
      <div className='flex' style={{ gap: 10 }}>
        {
          store.createdGames.map(v => {
            return <NewGameCard key={v.id}
              name={v.name}
              likes={0}
              dislikes={0}
              playerCount={0}
              placeId={v.rootPlace.id}
              iconUrl={icons[v.id]}
              year={v.year}
              creatorType='User'
              creatorId={store.userId}
              creatorName={store.username}
              className={s.listItem}
            ></NewGameCard>
          })
        }
      </div>
    </div>
  </div>
}

export default Creations;