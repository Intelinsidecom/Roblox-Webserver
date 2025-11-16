import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getRecommendedGames } from '../../../../services/games';
import GameRow from './GameRow';

const useStyles = createUseStyles({
    recommendedGamesContainer: {
        padding: 0,
        //margin: '0 0 18px',
        margin: 0,
        width: '100%',
        float: 'left',
        position: 'relative',
        minHeight: '1px',
        display: 'flex',
        flexDirection: 'column',
        '& div, & ul': {
            '&::before,&::after': {
                content: " ",
                display: 'table',
            }
        }
    },
    containerHeader: {
        margin: '3px 0 6px',
        '@media (max-width: 991px)': {
            //margin: '0 auto',
        },
        '& h3': {
            float: 'left',
            margin: 0,
            textAlign: 'center',
            fontSize: '20px',
            fontWeight: '700',
            lineHeight: '1em',
            padding: '5px 0',
        }
    },
    marginBottom: {
        marginBottom: '6px',
        maxWidth: '166px',
        '@media (max-width: 991px)': {
            padding: 0,
        },
    }
});

/**
 * Recommendations
 * @param {{
* assetId: number;
* assetType?: number;
* }} props
* @returns 
*/
const Recommendations = props => {
    const s = useStyles();
    var [recomms, setRecomms] = useState(null);

    recomms == null && getRecommendedGames({
        placeId: props.assetId,
        limit: 6,
    }).then(result => {
        setRecomms(result.games);
    });

    return <div className={s.recommendedGamesContainer}>
        <div className={s.containerHeader}>
            <h3>Recommended Games</h3>
        </div>
        {recomms && <GameRow listItemClass={s.marginBottom} games={recomms} />}
    </div>
};

export default Recommendations;