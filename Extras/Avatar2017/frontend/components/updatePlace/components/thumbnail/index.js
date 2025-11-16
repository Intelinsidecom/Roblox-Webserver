import { createUseStyles } from 'react-jss';
import { useState, useEffect } from 'react';
import ActionCalls from './actionCalls';
import updatePlaceStore from "../../stores/updatePlaceStore";
import FeedbackStore from "../../../../stores/feedback";
import {multiGetAssetThumbnails} from "../../../../services/thumbnails";
import {getGameMedia, getGameUrl} from "../../../../services/games";
import ActionButton from "../../../actionButton";
import buyButton from "../../../catalogDetailsPage/components/buyButton";
import useButtonStyles from "../../../../styles/buttonStyles";
import {deleteGameThumbnail} from "../../../../services/develop";
import {Random} from "../../../../lib/utils";
import {FeedbackType} from "../../../../models/feedback";

const useStyles = createUseStyles({
    contentContainer: {
        display: 'flex',
        flexDirection: 'column',
        paddingLeft: '12px',
        paddingRight: '12px',
        marginTop: '24px',
    },
    header: {
        '& h3': {
            fontWeight: '400!important',
            marginBottom: '1.5rem!important',
            fontSize: '2rem',
            lineHeight: '1.2',
        },
    },
    mainContainer: {
        flex: '0 0 auto',
        display: 'flex',
        flexDirection: 'row',
    },
    iconContainer: {
        display: 'flex',
        flexDirection: 'column',
        borderRight: '1px solid var(--text-color-secondary)',
        paddingRight: '20px',
        aspectRatio: '1.25/1'
    },
    gameIcon: {
        width: '100%!important',
        display: 'block!important',
        verticalAlign: 'middle',
        aspectRatio: '16/9',
    },
    noteText: {
        marginTop: '5px',
        marginBottom: '20px',
        fontSize: '10px',
        fontWeight: '500',
        lineHeight: '1.4em',
        display: 'block',
        width: '100%',
        fontStyle: 'italic',
        color: '#d2d2d2'
    },
    callsToAction: {
        display: 'flex',
        flexDirection: 'column',
        marginLeft: '20px',
        '& p': {
            marginBottom: '10px'
        }
    },
    feedback: {
        padding: '15px',
        backgroundColor: '#E2EEFE',
        border: '1px solid #6586A3',
        fontSize: '16px',
        fontWeight: '400',
        lineHeight: '1.4em',
    },
    footerContainer: {
        flex: '0 0 auto',
    },
    normal: {
        padding: '3px 18px'
    },
    mediaContainer: {
        width: '100%',
        display: 'flex',
        flexWrap: 'wrap',
        flexDirection: 'row',
        marginTop: 20,
        marginBottom: 50,
        gap: 10,
    },
    mediaWrapper: {
        width: 'calc(33% - 10px)',
        aspectRatio: '16 / 9',
        margin: 0,
        position: 'relative',
        '& img': {
            width: '100%',
            height: '100%',
            display: 'inline-block',
            cursor: 'pointer',
        },
        '& span': {
            position: 'absolute',
            top: -14,
            right: -14,
            zIndex: 3,
            cursor: 'pointer',
            backgroundColor: '#959595',
            borderRadius: '50%',
            filter: 'invert(1)',
        }
    },
})

const getSrcFromMedia = (selected, medias) => {
    switch (medias) {
        case 0:
            return "/img/loading.png";
        case 1:
            return "/img/error.png";
        case 2:
            return "/img/placeholder/icon_two.png";
        case 3:
            return "/img/placeholder.png";
        default:
            return medias[selected]?.imageUrl;
    }
}

