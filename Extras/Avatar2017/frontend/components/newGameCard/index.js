import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getGameUrl } from "../../services/games";
import CreatorLink from "../creatorLink";
import Link from "../link";

const useStyles = createUseStyles({
    listItem: {
        float: 'none',
        padding: '0 11px 0 0',
        display: 'inline-block',
        margin: 0,
        marginBottom: '12px',
        verticalAlign: 'top',
        whiteSpace: 'normal',
        listStyle: 'none',
        aspectRatio: '164/223',
        width: '16.8%',
        minWidth: '120px',
    },
    gameCardContainer: {
        width: '100%',
        position: 'relative',
        boxShadow: '0 1px 4px 0 rgba(25,25,25,0.3)',
        backgroundColor: 'var(--white-color)',
        borderRadius: '3px',
        float: 'left',
        margin: '0 auto',
        padding: 0,
        textAlign: 'left',
        whiteSpace: 'normal',
        fontSize: '16px',
        fontWeight: '300',
        lineHeight: '1.3em',
        '&:hover': {
            transition: 'box-shadow 200ms ease',
            height: 'auto',
            zIndex: '999',
            boxShadow: '0 1px 6px 0 rgba(25,25,25,0.75)',
        },
        '&:hover div': {
            display: 'block',
        },
    },
    gameCardLink: {
        display: 'flex',
        textDecoration: 'none!important',
        color: 'inherit',
        cursor: 'pointer',
        flexDirection: 'column',
    },
    gameCardThumbContainer: {
        borderTopLeftRadius: '3px',
        borderTopRightRadius: '3px',
        position: 'relative',
        height: '100%',
        aspectRatio: '1 / 1',
        display: 'inline-block',
        '& img': {
            display: 'inline-block',
            borderTopLeftRadius: '3px',
            borderTopRightRadius: '3px',
            width: '100%',
            height: '100%',
            overflow: 'hidden',
            margin: 'auto',
            verticalAlign: 'middle',
            border: 0,
            overflowClipMargin: 'content-box',
        },
    },
    gameCardThumbnailContainer: {
        overflow: 'hidden',
        position: 'relative',
        '& img': {
            aspectRatio: '16/9',
            width: 'auto',
            position: 'absolute',
            top: 0,
            left: '-40%'
        }
    },
    gameCardTitle: {
        margin: '3px 0',
        marginTop: '6px',
        padding: '0 6px',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
        whiteSpace: 'nowrap',
        cursor: 'pointer',
        lineHeight: '1.3em',
    },
    gameCardPlaying: {
        fontSize: '12px',
        fontWeight: '400',
        color: 'var(--text-color-tertiary)',
        width: '100%',
        padding: '0 6px',
        margin: '3px 0',
        lineHeight: '1.3em',
    },
    gameCardVote: {
        padding: '0 6px',
        margin: '3px 0',
        width: '100%',
    },
    voteBar: {
        position: 'relative',
        height: '20px',
        '&::before,&::after': {
            content: " ",
            display: 'table',
        },
    },
    voteThumbsUp: {
        float: 'left',
        display: 'flex!important',
    },
    thumbsUpColored: {
        backgroundPosition: '-16px 0!important',
    },

    voteContainer: {
        float: 'left',
        width: 'calc(99% - 32px)',
        height: '6px',
        margin: '9px auto 0 1%',
        position: 'relative',
    },
    voteBackground: {
        backgroundColor: 'var(--text-color-secondary)',
        position: 'absolute',
        top: 0,
        left: 0,
        height: '100%',
        width: '100%',
    },
    hasVotes: { backgroundColor: 'var(--bad-color)', opacity: '.6' },
    hasVotesPercentage: {
        backgroundColor: '#02b757!important'
    },
    votePercentage: {
        backgroundColor: 'var(--text-color-tertiary)',
        position: 'absolute',
        top: 0,
        left: 0,
        height: '100%',
    },
    segment: {
        backgroundColor: 'var(--white-color)',
        height: '6px',
        width: '2px',
        position: 'absolute',
        top: 0,
    },
    seg1: { left: '18%' },
    seg2: { left: '38%' },
    seg3: { left: '58%' },
    seg4: { left: '78%' },

    voteThumbsDown: {
        float: 'left',
        visibility: 'hidden',
        opacity: '.6',
        display: 'flex!important',
        '& span': {
            backgroundPosition: '0 -16px',
        },
    },
    thumbsDownColored: {
        backgroundPosition: '-16px -16px!important',
        visibility: 'visible',
        opacity: '1',
    },

    voteCounts: {
        position: 'relative',
        margin: 'auto 0',
        padding: '0 2px 5px',
        width: '89%',
        display: 'none',
        cursor: 'pointer',
        '& div': {
            fontSize: '12px',
            fontWeight: '300',
            lineHeight: '1.3em',
        }
    },
    voteCountVisible: {
        display: 'flex!important',
        flexDirection: 'column',
        alignItems: 'center',
        float: 'left',
    },
    upvoteCount: {
        color: '#02b757',
        float: 'left',
    },
    downvoteCount: {
        opacity: '.6',
        color: 'var(--bad-color)',
        float: 'right',
    },
    visibleCounts: { display: 'block' },

    gameCardFooterContainer: {
        display: 'none',
        backgroundColor: 'var(--white-color)',
        boxShadow: '0px 1px 6px 0px rgba(25,25,25,0.75)',
        clipPath: 'inset(2px -10px -10px -10px)',
        borderBottomLeftRadius: '3px',
        borderBottomRightRadius: '3px',
        position: 'absolute',
        bottom: '-43px',
        width: '100%',
        flexDirection: 'column',
        alignItems: 'center'
    },
    gameCardFooter: {
        borderTop: '1px solid var(--background-color)',
        width: '100%',
        lineHeight: '1.3em',
        whiteSpace: 'normal',
    },
    gameCreator: {
        padding: '5px 5px 0',
    },
    creatorText: {
        display: 'inline',
        width: '100%',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
        whiteSpace: 'nowrap',
        color: 'var(--text-color-secondary)',
        fontWeight: '400',
        fontSize: '12px',
        lineHeight: '1.3em!important',
        marginLeft: 0,
        '& a': {
            display: 'inline',
            //marginLeft: '3px',
            marginLeft: 0,
            color: 'var(--primary-color)',
            textDecoration: 'none!important',
            '&:hover': {
                color: 'var(--primary-color)',
                textDecoration: 'underline!important'
            }
        }
    },

    yearText2: {
        position: 'absolute',
        bottom: '20px',
        right: '10px',
        backgroundColor: 'rgba(0, 0, 0, 0.7)',
        color: 'var(--text-color-primary)',
        padding: '5px',
        fontSize: '14px',
        borderRadius: '5px',
    },

    triangleRight: {
        borderRight: '35px solid var(--white-color)',
        borderTop: '30px solid transparent',
        display: 'inline-block',
        position: 'absolute',
        right: '40px',
        bottom: '-4px',
        zIndex: '2'
    },
    yearDiv: {
        position: 'absolute',
        right: 0,
        bottom: '-4px',
        height: '30px',
        width: '40px',
        backgroundColor: 'var(--white-color)',
        display: 'flex!important',
        alignItems: 'center',
        zIndex: '2'
    },
    yearText: {
        position: 'absolute',
        left: '-12px',
        fontSize: '21px',
        color: 'var(--text-color-primary)',
        zIndex: '2',
        userSelect: 'none',
        fontWeight: '300',
        marginTop: '4px',
    },

    playerCount: {
        float: 'left',
        width: '65%',
        userSelect: 'none',
        color: 'var(--text-color-primary)',
        marginBottom: 0,
    },
    yearText3: {
        float: 'right',
        marginLeft: 'auto',
        //width: '35%',
        fontWeight: '400',
        userSelect: 'none',
        color: 'var(--text-color-primary)',
        marginBottom: 0,
    },
});

