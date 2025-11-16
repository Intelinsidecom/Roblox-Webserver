import React, {useEffect, useRef, useState} from "react";
import {createUseStyles} from "react-jss";
import {uploadAsset} from "../../../../services/develop";
import AuthenticationStore from "../../../../stores/authentication";
import ActionButton from "../../../actionButton";
import AssetList from "../assetList";
import FeedbackStore from "../../../../stores/feedback";
import {getGameUrl, getUniversebadgees, getUserGames} from "../../../../services/games";
import {useRouter} from "next/router";
import Link from "../../../link";
import buttonStyles from "../../../../styles/buttonStyles";
import useButtonStyles from "../../../../styles/buttonStyles";
import {getUniverseBadges} from "../../../../services/badges";

const useStyles = createUseStyles({
    subtext: {
        color: '#d2d2d2',
        fontSize: '14px',
        marginLeft: '8px',
    },
    inputItemName: {
        width: 'calc(100% - 200px)',
        flexGrow: 1,
        //marginLeft: '28px',
    },
    inputItemDesc: {
        width: 'calc(100% - 75px)',
        marginLeft: 0,
        flexGrow: 1,
    },
    gameSelectContainer: {
        marginTop: 30,
        display: 'flex',
        justifyContent: 'space-between',
        '& h2': {
            display: 'inline-block',
        }
    },
    wrap: {
        display: 'inline-block',
        margin: 'auto 0',
    },
    selectFrom: {},
    gameSelector: {
        marginLeft: 5
    },
    stuffContainer: {
        '& p': {
            marginBottom: '0.5em',
            display: 'flex',
            alignItems: 'center',
            gap: 5,
        }
    },
    badgeImageContainer: {
        height: '100px',
        width: '100px',
        '& img': {
            width: '100%',
            height: '100%',
            display: 'inline-block',
            borderRadius: 8
        }
    },
    badgeContentContainer: {
        marginLeft: 10,
        display: 'flex',
        gap: 10,
        width: 'calc(100% - 110px)',
    },
    fieldContainer: {
        display: 'flex',
        flexDirection: 'column',
        minWidth: '25%',
        '& span': {
            fontWeight: 500,
            marginTop: 3
        }
    },
    valueContainer: {
        display: 'flex',
        flexDirection: 'column',
        flexGrow: 1,
         maxWidth: 'calc(80% - 10px)',
        //overflowY: 'scroll',
        '& span': {
            whiteSpace: 'wrap',
            wordWrap: 'break-word',
            overflowWrap: 'break-word',
            marginTop: 3
        }
    },
    buttonStyle: {
        padding: '5px 13px',
    },
    loadingIcon: {
        background: 'url(/img/loading_old.gif)',
        backgroundSize: '48px 17px',
        width: '48px',
        height: '17px',
    },
})

