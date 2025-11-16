import { createUseStyles } from "react-jss";
import Link from "../../link";
import { themeType } from "../../../services/theme";
import { useState } from "react";

const formatCount = num => {
  if (num > 99) return '99+';
  return num;
}

const useStyles = createUseStyles({
  linkEntry: {
    marginBottom: '0',
    paddingTop: '5px',
    color: 'inherit',
  },
  name: {
    fontSize: '16px',
    fontWeight: 500,
    verticalAlign: 'middle',
    color: p => p.theme === themeType.obc2016 ? 'var(--white-color)' : 'var(--text-color-primary)',
  },
  wrapper: {
    color: 'inherit!important',
    '&:hover': {
      cursor: 'pointer',
    },
  },
  link: {
    color: 'inherit',
  },
  countWrapper: {
    float: 'right',
    paddingTop: '5px',
  },
  count: {
    background: '#01a2fd',
    color: 'white',
    borderRadius: '10px',
    padding: '2px 7px',
  },
  icon: {
    filter: p => p.theme === themeType.obc2016 ? 'invert(100%)' : 'none',
  },
  iconNoFilter: {
    filter: 'none!important',
  },
});

/**
 * Nav sidebar link entry
 * @param {{count?: number; name: string; icon: string; url: string; theme: any;}} props 
 * @returns 
 */
const LinkEntry = props => {
  const s = useStyles({ theme: props.theme });
  const [hover, setHover] = useState(false);

  return <Link href={props.url}>
    <a className={s.link} style={{ color: 'inherit' }}>
      <div className={s.wrapper + ' hover-' + props.icon}
        onMouseEnter={() => setHover(true)}
        onMouseLeave={() => setHover(false)}
      >
        <p className={s.linkEntry}>
          <span className={`${props.icon} ${s.icon} ${hover ? s.iconNoFilter : null}`}></span> <span className={s.name}>{props.name}</span>
          {props.count && <span className={s.countWrapper}>
            <span className={s.count}>{formatCount(props.count)}</span>
          </span> || null}
        </p>
      </div>
    </a>
  </Link>
}

export default LinkEntry;