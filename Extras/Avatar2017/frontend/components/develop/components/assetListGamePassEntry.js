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
})

const AssetListGamePassEntry = props => {
  const s = useStyles();
  return <div className={s.container}>
    <p className='mb-0'><span className={s.createdLabel}>Updated: </span> {dayjs(props.updated).format('M/d/YYYY')}</p>
    <p className='mb-0' style={{ marginRight: '20%' }}><span className={s.createdLabel}>Total Sales: </span> {props.sales}</p>
  </div>
}

export default AssetListGamePassEntry;