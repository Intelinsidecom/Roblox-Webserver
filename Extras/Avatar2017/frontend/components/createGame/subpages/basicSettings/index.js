import { createUseStyles } from "react-jss";
import {GameGenres} from "../../../updatePlace/components/basicSettings";
import {useEffect, useState} from "react";
import {getUserCreatedPlaceCount} from "../../../../services/games";
import Authentication from "../../../../stores/authentication";
import AuthenticationStore from "../../../../stores/authentication";

const useStyles = createUseStyles({
    contain: {
        margin: 0,
        maxWidth: '45%!important',
    },
    settingsContainer: {
        display: "flex",
        flexDirection: "column",
        '& span': {
            marginBottom: 3
        },
        '& textarea': {
            resize: "none",
            height: 125
        }
    },
    input: {
        marginBottom: 10
    },
})

const BasicSettings = props => {
    const s = useStyles();
    
    return <div className={`container noPadding ${s.contain}`}>
        <h2 style={{ marginBottom: 15 }}>Basic Settings</h2>
        <div className={s.settingsContainer}>
            <span>Name:</span>
            <input className={s.input} value={props.gameName} onChange={e => props.setGameName(e.target.value)} type='text' />
            <span>Description:</span>
            <textarea className={s.input} value={props.gameDescription} onChange={e => props.setGameDescription(e.target.value)} />
            <span>Comments Enabled:</span>
            <select className={s.input} value={props.commentsEnabled} onChange={e => props.setCommentsEnabled(e.currentTarget.value)}>
                <option value='true'>Yes</option>
                <option value='false'>No</option>
            </select>
            <span>Genre:</span>
            <select className={s.input} value={props.gameGenre} onChange={(e) => props.setGameGenre(e.currentTarget.value)}>
                {
                    GameGenres.map(v => {
                        return <option key={v.value} value={v.value}>{v.name}</option>
                    })
                }
            </select>
        </div>
    </div>
}

export default BasicSettings;