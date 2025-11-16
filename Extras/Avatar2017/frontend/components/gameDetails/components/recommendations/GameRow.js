import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import GameCard from '../../../newGameCard';
import ThumbnailStore from "../../../../stores/thumbnailStore";

const useStyles = createUseStyles({
    gameCardsContainer: {
        whiteSpace: 'nowrap',
        listStyle: 'none',
        margin: 0,
        padding: 0,
        flexDirection: 'row',
        '@media (max-width: 991px)': {
            overflow: 'auto',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '12px',
        },
    },
    gameCard: {

    }
});

/**
 * Recommendations
 * @param {{
* games: any[];
* listItemClass?: any;
* width?: number;
* }} props
* @returns 
*/

const GameRow = props => {
    const s = useStyles();
    const store = ThumbnailStore.useContainer();

    var customWidth = null;

    if (props.games) {
        customWidth = 1 / props.games.length;
    }

    return <ul className={`row ${s.gameCardsContainer}`}>
        {
            props.games && props.games.map((game) => {
                var thumbnail;
                var gameThumbnail = store.getGameIcon(game.universeId, '420x420');
                gameThumbnail ? thumbnail = gameThumbnail : thumbnail = '/img/placeholder/icon_one.png';
                return <GameCard
                    name={game.name}
                    playerCount={game.playerCount}
                    likes={game.totalUpVotes}
                    dislikes={game.totalDownVotes}
                    creatorId={game.creatorId}
                    creatorType={game.creatorType}
                    creatorName={game.creatorName}
                    iconUrl={thumbnail}
                    year={game.year}
                    placeId={game.placeId}
                    width={props?.width || customWidth}
                    className={props?.listItemClass}
                />
            })
        }
    </ul>
};

export default GameRow;