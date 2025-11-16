import dayjs from "dayjs";
import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  container: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
  },
  createdLabel: {
    color: '#999',
  },
  stats: {
    display: 'flex',
    marginRight: '20%',
    flexDirection: 'column',
  },
})

/**\
 *
 * @param {BadgeEntry} badge
 * @returns {JSX.Element}
 * @constructor
 */
const AssetListBadgeEntry = ({ badge }) => {
  const s = useStyles();
  return <div className={s.container}>
    <p className='mb-0'><span className={s.createdLabel}>Created: </span> {dayjs(badge.created).format('M/d/YYYY')}</p>
    <div className={s.stats}>
      <p className='mb-0'><span
          className={s.createdLabel}>Won Yesterday: </span> {badge.statistics.pastDayAwardedCount}</p>
      <p className='mb-0'><span
          className={s.createdLabel}>Won Ever: </span> {badge.statistics.awardedCount}
      </p>
    </div>
  </div>
}

export default AssetListBadgeEntry;