import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../../lib/numberUtils";
import Link from "../../link";

const useStyles = createUseStyles({
  statHeader: {
    margin: 0,
    textAlign: 'center',
    fontSize: '18px',
    fontWeight: 500,
    lineHeight: '1.4em',
    color: 'var(--text-color-secondary)',
    '@media(max-width: 767px)': {
      fontSize: '16px',
    },
  },
  statValue: {
    fontWeight: 400,
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

  statTextContainer: {
    color: "var(--primary-color)!important",
    textDecoration: 'none!important',
    '&:hover': {
      color: "var(--primary-color)!important",
      cursor: 'ponter',
      textDecoration: 'underline!important',
    }
  },
  statText: {
    fontSize: '20px',
    fontWeight: 400,
    lineHeight: '1em',
    margin: 0,
    padding: '5px 0',
    color: 'var(--primary-color)!important',
    textDecoration: 'none!important',
    '@media(max-width: 767px)': {
      padding: 0,
    },
    '&:hover': {
      color: 'var(--primary-color)!important',
      textDecoration: 'underline!important',
    }
  },
});

const RelationshipStatistics = props => {
  const { id, label, value, userId } = props;
  const s = useStyles();

  return <li style={{
    width: '25%', float: 'left', padding: '0 5px', textAlign: 'center'
  }}>
    <div className={s.statHeader}>{label}</div>
    {/*<Link href={`/users/${userId}/friends#!${id}`}>
      <a className={s.statValue}>
        {Number.isSafeInteger(value) ? abbreviateNumber(value) : '...'}
      </a>
    </Link>*/}
    <Link href={id === "rap" ? `/internal/collectibles?userId=${userId}` : `/users/${userId}/friends#!${id}`}>
      <a className={s.statTextContainer}>
        <h3 className={s.statText}>{Number.isSafeInteger(value) ? abbreviateNumber(value) : '...'}</h3>
      </a>
    </Link>
  </li>
}

export default RelationshipStatistics;