const Badges = props => {
    const {id, groupId} = props;
    
    const auth = AuthenticationStore.useContainer();
    const router = useRouter();
    
    const feedback = FeedbackStore.useContainer();
    const [locked, setLocked] = useState(false);
    const [previewing, setPreviewing] = useState(false);
    // 0 == loading, 1 == failed, array = success
    const [gamesList, setGamesList] = useState(0);
    const [selectedGame, setSelectedGame] = useState(null); // should be a ref to an entry in the games list
    // 0 == loading, 1 == failed, array = success
    const [badgesList, setBadgesList] = useState(0);
    const nameRef = useRef(null);
    const [name, setName] = useState(null);
    const [desc, setDesc] = useState('');
    /**
     * @type {React.Ref<HTMLInputElement>}
     */
    const fileRef = useRef(null);
    const [file, setFile] = useState(null);
    const [fileBlob, setFileBlob] = useState(null);
    
    const deleteQuery = (str) => {
        const updatedQuery = router.query;
        delete updatedQuery[str];
        
        router.push({
            pathname: router.pathname,
            query: updatedQuery,
        }, undefined, {shallow: true});
    }
    
    const updateQuery = (field, value) => {
        const updatedQuery = router.query;
        updatedQuery[field] = value;
        
        router.push({
            pathname: router.pathname,
            query: updatedQuery,
        }, undefined, {shallow: true});
    }
    
    const onSubmit = e => {
        e.preventDefault();
        if (locked) return;
        
        setLocked(true);
        uploadAsset({
            name,
            assetTypeId: id,
            file,
            groupId,
            description: desc,
            universeId: selectedGame.id
        }).then(() => {
            window.location.reload();
        }).catch(e => {
            feedback.addFeedback(e.message);
            setLocked(false);
        })
    }
    
    const onCancel = e => {
        e.preventDefault();
        if (locked) return;
        setLocked(true);
        
        nameRef.current = { value: "" };
        setDesc('');
        fileRef.current = null;
        
        setPreviewing(false);
        setLocked(false);
    }
    
    const changePreviewing = () => {
        if (locked) return;
        if (!fileRef.current?.files?.length) return feedback.addFeedback('You must select a file');
        if (!nameRef.current?.value || nameRef.current.value.length < 3) return feedback.addFeedback('You must specify a name');
        //if (!desc) return feedback.addFeedback('You must specify a description');
        if (!desc) setDesc('');
        let image = fileRef.current.files[0];
        if (image.size >= 8e+7) return feedback.addFeedback('The file is too large');
        if (image.size === 0) return feedback.addFeedback('The file is empty');
        setName(nameRef.current.value);
        setFile(image);
        const reader = new FileReader();
        reader.onload = (e) => setFileBlob(e.target.result);
        reader.readAsDataURL(image);
        setPreviewing(true);
    }
    
    useEffect(() => {
        if (router.query.universeId !== undefined && Array.isArray(gamesList) && gamesList[router.query.universeId] !== null) {
            const selected = gamesList.find(g => g.id === parseInt(router.query.universeId));
            setSelectedGame(selected);
        } else if (router.query.universeId !== undefined && Array.isArray(gamesList) && gamesList.length > 0) {
            deleteQuery("universeId")
        }
    }, [router.query, gamesList]);
    
    useEffect(() => {
        //setGamesList(null); // might cause issues with rerendering
        if (!auth.userId || !id) return;
        
        try {
            setGamesList(0);
            setSelectedGame(null);
            getUserGames({userId: auth.userId}).then(gamesL => {
                const gameArray = gamesL?.data;
                if (!gameArray || !gameArray.length) {
                    setGamesList(1);
                    return;
                }
                
                const universeId = parseInt(router.query.universeId);
                const queriedGame = gameArray.find(g => g.id === universeId);
                if (gameArray.length === 0) {
                    setSelectedGame(null);
                } else if (!universeId || !queriedGame) {
                    updateQuery("universeId", gameArray[0].id);
                    setSelectedGame(gameArray[0]);
                } else {
                    setSelectedGame(queriedGame);
                }
                setGamesList(gameArray);
            });
        } catch (e) {
            feedback.addFeedback(e);
            setGamesList(1);
        }
        
    }, [auth.userId, id]);
    
    useEffect(() => {
        if (!selectedGame?.id) return;
        try {
            // TODO: make this support 500 badges, rn it's only 25
            getUniverseBadges({
                limit: 25,
                // cursor: '',
                universeId: selectedGame.id,
                //groupId,
            }).then(badgeCollection => {
                setBadgesList({
                    nextPageCursor: badgeCollection.nextPageCursor,
                    previousPageCursor: badgeCollection.previousPageCursor,
                    data: badgeCollection.data.map(d => ({ ...d, assetId: d.id, assetType: 21 }))
                });
            });
        } catch (e) {
            feedback.addFeedback(e);
            setBadgesList(1);
        }
    }, [selectedGame])
    
    const s = useStyles();
    const buttonStyles = useButtonStyles();
    
    return <div className='row'>
        <div className='col-12'>
            <h2>
                Create a Badge
            </h2>
        </div>
        <div className='col-12'>
            {previewing ?
                <div className='ms-4 me-4 mt-4 flex flex-column'>
                    <div className='flex w-100'>
                        <div className={s.badgeImageContainer}>
                            <img src={fileBlob ? fileBlob : '/img/placeholder.png'}/>
                        </div>
                        <div className={s.badgeContentContainer}>
                            <div className={s.fieldContainer}>
                                <span>Name:</span>
                                <span>Target Game:</span>
                                <span>Description:</span>
                            </div>
                            <div className={s.valueContainer}>
                                <span>{nameRef?.current?.value || name}</span>
                                <span><Link
                                    href={getGameUrl({placeId: selectedGame.rootPlaceId, name: selectedGame.name})}><a
                                    className='link2018'>{selectedGame.name}</a></Link></span>
                                <span>{desc}</span>
                            </div>
                        </div>
                    </div>
                    <div className='float-left flex mt-5' style={{ gap: 5 }}>
                        { locked ?
                            <span className={s.loadingIcon} />
                            :
                            <>
                                <ActionButton className={s.buttonStyle} disabled={locked} label='Verify Upload' onClick={e => onSubmit(e)}/>
                                <ActionButton className={s.buttonStyle} buttonStyle={buttonStyles.cancelButton} disabled={locked} label='Cancel' onClick={e => onCancel(e)}/>
                            </>
                        }
                    </div>
                </div>
                : <div className={`ms-4 me-4 mt-4 ${s.stuffContainer}`}>
                    {//details.templateUrl ? <p>Did you use the template? If not, <a href={details.templateUrl}>download it here</a>.</p> : null}
                    }
                    <p>Target Game: {selectedGame && selectedGame?.id &&
                        <Link href={getGameUrl({placeId: selectedGame.rootPlaceId, name: selectedGame.name})}><a
                            className='link2018'>{selectedGame.name}</a></Link>}</p>
                    <p>Find your image: <input ref={fileRef} type='file' accept='image/*'/> {
                        //{feedback && <span className='text-danger'>{feedback}</span>}
                    }</p>
                    <p>Badge Name: <input ref={nameRef} type='text' className={s.inputItemName}/></p>
                    {/*<p>Description: <input ref={descRef} type='text' className={s.inputItemDesc}/></p>*/}
                <div style={{display: "flex", alignItems: "top", width: '100%', marginBottom: '1em', flexDirection: "column"}}>
                    <label style={{marginRight: "8px"}}>Description:</label>
                    <textarea
                        value={desc}
                        onChange={e => setDesc(e.target.value)}
                        rows={2}
                        className={s.inputItemDesc}
                    />
                </div>
                <div className='float-left'>
                    <ActionButton disabled={locked} label='Preview' onClick={() => changePreviewing()}/>
                </div>
            </div>}
        </div>
        <div className={`${s.gameSelectContainer} col-12`}>
            <div>
                <h2>
                    Badges
                </h2>
            </div>
            {
                Array.isArray(gamesList) && gamesList.length > 0 && <div className={s.wrap}>
                    <span className={`${s.selectFrom}`}>Select from Public Games:</span>
                    <select className={`${s.gameSelector}`} onChange={e => {
                        const selected = gamesList.find(g => g.id === parseInt(e.target.value));
                        updateQuery("universeId", selected.id);
                        setSelectedGame(selected);
                    }}>
                        {
                            gamesList.map((game) /** @type {UserGameEntry} */ =>
                                <option selected={game.id === selectedGame.id} value={game.id}
                                        key={game.id}>{game.name}</option>
                            )
                        }
                    </select>
                </div>
            }
        </div>
        <div className='col-12 mt-4'>
            {Array.isArray(badgesList.data) ?
                (badgesList.data.length === 0 ? <p>No Badges found.</p> : <AssetList assets={badgesList.data}/>)
                : null
            }
        </div>
    </div>
}

export default Badges;