const Thumbnail = () => {
    // based off these videos
    // https://www.youtube.com/watch?v=qFRaI7_OhOc
    // https://www.youtube.com/watch?v=m54ngsDywcQ
    const s = useStyles();
    const [deleteLock, setDeleteLock] = useState(false);
    // 0 == loading, 1 == failed to load, 2 == empty, array = success
    const [media, setMedia] = useState(0);
    // selected means which one is shown on the big picture
    const [selectedMedia, setSelectedMedia] = useState(0);
    const store = updatePlaceStore.useContainer();
    const feedback = FeedbackStore.useContainer();
    const buttonStyles = useButtonStyles();
    
    const refreshGameMedia = (reloadStoreMedia) => {
        if (reloadStoreMedia) {
            store.refreshPlaceMedia(store.details.universeId);
            // returning cuz whenn it changes it'll go back anyway
            return;
        }
        if (store?.media?.length === null) {
            setMedia(1);
            return;
        } else if (store.media.length === 0) {
            setMedia(2);
            return;
        }
        
        setMedia(0);
        setSelectedMedia(0);
        multiGetAssetThumbnails({ assetIds: store.media.map(media => media.imageId) }).then(res => {
            if (!res?.length) {
                setMedia(1);
                return;
            } else if (res.length === 0) {
                setMedia(2);
                return;
            }
            setMedia(res);
        });
    }
    
    useEffect(refreshGameMedia, [store.media]);
    
    return <div className={s.contentContainer}>
        <div className={`${s.header} col-12`}>
            <h3>Thumbnails</h3>
        </div>
        <div className={`${s.mainContainer} col-12`}>
            <div className={`${s.iconContainer} col-8`}>
                <img className={s.gameIcon} src={getSrcFromMedia(selectedMedia, media) || '/img/placeholder.png'} alt='Game Thumbnail'/>
                <div className={s.mediaContainer}>
                    {Array.isArray(media) ?
                        media.map((thumb, index) =>
                            <div className={s.mediaWrapper} style={{ height: (100 / (Math.floor((media.length - 1) / 3) + 1)) + '%' }} key={thumb.targetId}>
                                <img src={thumb.imageUrl || '/img/placeholder.png'} alt={`Game Media ID ${thumb.targetId}`} onClick={e => {
                                    e.preventDefault();
                                    setSelectedMedia(index);
                                }}/>
                                <span className='icon-close' onClick={e => {
                                    e.preventDefault();
                                    if (deleteLock) return;
                                    setDeleteLock(true);
                                    deleteGameThumbnail({
                                        universeId: store.details.universeId,
                                        thumbnailId: thumb.targetId
                                    }).then(() => {
                                        // timeout cuz it take ssome time to delete for some reason
                                        setTimeout(() => {
                                            feedback.addFeedback("Thumbnail successfully removed.", FeedbackType.SUCCESS);
                                            refreshGameMedia(true);
                                            setDeleteLock(false);
                                        }, Random(28, 19) * 100)
                                    });
                                }}/>
                            </div>
                        )
                        : null
                    }
                </div>
                {/*<p className={s.noteText}>Note: You can only have 1 thumbnail per game (for now).</p>*/}
            </div>
            <div className={`${s.callsToAction} col-4`}>
                <p style={{
                    fontSize: '18px',
                }}>Add a New Thumbnail</p>
                <p style={{
                    fontSize: '16px'
                }}>Media type:</p>
                <ActionCalls universeId={
                    store.details.universeId
                } feedback={feedback} refreshIcon={refreshGameMedia}/>
            </div>
        </div>
        <div className={`${s.footerContainer} col-12`}>
            <div className='d-inline-block'>
                <ActionButton disabled={store.locked} buttonStyle={buttonStyles.continueButton} className={s.normal}
                              label='Save'
                              onClick={() => {
                                  window.location.href = getGameUrl({placeId: store.placeId, name: 'placeholder'})
                              }}/>
            </div>
            <div className='d-inline-block ms-4'>
                <ActionButton disabled={store.locked} buttonStyle={buttonStyles.cancelButton} className={s.normal}
                              label='Cancel'
                              onClick={() => {
                                  window.location.href = getGameUrl({placeId: store.placeId, name: 'placeholder'})
                              }}/>
            </div>
        </div>
    </div>
};

export default Thumbnail;