// loads and styles game thumbnails

import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";

import GameDetailsStore from "../../stores/gameDetailsStore";
import {getGameMedia} from "../../../../services/games";
import {multiGetAssetThumbnails, multiGetUserThumbnails} from "../../../../services/thumbnails";

const useStyles = createUseStyles({
    innerCarousel: {
        overflow: 'hidden',
        width: '100%',
        height: '100%',
        position: 'relative',
        '&:hover $carouselControl': {
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center'
        }
    },
    carouselItem: {
        transition: 'opacity 1s ease-in-out',
        opacity: 0,
        top: 0,
        left: 0,
        width: '100%',
        height: '100%',
        display: 'block',
        position: 'absolute',
        zIndex: 0,
        transform: 'translate3d(0,0,0)',
        '& img':{
            width: '100%',
            height: '100%',
            verticalAlign: 'middle',
            border: 0,
        },
    },
    activeCarouselItem: {
        opacity: 1,
        zIndex: 3,
        transition: 'opacity .5s ease-in-out',
    },
    nextCarouselItem: {
        transition: 'opacity 1s ease-in-out',
    },
    carouselControl: {
        position: 'absolute',
        top: 'calc(50% - 24px)',
        backgroundColor: 'rgba(25, 25, 25, 0.3)',
        border: '2px solid rgba(255, 255, 255, 1.0)',
        borderRadius: '50%',
        height: '48px',
        width: '48px',
        userSelect: 'none',
        cursor: 'pointer',
        //textAlign: 'center',
        zIndex: 1000,
        display: 'none',
        '&:hover': {
            backgroundColor: 'rgba(25, 25, 25, 0.75)',
        }
    },
});

/**
 * @param {{styles: string; imageLink: string; status: string; active: boolean;}} props
 * @returns {JSX.Element}
 * @constructor
 */
const CarouselThumbnail = props => {
    // let first;
    // isFirst ? first = useStyles().activeCarouselItem : first = ' ';
    // return <div className={useStyles().carouselItem + ' ' + first+ ' ' + css}>
    //     <span>
    //         <img src={imageLink} alt={"Game Thumbnail"}></img>
    //     </span>
    // </div>
    return <div className={`${props.styles.carouselItem} ${props.active && props.styles.activeCarouselItem}`}>
        <span>
            <img src={props.status === 'Pending' ? "/img/placeholder.png" : props.status === 'Blocked' ? "/img/blocked.png" : props.imageLink} alt={"Game Thumbnail"} />
        </span>
    </div>
}

const gameThumbnails = props => {
    const store = GameDetailsStore.useContainer();
    // 0 === loading, 1 === use placeholder, 2 === failed to load, 3 === moderated, array is succedss
    /** @type {useState<AssetThumbnail[]>} */
    const [images, setImages] = useState([]);
    // 0 === loading, 1 === completed
    const [imageStatus, setImageStatus] = useState(0);
    const [currentImage, setCurrentImage] = useState(0);
    const s = useStyles();
    
    useEffect(() => {
        // erm wtf is this code
        if (!store.universeDetails || !store.universeDetails.id || !store.placeDetails || !store.placeDetails.placeId || !store.media || !store.media.length) {
            setImageStatus(2);
            return;
        } else if (store.media.length === 0) {
            setImageStatus(1);
            return;
        }
        
        multiGetAssetThumbnails({ assetIds: store.media.filter(v => v.approved).map(media => media.imageId) }).then(res => {
            setImageStatus(1);
            setImages(res);
        });
        // multiGetAssetThumbnails({ assetIds: [store.placeDetails.placeId] }).then(thumbs => {
        //     if (thumbs && thumbs.length && thumbs.length > 0) {
        //         setImages(thumbs);
        //         setImageStatus(1);
        //     }
            // /**
            //  * @type AssetThumbnail[]
            //  */
            // const thumbs = thumbs1;
            // if (!thumbs || !thumbs.length) {
            //     setImages(2);
            //     return;
            // } else if (thumbs.length === 1 && thumbs[0].state === "Pending") {
            //     setImages(1);
            //     return;
            // } else if (thumbs.length > 0) {
            //     setImages(thumbs);
            //     console.log(thumbs);
            //     console.log(images);
            //     return;
            // }
            // setImages(2);
        //})
    }, [store.universeDetails, store.details, store.placeDetails, store.media]);
    
    const changeCurrentImage = (e, forward) => {
        e.preventDefault();
        if (forward) {
            // check if current image is at the end of the images. if it is,
            // loop back to beginning and return
            if (currentImage === images.length - 1) {
                setCurrentImage(0);
                return;
            }
            setCurrentImage(currentImage + 1);
        } else {
            // check if current image is at the start of the images. if it is,
            // loop to end and return
            if (currentImage === 0) {
                setCurrentImage(images.length - 1);
                return;
            }
            setCurrentImage(currentImage - 1);
        }
    }
    
    return <div className={s.innerCarousel}>
        {
            /*images.forEach(image => {
                addCarouselImage(image, ' ', isFirstOne);
                isFirstOne ? isFirstOne = false  : null;
            })*/
        }
        {/*
            imageUrl ? addCarouselImage(imageUrl, ' ', true) : addCarouselImage('/img/placeholder/icon_two.png', ' ', true)
        */}
        {
            images.length > 0 && imageStatus === 1 ?
                images.map((thumb, index) =>
                    <CarouselThumbnail
                        key={thumb.imageUrl}
                        status={thumb.state}
                        imageLink={thumb.imageUrl}
                        styles={s}
                        active={currentImage === index}
                    />
                )
                : <CarouselThumbnail styles={s} active={true} status={'Completed'}
                                     imageLink={"/img/" + (imageStatus === 0 ? "loading.png" : "placeholder/icon_two.png") }
                />
        }
        {
            images.length > 1 && <>
                <div onClick={e => changeCurrentImage(e, false)} className={s.carouselControl} style={{left: 18}}>
                    <span className='icon-carousel-left'/>
                </div>
                <div onClick={e => changeCurrentImage(e, true)} className={s.carouselControl} style={{right: 18}}>
                    <span className='icon-carousel-right'/>
                </div>
            </>
        }
    </div>
}

export default gameThumbnails;