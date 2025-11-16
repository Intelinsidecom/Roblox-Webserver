import { createUseStyles } from "react-jss"
import { useEffect, useState } from "react";
import { getAudioURL } from "../../../services/catalog";
import Router from "next/router";

const useStyles = createUseStyles({
    wrapper: {
        //overflowX: 'hidden',
        position: 'relative',
        width: '100%',
        marginTop: 'auto',
    },
    smallImg: {
        margin: '3px',
        bottom: 0,
        right: 0,
        position: 'absolute',
    },
    img: {
        bottom: 0,
        right: 0,
        margin: '6px',
        position: 'absolute',
        //marginRight: '0',
    },
    audioPlayButton: {
        width: '48px',
        height: '48px',
        backgroundSize: '96px auto',
        cursor: 'pointer',
    },
    smallPlayButton: {
        width: '28px',
        height: '28px',
        backgroundSize: '56px auto',
        cursor: 'pointer'
    },
});

/**
 *
 * @param {{audioId: number; small?: boolean;}} props
 * @returns
 */

const PlayButton = props => {
    const s = useStyles();
    const [audio, setAudio] = useState(null);
    const [playing, setPlaying] = useState(false);
    const [audioUrl, setAudioUrl] = useState(null);
    const small = props?.small;

    useEffect(() => {
        getAudioURL({ audioId: props.audioId }).then(audioUrl => {
            setAudioUrl(audioUrl);
        });
    })

    useEffect(() => {
        if (playing && audio) {
            const playAudio = () => audio.play();
            const setPlayingFalse = () => setPlaying(false);
            audio.addEventListener('canplaythrough', playAudio);
            audio.addEventListener('ended', setPlayingFalse);
            return () => {
                audio.removeEventListener('canplaythrough', playAudio);
                audio.removeEventListener('ended', setPlayingFalse);
            };
        } else if (audio) {
            audio.pause();
            audio.currentTime = 0;
            setAudio(null);
        }
    }, [playing, audio]);

    useEffect(() => {
        const handleLocationChange = () => {
            if (window.location.href != '/develop?View=3' ||  '/develop?View=34') {
                setPlaying(false);
            }
        };
        Router.events.on('routeChangeStart', handleLocationChange);
        return () => {
            Router.events.off('routeChangeStart', handleLocationChange);
        };
    }, [Router]);

    const handlePlayPause = (e) => {
        e.preventDefault();
        if (!playing && typeof (audioUrl) == 'string') {
            const newAudio = new Audio(audioUrl);
            setAudio(newAudio);
            setPlaying(true);
        } else {
            setPlaying(false);
        }
    };

    return <div className={`${s.wrapper}`}>
        <span className={`${playing ? small ? 'icon-pause' : 'icon-pause-big' : small ? 'icon-play' : 'icon-play-big'} ${small ? s.smallImg : s.img} ${small ? s.smallPlayButton : s.audioPlayButton}`}
            onClick={handlePlayPause}>
        </span>
    </div>
}

export default PlayButton;