function getImageAspectRatio(url) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            const aspectRatio = img.naturalWidth / img.naturalHeight;
            resolve(aspectRatio);
        };
        img.onerror = (error) => {
            reject(error);
        };
        img.src = url;
    });
}

/**
 * NewGameCard
 * @param {{
 * name: string;
 *  playerCount: number;
 *  likes: number;
 *  dislikes: number;
 *  creatorId: number;
 *  creatorType: string | number;
 *  creatorName: string;
 *  year?: number;
 *  placeId: number;
 *  iconUrl?: string;
 *  className?: string;
 *  hideVoting?: boolean;
 *  width?: number;
 * }} props
 * @returns 
 */
const NewGameCard = props => {
    const s = useStyles();
    const [iconUrl, setIconUrl] = useState('/img/placeholder/icon_one.png');
    const [isThumbnail, setIsThumbnail] = useState(false);
    const url = getGameUrl({
        placeId: props.placeId,
        name: props.name,
    });
    const { hideVoting } = props;
    useEffect(() => {
        if (!props.iconUrl) {
            setIconUrl('/img/placeholder/icon_one.png');
            return
        }
        setIconUrl(props.iconUrl);
        getImageAspectRatio(iconUrl).then(aspectRatio => {
            if (aspectRatio === 1) { // game icon
                setIsThumbnail(false)
            } else { // thumbnail
                setIsThumbnail(true)
            }
        })
    }, [props.iconUrl]);

    const [linkHover, setLinkHover] = useState(false);
    const linkHoverEnable = () => { setLinkHover(true); }
    const linkHoverDisable = () => { setLinkHover(false); }

    const [containerHover, setContainerHover] = useState(false);
    const hoverEnable = () => { setContainerHover(true); }
    const hoverDisable = () => { setContainerHover(false); }

    const total = props.likes + props.dislikes;
    const greenPercent = Math.ceil((props.likes / total) * 100);

    const gameFooterClass = (containerHover) ? s.voteCountVisible : '';
    const voteCountsClass = (containerHover) ? s.visibleCounts : '';
    const hasVotes = (containerHover && total > 0) ? s.hasVotes : '';
    const thumbsUpClass = (linkHover) ? s.thumbsUpColored : '';
    const thumbsDownClass = (linkHover) ? s.thumbsDownColored : '';
    const votePercentageClass = (linkHover) ? s.hasVotesPercentage : '';
    const guh = createUseStyles({ width: { width: `calc(100 * ${props.width}%)` } });
    const s2 = guh();
    const thumbnail = isThumbnail ? s.gameCardThumbnailContainer : '';

    const width = props?.width ? s2.width : '';

    return <li className={`${s.listItem} ${props.className} ${width}`}
    >
        <div className={s.gameCardContainer} onTouchStart={hoverEnable} onTouchEnd={hoverDisable} onMouseEnter={hoverEnable} onMouseLeave={hoverDisable}>
            <Link href={url}>
                <a href={url} className={s.gameCardLink} onTouchStart={linkHoverEnable} onTouchEnd={linkHoverDisable} onMouseEnter={linkHoverEnable} onMouseLeave={linkHoverDisable}>
                    <div className={`${s.gameCardThumbContainer} ${thumbnail}`}>
                        {/*<div>
                        <div className={s.triangleRight}></div>
                        <div className={s.yearDiv}>
                            <div className={s.yearText}>{props.year}</div>
                        </div>
                    </div>*/}
                        <img src={iconUrl} />
                    </div>
                    <div className={s.gameCardTitle} title={props.name}>{props.name}</div>
                    <div className={s.gameCardPlaying}>
                        <p className={s.playerCount}>{props.playerCount} Playing</p>
                        <p className={s.yearText3}>{props?.year || '????'}</p>
                    </div>
                    <div className={s.gameCardVote}>
                        <div className={s.voteBar}>
                            <div className={s.voteThumbsUp}>
                                <span className={`icon-thumbs-up ${thumbsUpClass}`}></span>
                            </div>

                            <div className={s.voteContainer}>
                                <div className={`${s.voteBackground} ${hasVotes}`}></div>
                                <div className={`${s.votePercentage} ${votePercentageClass}`} style={{ width: greenPercent + '%' }}></div>
                                <div className=" ">
                                    <div className={s.segment + ' ' + s.seg1}></div>
                                    <div className={s.segment + ' ' + s.seg2}></div>
                                    <div className={s.segment + ' ' + s.seg3}></div>
                                    <div className={s.segment + ' ' + s.seg4}></div>
                                </div>
                            </div>

                            <div className={s.voteThumbsDown}>
                                <span className={`icon-thumbs-down ${thumbsDownClass}`}></span>
                            </div>
                        </div>
                    </div>
                </a>
            </Link>
            <div className={`${s.gameCardFooterContainer} ${gameFooterClass}`}>
                <div className={`${s.voteCounts} ${voteCountsClass}`}>
                    <div className={s.downvoteCount}>{props.dislikes}</div>
                    <div className={s.upvoteCount}>{props.likes}</div>
                </div>
                <div className={s.gameCardFooter}>
                    <div className={s.gameCreator}>
                        <span className={s.creatorText}>By <CreatorLink type={props.creatorType} name={props.creatorName} id={props.creatorId} /></span>
                    </div>
                </div>
            </div>
        </div>
    </li>
}

export default NewGameCard;