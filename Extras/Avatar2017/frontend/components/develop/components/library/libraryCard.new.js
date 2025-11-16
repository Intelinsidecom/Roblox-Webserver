import dayjs from "dayjs";
import React, { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { getLibraryItemUrl } from "../../../../services/catalog";
import CreatorLink from "../../../creatorLink";
import thumbnailStore from "../../../../stores/thumbnailStore";
import Link from "../../../link";

const useCatalogPageStyles = createUseStyles({
  image: {
    width: '100%',
    height: 'auto',
    border: '1px solid #eee',
    margin: '0 auto',
    display: 'block',
  },
  imageBig: {
    maxWidth: '152px',
    maxHeight: '152px',
    display: 'block',
    margin: '0 auto',
  },
  imageSmall: {
    maxWidth: '112px',
    maxHeight: '112px',
    display: 'block',
    margin: '0 auto',
  },
  detailsLarge: {

  },
  detailsSmall: {

  },
  detailsWrapper: {
    position: 'absolute',
    display: 'none',
    background: 'var(--white-color)',
    paddingBottom: '5px',
    border: '1px solid var(--text-color-quinary)',
    transform: 'scale(110%)',
    zIndex: 2,
    paddingLeft: '4px',
  },
  detailsOpen: {
    display: 'block',
    marginTop: '-10px',
  },
  detailsKey: {
    fontSize: '10px',
    marginLeft: '0',
    marginRight: '2px',
    opacity: 0.7,
    fontWeight: 600,
  },
  detailsValue: {
    fontSize: '10px',
  },
  detailsEntry: {
    marginBottom: 0,
    marginTop: '-5px',
  },
  overviewDetails: {

  },
  itemName: {
    overflow: 'hidden',
  },
});

const LibraryCard = props => {
  const s = useCatalogPageStyles();
  const isLarge = false;
  const c = isLarge ? 'col-6 col-md-6 col-lg-3 mb-4 ' : 'col-6 col-md-6 col-lg-2 mb-2';
  const thumbs = thumbnailStore.useContainer();

  const [image, setImage] = useState(thumbs.getPlaceholder());
  const [itemUrl, setItemUrl] = useState('/develop')
  useEffect(() => {
    setImage(thumbs.getAssetThumbnail(props.id));
    setItemUrl(getLibraryItemUrl({ assetId: props.id, name: props.name }));
  }, [props, thumbs.thumbnails]);

  const [showDetails, setShowDetails] = useState(false);
  const cardRef = useRef(null);
  // various conditionals
  const nameHeight = 36;
  const nameRef = useRef(null);

  return <div className={`${c}`} onMouseEnter={() => setShowDetails(true)} onMouseLeave={() => setShowDetails(false)}>
    <div ref={cardRef} className={isLarge ? s.imageBig : s.imageSmall} >
      <Link href={itemUrl}>
        <a href={itemUrl}>
          <div style={{ zIndex: showDetails ? 10 : 0, position: 'relative' }}>
            <img alt={props.name} src={image} className={`${s.image} ${props.mode === 'large' ? s.imageBig : s.imageSmall}`} onError={e => {
              if (e.currentTarget.src !== thumbs.getPlaceholder()) {
                setImage(thumbs.getPlaceholder());
              }
            }} />
            <div className={s.overviewDetails}>
              <p ref={nameRef} className={`mb-0 ${s.itemName}`} style={{ maxHeight: nameHeight }}>{props.name}</p>
              <p className='mb-0 '>{props.isForSale ? 'Free' : 'Offsale'}</p>
            </div>
          </div>
        </a>
      </Link>
      <div
        style={showDetails ? {
          width: cardRef.current.clientWidth,
          paddingTop: cardRef.current.clientHeight + 'px',
          marginTop: '-' + cardRef.current.clientHeight + 'px',
        } : undefined}
        className={s.detailsWrapper + ' ' + (isLarge ? s.detailsLarge : s.detailsSmall) + ' ' + (showDetails ? s.detailsOpen : '')}>
        
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Creator: </span>
          <span className={s.detailsValue}>
            <CreatorLink id={props.creatorTargetId} name={props.creatorName} type={props.creatorType} />
          </span>
        </p>
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Updated: </span>
          <span className={s.detailsValue}>
            {dayjs(props.updatedAt).fromNow()}
          </span>
        </p>
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Sales: </span>
          <span className={s.detailsValue}>
            {props.saleCount.toLocaleString()}
          </span>
        </p>
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Favorited: </span>
          <span className={s.detailsValue}>
            {props.favoriteCount.toLocaleString()}
          </span>
        </p>
      </div>
    </div>
    <div />
  </div>
}

export default LibraryCard;
