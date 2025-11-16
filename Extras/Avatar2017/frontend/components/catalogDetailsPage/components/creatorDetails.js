import dayjs from "../../../lib/dayjs";
import React from "react";
import {createUseStyles} from "react-jss";
import BcOverlay from "../../bcOverlay";
import CreatorLink from "../../creatorLink";
import PlayerImage from "../../playerImage";
import GroupIcon from "../../groupIcon";
import {getGameUrl} from "../../../services/games";
import Link from "../../link";

const useStatEntryStyles = createUseStyles({
    text: {
        fontSize: '12px',
        paddingBottom: 0,
        marginBottom: 0,
    },
    statName: {
        color: '#999',
    }
});

const StatEntry = props => {
    const s = useStatEntryStyles();
    return <p className={s.text}>
        <span className={s.statName}>{props.name}: </span><span className={props.className}>{props.value}</span>
    </p>
}

const useStyles = createUseStyles({
    ownedStat: {
        verticalAlign: 'text-bottom',
        display: 'inline-block',
        fontSize: '10px',
        backgroundColor: '#02b757',
        color: '#fff',
        padding: '3px',
        borderRadius: '50%',
        lineHeight: 1
    },
    ownedParent: {
        display: 'inline-flex',
    },
});


const CreatorDetails = props => {
    const s = useStyles();
    
    return <div className='row'>
        <div className='col-4 pe-0'>
            {
                props.type === 'User' ? <PlayerImage id={props.id} name={props.name}/> :
                    <GroupIcon id={props.id} name={props.name}/>
            }
            <BcOverlay id={props.id}/>
        </div>
        <div className='col-8 ps-0'>
            <StatEntry name="Creator" value={
                <CreatorLink id={props.id} name={props.name} type={props.type}/>
            }/>
            <StatEntry name="Created" value={dayjs(props.createdAt).format('M/D/YYYY')}/>
            <StatEntry name="Updated" value={dayjs(props.updatedAt).format('M/D/YYYY')}/>
            {props.owned &&
                <StatEntry className={s.ownedParent} name="Owned" value={props.owned ?
                    <div className={s.ownedStat}>
                        <span className='icon-checkmark-white-hold'/>
                    </div>
                    : null}
                />
            }
            {props.gamePassPlace &&
                <StatEntry name="Game"
                           value={
                               <Link href={getGameUrl({
                                   placeId: props.gamePassPlace.rootPlaceId,
                                   name: props.gamePassPlace.name
                               })}>
                                   <a style={{marginLeft: 0}} className='link2018'
                                      href={getGameUrl({
                                          placeId: props.gamePassPlace.rootPlaceId,
                                          name: props.gamePassPlace.name
                                      })}>
                                       {props.gamePassPlace.name}
                                   </a>
                               </Link>
                           }/>
            }
        </div>
    </div>
}

export default CreatorDetails;
