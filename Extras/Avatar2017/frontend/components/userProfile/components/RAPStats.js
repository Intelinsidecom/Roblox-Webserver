import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../../lib/numberUtils";
import Link from "../../link";

const useStyles = createUseStyles({
  statHeader: {
    color: 'var(--text-color-quinary)',
    fontWeight: 400,
    marginBottom: 0,
    textAlign: 'center',
    fontSize: '18px',
  },
  statValue: {
    fontWeight: 300,
    marginBottom: 0,
    textAlign: 'center',
    fontSize: '20px',
    '&> a': {
      color: 'var(--primary-color)',
      '&:hover': {
        textDecoration: 'underline!important',
      }
    }
  },
});

const RAPStats = props => {
  const { label, value, userId } = props;
  const s = useStyles();

  return <div className='col-12 col-lg-2'>
    <p className={s.statHeader}>{label}</p>
    <p className={s.statValue}>
      <Link href={`/internal/collectibles?userId=${userId}`}>
        <a>
          {value}
        </a>
      </Link>
    </p>
  </div>
}

export default RAPStats;