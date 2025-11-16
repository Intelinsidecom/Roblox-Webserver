import dayjs from "dayjs";
import { createUseStyles } from "react-jss";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";
import Subtitle from "./subtitle";
import { useEffect, useState } from "react";

const useStatisticEntryStyles = createUseStyles({
  label: {
    fontWeight: 400,
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    color: 'var(--text-color-secondary)',
    marginBottom: 0,
    textAlign: 'center',
    fontSize: '16px',
  },
  value: {
    marginTop: '5px',
    textAlign: 'center',
    fontSize: '18px',
    marginBottom: '0!important',
  },
});

const StatisticEntry = props => {
  const s = useStatisticEntryStyles();
  return <div className='col-4'>
    <p className={s.label}>{props.label}</p>
    <p className={s.value}>{props.value}</p>
  </div>
}

const useStatisticStyles = createUseStyles({
  padder: {
    padding: '15px!important'
  }
})

const Statistics = props => {
  const s = useStatisticStyles()
  const store = UserProfileStore.useContainer();
  const cardStyles = useCardStyles();
  const [playerVisits, setPlayerVisits] = useState(null);
  useEffect(() => {

    if (store.createdGames == null)
      return

    var totalVisits = 0
    for (const game of store.createdGames) {
      totalVisits += game.placeVisits
    }
    setPlayerVisits(totalVisits)
  }, [store.createdGames])
  return <div className='flex'>
    <div className='col-12'>
      <Subtitle>Statistics</Subtitle>
    </div>
    <div className='col-12'>
      <div className={cardStyles.card}>
        <div className={`flex ${s.padder}`}>
          <StatisticEntry label='Join Date' value={dayjs(store.userInfo.created).format('M/DD/YYYY')}></StatisticEntry>
          <StatisticEntry label='Place Visits' value={playerVisits || (0).toLocaleString()}></StatisticEntry>
          <StatisticEntry label='Forum Posts' value={(0).toLocaleString()}></StatisticEntry>
        </div>
      </div>
    </div>
  </div>
}

export default Statistics;