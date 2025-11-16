import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import GameCard from '../../newGameCard';
import ThumbnailStore from "../../../stores/thumbnailStore";
import { multiGetGameVotes } from "../../../services/games";
import { multiGetAssetThumbnails, multiGetUniverseIcons2 } from "../../../services/thumbnails";
import { ThumbnailFromState } from "../../AvatarEditorPage/components/avatarCardList";

const useStyles = createUseStyles({
    gameCardsContainer: {
        whiteSpace: 'nowrap',
        listStyle: 'none',
        margin: 0,
        padding: 0,
        flexDirection: 'row',
        gap: '12px',
        height: '250px',
    },
});

/**
 * Recommendations
 * @param {{
* games: any[];
* }} props
* @returns
*/

const GameRow = props => {
    const s = useStyles();
    const store = ThumbnailStore.useContainer();
    const [games, setGames] = useState([]);
    
    let customWidth = null;
    
    useEffect(() => {
        if (props.games) {
            customWidth = 1 / props.games.length;
            //if (games.length !== 0) {
            const universeIds = props.games.map(game => game.universeId);
            const gamesNew = [];
            multiGetGameVotes({ universeIds }).then(votes => {
                multiGetUniverseIcons2({universeIds: universeIds}).then(universes => {
                    props.games.map(game => {
                        const voteData = votes.find(vote => vote.id === game.universeId);
                        const iconData = universes.find(thumb => thumb.targetId === game.universeId);
                        if (voteData && iconData) {
                            gamesNew.push({
                                ...game,
                                ...iconData,
                                totalUpVotes: voteData.upVotes,
                                totalDownVotes: voteData.downVotes,
                            })
                        } else if (voteData && !iconData) {
                            gamesNew.push({
                                ...game,
                                totalUpVotes: voteData.upVotes,
                                totalDownVotes: voteData.downVotes,
                            })
                        } else if (!voteData && iconData) {
                            gamesNew.push({
                                ...game,
                                ...iconData,
                            })
                        } else {
                            gamesNew.push(game);
                        }
                    });
                    setGames(gamesNew);
                })
            });
            //}
        }
    }, [props])

    return <ul className={s.gameCardsContainer}>
        {
            games.map(game => {
                // const gameThumbnail = store.getGameIcon(game.placeId, '420x420');
                // gameThumbnail ? thumbnail = gameThumbnail : thumbnail = '/img/placeholder/icon_one.png';
                const thumbnail =
                    game?.state && game?.imageUrl
                    ?
                    ThumbnailFromState(game.imageUrl, game.state)
                    :
                    "/img/placeholder/icon_one.png";
                return <GameCard
                    name={game.name}
                    playerCount={game.playerCount || '?'}
                    likes={game.totalUpVotes || 0}
                    dislikes={game.totalDownVotes || 0}
                    creatorId={game.builderId}
                    creatorType={game.builderType}
                    creatorName={game.builder}
                    iconUrl={thumbnail}
                    year={game.year || 2012}
                    placeId={game.placeId}
                    width={customWidth}
                />
            })
        }
    </ul>
};

export default GameRow;