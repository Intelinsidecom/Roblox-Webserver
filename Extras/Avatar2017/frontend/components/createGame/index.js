import React, {useEffect, useRef, useState} from "react";
import { createUseStyles } from "react-jss";
import OldVerticalTabs from "../oldVerticalTabs2";
import AuthenticationStore from "../../stores/authentication";
import Templates from "./subpages/templates";
import BasicSettings from "./subpages/basicSettings";
import Access from "./subpages/access";
import AdvancedSettings from "./subpages/advancedSettings";
import ActionButton from "../actionButton";
import useButtonStyles from "../../styles/buttonStyles";
import FeedbackStore from "../../stores/feedback";
import {FeedbackType} from "../../models/feedback";
import {createGameRequest, getUserCreatedPlaceCount} from "../../services/games";
import {setUniverseMaxPlayers, setUniverseYear, updateAsset} from "../../services/develop";

const useStyles = createUseStyles({
    contentContainer: {
        padding: '15px',
    },
    creationContainer: {
        display: 'flex',
        flexDirection: 'column',
    },
    actionContainer: {
        display: 'flex',
        flexDirection: 'row',
        gap: '10px',
        marginTop: 20,
    },
    properPadding: {
        padding: '4px 8px'
    },
    WillTheRealSlimShadyPleaseStandUp: {
        '& *': {
            fontFamily: 'Source Sans Pro, Arial'
        }
    },
    
    loadingModal: {
        position: 'absolute',
        top: 40,
        left: 0,
        width: '100vw',
        height: '100vh',
        backgroundColor: 'rgba(0,0,0,0.7)',
        zIndex: 1000
    },
    loadingContainer: {
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        flexDirection: 'column',
        gap: 12,
        margin: "auto",
        width: "100%",
        height: "100%",
    },
    loadingIcon: {
        background: 'url(/img/loading_old.gif)',
        backgroundSize: '48px 17px',
        width: '48px',
        height: '17px',
    },
})

const CreateGame = props => {
    const auth = AuthenticationStore.useContainer();
    if (!auth.userId || !auth.username) return null;
    const s = useStyles();
    const feedback = FeedbackStore.useContainer();
    const but = useButtonStyles();
    const [tab, setTab] = useState('Templates');
    const [locked, setLocked] = useState(false);
    
    const [selectedTemplate, setSelectedTemplate] = useState(36568);
    const [gameName, setGameName] = useState(`${auth.username}'s Place Number: 0`);
    const [gameDescription, setGameDescription] = useState('');
    const [commentsEnabled, setCommentsEnabled] = useState('true');
    const [gameGenre, setGameGenre] = useState('All');
    
    useEffect(async () => {
        setGameName(`${auth.username}'s Place Number: ${await getUserCreatedPlaceCount({userId: auth.userId}) + 1}`);
    }, []);
    
    const [playableDevices, setPlayableDevices] = useState({
        computer: true,
        phone: true,
        tablet: true,
        console: false
    });
    const [playerCount, setPlayerCount] = useState(10);
    const [gameYear, setGameYear] = useState(2017);
    const [access, setAccess] = useState('Everyone');
    
    //const [gearGenres, setGearGenres] = useState(false);
    const [uncopylocked, setUncopylocked] = useState(false);
    
    const options = [
        {
            name: 'Templates',
            displayName: 'Templates',
            element: <Templates template={selectedTemplate} setTemplate={setSelectedTemplate} />,
        },
        {
            name: 'BasicSettings',
            displayName: 'Basic Settings',
            element: <BasicSettings
                gameName={gameName}
                setGameName={setGameName}
                gameDescription={gameDescription}
                setGameDescription={setGameDescription}
                commentsEnabled={commentsEnabled}
                setCommentsEnabled={setCommentsEnabled}
                gameGenre={gameGenre}
                setGameGenre={setGameGenre} />,
        },
        {
            name: 'Access',
            displayName: 'Access',
            element: <Access
                playableDevices={playableDevices}
                setPlayableDevices={setPlayableDevices}
                playerCount={playerCount}
                setPlayerCount={setPlayerCount}
                access={access}
                setAccess={setAccess}
                gameYear={gameYear}
                setGameYear={setGameYear}
            />,
        },
        {
            name: 'Advanced Settings',
            displayName: 'Advanced Settings',
            element: <AdvancedSettings uncopylocked={uncopylocked} setUncopylocked={setUncopylocked} />,
        }
    ]
    
    const createGame = async () => {
        if (locked) return;
        setLocked(true);
        
        if (gameName.length < 3 || gameName.length > 100) {
            feedback.addFeedback("Game name is too long or too short. You figure it out, loser.", FeedbackType.ERROR)
            setLocked(false);
            return;
        }
        if (gameDescription && gameDescription.length > 1000) {
            feedback.addFeedback("Game description is too long or too short.", FeedbackType.ERROR)
            setLocked(false);
            return;
        }
        
        let placeId = await createGameRequest({ templatePlaceId: selectedTemplate });
        if (typeof placeId === 'string') {
            feedback.addFeedback("An error occurred: " + placeId, FeedbackType.ERROR)
            setLocked(false);
            return;
        }
        if (!placeId) {
            feedback.addFeedback("Failed to create game due to an unexpected error.", FeedbackType.ERROR)
            setLocked(false);
            return;
        }
        await updateAsset({
            assetId: placeId.placeId,
            name: gameName,
            description: gameDescription,
            genres: [gameGenre],
            isCopyingAllowed: uncopylocked,
            enableComments: commentsEnabled === 'true' || commentsEnabled === true,
            verbose: true
        });
        await setUniverseMaxPlayers({ universeId: placeId.universeId, maxPlayers: playerCount, verbose: true });
        await setUniverseYear({ universeId: placeId.universeId, year: gameYear, verbose: true });
        
        window.location.href = `/games/${placeId.placeId}/--`
        // const gameStuff = {
        //     template: selectedTemplate,
        //     name: gameName,
        //     description: gameDescription,
        //     genre: gameGenre,
        //     comments: commentsEnabled === 'true',
        //     playableDevices,
        //     maxPlayerCount: playerCount,
        //     placeAccess: access,
        //     uncopylocked
        // };
        // console.log(gameStuff);
        // feedback.addFeedback("Game successfully created!", FeedbackType.SUCCESS);
        // setTimeout(() => window.location.href = `/games/${placeId}/--`, 5000);
    }
    
    return <div className={`container ssp ${s.WillTheRealSlimShadyPleaseStandUp}`}>
        {
            locked && <div className={s.loadingModal}>
                <div className={s.loadingContainer}>
                    <span className={s.loadingIcon}></span>
                    <span style={{ color: "#fff" }}>Creating game...</span>
                </div>
            </div>
        }
        <h1 style={{fontWeight: 600, marginBottom: '10px'}}>Create Game</h1>
        <div>
            <OldVerticalTabs contentStyles={`${s.contentContainer} vTabContent`} options={options} default={tab} onChange={n => setTab(n.name)}/>
        </div>
        <div className={s.actionContainer}>
            <ActionButton className={s.properPadding} buttonStyle={but.buyButton} label='Create Game' onClick={async e => {
                e.preventDefault();
                await createGame();
            }} />
            <ActionButton className={s.properPadding} buttonStyle={but.cancelButton} label='Cancel' onClick={e => {
                e.preventDefault();
                window.location.href = '/develop?View=0';
            }} />
        </div>
    </div>
}

export default CreateGame;