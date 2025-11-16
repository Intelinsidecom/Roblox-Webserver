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
        maxHeight: '100%'
    },
    carouselItem: {
        transition: 'opacity .5s ease-in-out',
        opacity: 0,
        top: 0,
        left: 0,
        width: '100%',
        display: 'block',
        position: 'absolute',
        zIndex: 0,
        transform: 'translateZ(0)',
        '& img':{
            width: '100%',
            verticalAlign: 'middle',
            border: 0,
        },
    },
    activeCarouselItem: {
        opacity: 1,
        //zIndex: 3
      },
});

const addCarouselImage = (imageLink, css, isFirst) => {
    let first;
    isFirst ? first = useStyles().activeCarouselItem : first = ' ';
    return <div className={useStyles().carouselItem + ' ' + first+ ' ' + css}>
        <span>
            <img src={imageLink} alt={"Game Thumbnail"}></img>
        </span>
    </div>
}

const gameThumbnails = props => {
    const store = GameDetailsStore.useContainer();
    const [imageUrl, setImageUrl] = useState(null);
    const [images, setImages] = useState(null);
    const s = useStyles();

    useEffect(() => {
        if (!store.universeDetails || !store.universeDetails.id)
          return;
    
        getGameMedia({universeId: store.universeDetails.id}).then(media => {
          const images = media.filter(v => v.assetType === 'Image');
          if (images.length) {
            multiGetAssetThumbnails({assetIds: images.map(v => v.imageId)}).then(thumb => {
              // Default to first thumbnail
              setImageUrl(thumb[0].imageUrl);
              setImages(thumb.map(v => v.imageUrl));
            })
          } else {
            //setImageUrl('/img/placeholder/icon_two.png');
            //setImages('/img/placeholder/icon_two.png');
            multiGetAssetThumbnails({assetIds: [store.universeDetails.rootPlaceId]}).then(thumb => {
                if (thumb.length) {
                  setImageUrl(thumb[0].imageUrl);
                  setImages([thumb[0].imageUrl]);
                } else {
                  setImageUrl('/img/placeholder/icon_two.png');
                  setImages('/img/placeholder/icon_two.png');
                }
              })
          }
        })
      }, [store.universeDetails, store.details]);
    
    return <div className={s.innerCarousel}>
        {
            /*images.forEach(image => {
                addCarouselImage(image, ' ', isFirstOne);
                isFirstOne ? isFirstOne = false  : null;
            })*/
        }
        {imageUrl ? addCarouselImage(imageUrl, ' ', true) : addCarouselImage('/img/placeholder/icon_two.png', ' ', true)}
    </div>
}

export default gameThumbnails;