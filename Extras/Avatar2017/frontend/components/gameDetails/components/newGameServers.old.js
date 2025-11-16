import { createUseStyles } from "react-jss";
import { getServers } from "../../../services/games";
import { launchGameFromJobId } from "../../../services/games";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import CreatorLink from "../../creatorLink";
import PlayerImage from "../../playerImage";
import GameDetailsStore from "../stores/gameDetailsStore";

const ServerEntry = props => {
    const store = GameDetailsStore.useContainer();
    const buttonStyles = useButtonStyles();

}

const useStyles = createUseStyles({
    buttonWrapper: {
        margin: '0 auto',
        width: '100%',
        maxWidth: '200px',
    },
    tabPane: {
        backgroundColor: 'var(--white-color)',
        padding: '15px',
        display: 'flex',
        flexDirection: 'column',
    },
    gamePassContainer: {
        overflow: 'hidden',
        margin: '0 0 6px',
        padding: '0'
    },
    containerHeader: {
        fontSize: '16px',
        fontWeight: '400',
        lineHeight: '1.4em',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'space-between'
    },
    containerHeaderText: {
        fontSize: '20px',
        fontWeight: '700',
        float: 'left',
        margin: 0,
        lineHeight: '1.4em',
        paddingBottom: '5px'
    },
    refreshButton: {
        border: 0,
        backgroundColor: 'transparent',
        width: 'auto',
        textAlign: 'center',
        fontSize: '16px',
        fontWeight: '500',
        position: 'relative',
        float: 'right',
        margin: 0,
        display: 'inline-block',
        height: 'auto',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        padding: '4px',
        lineHeight: '100%',
        borderRadius: '3px',
    },
    textLink: {
        userSelect: 'none',
        cursor: 'pointer',
        textDecoration: 'none!important',
        color: 'var(--primary-color)',
        '&:hover': {
            textDecoration: 'underline!important',
            color: 'var(--primary-color)',
        },
    },
})

const GameServers = props => {
    const store = GameDetailsStore.useContainer();
    const s = useStyles();
    const showButton = !store.servers || store.servers && store.servers.areMoreAvailable && !store.servers.loading;


    return <div className={' ' + s.tabPane}>
        <div className={s.gamePassContainer}>
            <div className={s.containerHeader}>
                <h3 className={s.containerHeaderText}>Other Servers</h3>
                <span className={s.refreshButton + ' ' + s.textLink}>Refresh</span>
            </div>
            s
        </div>
    </div>
};

export default GameServers;