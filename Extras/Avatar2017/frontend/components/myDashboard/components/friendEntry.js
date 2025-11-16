import dayjs from "dayjs";
import { createUseStyles } from "react-jss";
import { getGameUrl } from "../../../services/games";
import Activity from "../../userActivity";
import DashboardStore from "../stores/dashboardStore";
import Link from "../../link";
import PlayerHeadshot from "../../playerHeadshot";

const useStyles = createUseStyles({
  friendEntry: {
    paddingLeft: '10px',
    paddingRight: '10px',
    maxWidth: '100px',
    overflow: 'hidden',
  },
  thumbnailWrapper: {
    maxWidth: '85px',
    borderRadius: '100%',
    border: '1px solid var(--text-color-quinary)',
    overflow: 'hidden',
    boxShadow: '0 1px 3px rgb(150 150 150 / 74%)',
    margin: '0 auto',
    display: 'block',
    width: '100%',
  },
  username: {
    whiteSpace: 'nowrap',
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    textAlign: 'center',
    marginBottom: 0,
    width: '100%',
    marginTop: '4px',
    fontSize: '15px',
    fontWeight: 500,
    color: '#4a4a4a',
  },
  activityWrapper: {
    float: 'right',
    marginTop: '-26px',
    zIndex: 2,
    position: 'relative',
  },
});
const FriendEntry = props => {
  const store = DashboardStore.useContainer();
  const onlineStatus = store.friendStatus && store.friendStatus[props.id];
  const s = useStyles();
  return <div className={s.friendEntry}>
    <Link href={`/users/${props.id}/profile`}>
      <a>
        <div className={s.thumbnailWrapper}>
          <PlayerHeadshot id={props.id} name={props.name}/>
        </div>
        {onlineStatus && <div className={s.activityWrapper}><Activity {...onlineStatus}/></div>}
        <p className={s.username}>{props.name}</p>
      </a>
    </Link>
  </div>
}

export default FriendEntry;