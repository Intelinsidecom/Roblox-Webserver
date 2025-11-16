import SmallGameCard from "../../newGameCard";
//import GameRow from "../../gameDetails/components/recommendations/GameRow";
import GameRow from "./gameRow";
//, { useStyles as useGameRowStyles } 
import React, { useEffect, useState } from "react";
import GamesPageStore from "../../../stores/gamesPage";
import Link from "../../link";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";
import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  listItem: {
    maxWidth: '162px',
    '@media(max-width: 994px)': {
      minWidth: '144px',
    },
  },
  filterHeader: {
    fontSize: '32px',
    fontWeight: 800,
    lineHeight: '1.4em',
    padding: 0,
  },
})

const formatSortFilter = str => {
  return str
    .split(/(?=[A-Z])/)
    .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
    .join(' ');
}

const Games = (props) => {
  const store = GamesPageStore.useContainer();
  //const gameS = useGameRowStyles();
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  let existingGames = {};

  if (store.infiniteGamesGrid) {
    if (store.infiniteGamesGrid.games.length === 0) {
      return <p className="mt-4">No results.</p>;
    }
    return (
      <div className="row">
        {store.sortToken && <h3 className={s.filterHeader}>{formatSortFilter(store.sortToken)}</h3>}
        {store.infiniteGamesGrid.games.map((v) => (
          <SmallGameCard
            key={v.universeId}
            placeId={v.placeId}
            creatorId={v.creatorId}
            creatorType={v.creatorType}
            creatorName={v.creatorName}
            iconUrl={store.icons[v.universeId]}
            year={v.year}
            likes={v.totalUpVotes}
            dislikes={v.totalDownVotes}
            name={v.name}
            className={s.listItem}
            playerCount={v.playerCount}
          />
        ))}
      </div>
    );
  }

  return (
    <div className="row">
      {store.sorts ? (
        store.sorts.map((v) => {
          if (existingGames[v.token]) {
            return null;
          }
          existingGames[v.token] = true;
          let games = store.games && store.games[v.token] || null;
          return <>
            {/*<GameRow listItemClass={s.listItem} key={v.token} games={games} />*/}
            <GameRow
              key={v.token}
              token={v.token}
              games={games}
              ads={true}
              title={v.displayName}
              icons={store.icons} />
          </>
        })
      ) : (
        <></>
      )}
    </div>
  );
};

export default Games;
