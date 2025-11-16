import {createUseStyles} from "react-jss";
import { useEffect, useState } from "react";
import { getAudioURL } from "../../../services/catalog";
import { useRouter } from "next/router";

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
// TODO: fix and use lat r
/**
 * @param {number} audioId
 * @param {boolean} [small]
 * @returns {JSX.Element}
 * @constructor
 */
function AudioPlayButton({ audioId, small = false }) {
    const s = useStyles();
    const [audioElm, setAudioElm] = useState(/** @type {HTMLAudioElement|null} */(null));
    const [isPlaying, setPlaying] = useState(false);
    const router = useRouter();
    
    useEffect(async () => {
        let audioUrl = await getAudioURL({ audioId });
        let audio = new Audio(audioUrl);
        document.body.appendChild(audio);
        setAudioElm(audio);
        
        return () => setAudioElm(null);
    }, [audioId]);
    
    useEffect(() => {
        if (!audioElm) return;
        console.log(isPlaying);
        if (isPlaying) {
            const playAudio = () => {
                console.log("PLAYYY");
                audioElm.play();
            };
            console.log("p;ayed");
            const setPlayingFalse = () => {
                console.log("NO PLAY");
                setPlaying(false);
            };
            audioElm.addEventListener("canplaythrough", playAudio);
            audioElm.addEventListener("ended", setPlayingFalse);
            audioElm.addEventListener("canplay", () => console.log("Can play now"));
            audioElm.addEventListener("loadeddata", () => console.log("Data loaded"));
            audioElm.addEventListener("canplaythrough", () => console.log("Can play through without buffering"));
            
            return () => {
                audioElm.removeEventListener("canplaythrough", playAudio);
                audioElm.removeEventListener("ended", setPlayingFalse);
            };
        } else {
            console.log("paused");
            audioElm.pause();
            audioElm.currentTime = 0;
        }
    }, [isPlaying, audioElm]);
    
    useEffect(() => {
        const handleRouteChange = () => {
            if (window.location.href !== "/develop?View=3" || window.location.href !== "/develop?View=34") {
                setPlaying(false);
            }
        }
        router.events.on("routeChangeStart", handleRouteChange);
        return () => {
            router.events.off("routeChangeStart", handleRouteChange);
        };
    }, []);
    
    if (!audioElm) return null;
    
    return <div className={s.wrapper}>
        <span
            className={`${isPlaying ? small ? 'icon-pause' : 'icon-pause-big' : small ? 'icon-play' : 'icon-play-big'} ${small ? s.smallImg : s.img} ${small ? s.smallPlayButton : s.audioPlayButton}`}
            onClick={e => {
                e.preventDefault();
                if (!audioElm) return;
                setPlaying(!isPlaying);
            }}>
        </span>
    </div>
}

export default AudioPlayButton;
