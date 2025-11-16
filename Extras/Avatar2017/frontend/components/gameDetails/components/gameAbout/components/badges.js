import GameDetailsStore from "../../../stores/gameDetailsStore";
import {createUseStyles} from "react-jss";
import {useEffect} from "react";
import ActionButton from "../../../../actionButton";
import useButtonStyles from "../../../../../styles/buttonStyles";
import Link from "../../../../link";
import ItemImage from "../../../../itemImage";
import {getGameUrl, getLibraryItemUrl} from "../../../../../services/games";
import {getItemUrl} from "../../../../../services/catalog";

const getRarityLabel = (percentage) => {
    if (percentage >= 90 && percentage <= 100) return "Freebie";
    if (percentage >= 80 && percentage < 90) return "Cake Walk";
    if (percentage >= 50 && percentage < 80) return "Easy";
    if (percentage >= 30 && percentage < 50) return "Moderate";
    if (percentage >= 20 && percentage < 30) return "Challenging";
    if (percentage >= 10 && percentage < 20) return "Hard";
    if (percentage >= 5 && percentage < 10) return "Extreme";
    if (percentage >= 1 && percentage < 5) return "Insane";
    if (percentage >= 0 && percentage < 1) return "Impossible";
    return null;
}

const useEntryStyles = createUseStyles({
    badgeContainer: {
        display: 'flex',
        padding: 12,
        boxShadow: 'none',
        margin: 0,
    },
    badgeImageContainer: {
        width: '150px',
        '& img': {
            width: '150px',
            height: '150px',
            display: 'inline-block',
            verticalAlign: 'middle',
            padding: 0,
            margin: 0
        }
    },
    badgeContentContainer: {
        padding: '0 12px',
        width: 'calc(100% - 150px)',
        display: 'flex',
        //justifyContent: 'space-between',
        flexDirection: 'column',
    },
    badgeDetailsContainer: {
        width: '100%',
        display: 'flex',
        flexDirection: 'column',
        '& a': {
            fontWeight: 500,
            textAlign: 'start',
            color: 'inherit',
        },
        '& p': {
            height: '48px',
            margin: '6px 0',
            wordWrap: 'break-word',
            fontWeight: 400,
            lineHeight: 1.5,
            overflow: 'hidden',
            whiteSpace: 'normal',
            hyphens: 'none',
            textAlign: 'start'
        }
    },
    badgeStatsContainer: {
        width: '100%',
        display: 'flex',
        //justifyContent: 'space-between',
        alignItems: 'center',
        margin: '6px 0 0',
    },
    badgeStatContainer: {
        width: '33.3%',
        margin: '6px 0 0',
    },
    badgeStatField: {
        textAlign: 'center',
        color: '#b8b8b8',
        fontWeight: 500
    },
    badgeStatValue: {
        textAlign: 'center',
        margin: '6px 0 0'
    },
});

/**
 * @param {BadgeEntry} badge
 * @returns {JSX.Element}
 * @constructor
 */
const BadgeEntry = ({badge}) => {
    const s = useEntryStyles();
    const itemUrl = getLibraryItemUrl({ assetId: badge.id, name: badge.name });
    
    return <div className={`section-content ${s.badgeContainer}`}>
        <div className={s.badgeImageContainer}>
            <Link href={itemUrl}>
                <a href={itemUrl}>
                    <ItemImage id={badge.id} name={badge.name} />
                </a>
            </Link>
        </div>
        <div className={s.badgeContentContainer}>
            <div className={s.badgeDetailsContainer}>
                <Link href={itemUrl}>
                    <a href={itemUrl}>{badge.name}</a>
                </Link>
                <p>{badge.description}</p>
            </div>
            <div className={s.badgeStatsContainer}>
                <div className={s.badgeStatContainer}>
                    <div className={s.badgeStatField}>Rarity</div>
                    <div className={s.badgeStatValue}>{badge.statistics.winRatePercentage}% ({getRarityLabel(badge.statistics.winRatePercentage)})</div>
                </div>
                <div className={s.badgeStatContainer}>
                    <div className={s.badgeStatField}>Won Last 24hr</div>
                    <div className={s.badgeStatValue}>{badge.statistics.pastDayAwardedCount}</div>
                </div>
                <div className={s.badgeStatContainer}>
                    <div className={s.badgeStatField}>Won Ever</div>
                    <div className={s.badgeStatValue}>{badge.statistics.awardedCount}</div>
                </div>
            </div>
        </div>
    </div>
}

const useStyles = createUseStyles({
    descriptionText: {
        whiteSpace: 'break-spaces',
    },
    badgeList: {
        display: 'flex',
        gap: 6,
        flexDirection: 'column',
    },
    seeMoreButton: {
        padding: 7,
        width: '100%',
        fontWeight: 500,
        fontSize: '16px',
        lineHeight: '100%',
    },
});

const Badges = () => {
    const store = GameDetailsStore.useContainer();
    const badges = store.badges;
    const buttonStyles = useButtonStyles();
    const s = useStyles();
    
    useEffect(() => {
    }, [badges]);
    
    
    return <div className={s.badgeList}>
        {
            badges.data.filter(badge => badge.enabled === true).map(badge => <BadgeEntry badge={badge}/>)
        }
        {
            badges.nextPageCursor !== null &&
            <div>
                <ActionButton className={s.seeMoreButton} buttonStyle={buttonStyles.newCancelButton} label='See More'
                              onClick={e => {
                                  e.preventDefault();
                                  store.loadBadges();
                              }}
                />
            </div>
        }
    </div>
}

export default Badges;