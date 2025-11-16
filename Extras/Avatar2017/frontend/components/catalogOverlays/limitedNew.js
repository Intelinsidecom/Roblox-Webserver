import { createUseStyles } from "react-jss";

const useLimitedOverlayStyles = createUseStyles({
    overlay: {
        aspectRatio: "110 / 16",
        marginTop: -40,
        width: "auto",
        height: 16,
        marginLeft: -0.5
    },
});

const LimitedOverlay = props => {
    const s = useLimitedOverlayStyles();
    return <span className={`icon-labels Limited ${s.overlay}`} style={{ backgroundPosition: "0 calc(-18px * (4 - 1))" }} />
}

const LimitedUniqueOverlay = props => {
    const s = useLimitedOverlayStyles();
    return <span className={`icon-labels LimitedUnique ${s.overlay}`} style={{ backgroundPosition: "0 calc(-18px * (8 - 1))" }} />
}

export {
    LimitedOverlay,
    LimitedUniqueOverlay
};