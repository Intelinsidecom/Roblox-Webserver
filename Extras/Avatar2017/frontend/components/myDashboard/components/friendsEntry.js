import dayjs from "dayjs";
import { createUseStyles } from "react-jss";
import { getGameUrl } from "../../../services/games";
import Activity from "../../userActivity";
import DashboardStore from "../stores/dashboardStore";
import Link from "../../link";
import PlayerHeadshot from "../../playerHeadshot";

const useStyles = createUseStyles({
    friendEntry: {
        padding: 0,
        maxWidth: '100px',
        overflow: 'hidden',
        listStyle: 'none',
        width: '11.11111%',
        minWidth: '95px',
        '@media (max-width: 991px)': {
            width: '14.28571%'
        },
        '@media (max-width: 767px)': {
            width: '20%'
        },
        '@media (max-width: 543px)': {
            width: '33.33333%'
        },
    },
    friendWrapper: {
        margin: '3px auto',
    },
    thumbnailWrapper: {
        maxWidth: '90px',
        borderRadius: '100%',
        //border: '1px solid var(--text-color-quinary)',
        border: 'none',
        //overflow: 'hidden',
        boxShadow: '0 1px 4px 0 rgba(25,25,25,0.3)',
        margin: '0 auto',
        display: 'block',
        width: '100%',
        position: 'relative',
        '&:hover': {
            transition: 'box-shadow 200ms ease',
            boxShadow: '0 1px 6px 0 rgba(25,25,25,0.75)'
        }
    },
    username: {
        whiteSpace: 'nowrap',
        textOverflow: 'ellipsis',
        overflow: 'hidden',
        textAlign: 'center',
        marginBottom: 0,
        width: '100%',
        marginTop: '3px',
        fontSize: '15px',
        fontWeight: 300,
        color: 'var(--text-color-primary)',
        '&:hover': {
            textDecoration: 'none!important',
            color: 'var(--primary-color)'
        },
    },
    activityWrapper: {
        float: 'right',
        //marginTop: '-26px',
        zIndex: 2,
        position: 'absolute',
        right: 0,
        bottom: 0,
    },
    img: {
        borderRadius: '100%',
    },
});
const FriendEntry = props => {
    const store = DashboardStore.useContainer();
    const onlineStatus = store.friendStatus && store.friendStatus[props.id];
    const s = useStyles();
    return <li className={s.friendEntry}>
        <div className={s.friendWrapper}>
            <span>
                <Link href={`/users/${props.id}/profile`}>
                    <a>
                        <div className={s.thumbnailWrapper}>
                            <PlayerHeadshot className={s.img} id={props.id} name={props.name} />
                            {onlineStatus && <div className={s.activityWrapper}><Activity {...onlineStatus} /></div>}
                        </div>
                        <p className={s.username}>{props.name}</p>
                    </a>
                </Link>
            </span>
        </div>
    </li>
}

export default FriendEntry;