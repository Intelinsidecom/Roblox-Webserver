import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getBaseUrl } from "../../lib/request";
import { reportImageFail } from "../../services/metrics";
import thumbnailStore from "../../stores/thumbnailStore";

const useStyles = createUseStyles({
    image: {
        maxWidth: '400px',
        width: '100%',
        margin: '0 auto',
        height: 'auto',
        display: 'block',
    },
})

const PlayerImage = (props) => {
    const s = useStyles();
    const size = props.size || 420;
    const [retryCount, setRetryCount] = useState(0);
    const thumbs = thumbnailStore.useContainer();
    const [image, setImage] = useState(props.url ? props.url : thumbs.getUserThumbnail(props.id, '420x420'));
    
    useEffect(() => {
        if (props.url) {
            setImage(props.url)
            return
        }
        setRetryCount(0);
        setImage(thumbs.getUserThumbnail(props.id, `${size}x${size}`));
    }, [props]);
    
    useEffect(() => {
        if (props.url) {
            return
        }
        setImage(thumbs.getUserThumbnail(props.id, `${size}x${size}`));
    }, [thumbs.thumbnails]);
    
    return <img
        className={`${s.image} ${props.className}`}
        src={image}
        alt={props.name}
        style={props.invisible ? {display: "none"} : {}}
        onError={
            e => {
                if (retryCount >= 3) return;
                reportImageFail({
                    errorEvent: e,
                    type: 'playerHeadshot',
                    src: image,
                })
                setRetryCount(retryCount + 1);
                setImage('/img/placeholder.png')
            }}
    />
}

export default PlayerImage;