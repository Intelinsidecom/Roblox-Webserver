import {useEffect, useState} from "react";
import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  followContainer:{
    width: '68px',
    textAlign: 'center',
    cursor: 'pointer',
  },
  followButton:{
    cursor: 'pointer',
    textDecoration: 'none',
    outline: 0,
    background: 'transparent',
  },
  followIcon:{
    marginBottom: '4px',
    backgroundPosition: '0 -252px',
    backgroundImage: 'url(/img/branded.svg)',
    backgroundRepeat: 'no-repeat',
    backgroundSize: 'auto auto',
    width: '28px',
    height: '28px',
    display: 'inline-block',
    verticalAlign: 'middle',
  },
  followIconSelected:{
    backgroundPosition: '-28px -252px'
  },
  followLabel:{
    fontSize: '12px'
  },
});

const follow = props => {
  const s = useStyles();

  const [iconHovered, setIconHovered] = useState(false);

  const mouseEnter = () => setIconHovered(true);
  const mouseLeave = () => setIconHovered(false);
  const buttonClass = iconHovered ? s.followIconSelected : '';

  return <div className={s.followContainer}>
    <a className={`${s.followButton}`} href="#"
     onMouseEnter={mouseEnter} onMouseLeave={mouseLeave}
     >
        <div className={`${s.followIcon} ${buttonClass}`}></div>
        <div className={s.followLabel}>Follow</div>
    </a>
  </div>
}

export default follow;