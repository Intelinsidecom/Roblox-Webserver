import { useState } from "react";
import { createUseStyles } from "react-jss";
import { isTouchDevice } from "../../../lib/utils";
import UserProfileStore from "../stores/UserProfileStore";

const useStyles = createUseStyles({
  body: {
    fontWeight: 300,
    marginBottom: 0,
    fontSize: '12px',
    padding: '10px 20px',
  },
  previousNamesLabel: {
    fontSize: '12px',
    fontWeight: 500,
    cursor: 'pointer',
    userSelect: 'none',
    color: 'var(--text-color-secondary)',
  },
  span: {
    color: 'inherit',
  },
  icon: {
    backgroundSize: '40px auto',
    width: '20px',
    height: '20px',
    backgroundPosition: '0 -780px',
    opacity: '.5',
    '&:hover': {
      cursor: 'pointer',
      backgroundPosition: '-20px -780px',
    }
  },
  previousNamesToolTip: {
    position: 'absolute',
    display: 'flex',
    flexDirection: 'column',
    width: '150px',
    padding: '4px 8px',
    background: 'rgba(0,0,0,0.65)',
    zIndex: 99,
    overflow: "visible",
    transition: 'opacity .15s linear',
    opacity: 0,
  },
  opacity: {
    opacity: 1,
  },
  previousName: {
    color: '#fff',
    marginBottom: 0,
    display: 'flex',
  },
});

const PreviousUsernames = props => {
  const store = UserProfileStore.useContainer();
  const [hasTouchScreen] = useState(isTouchDevice());
  const s = useStyles();
  const [tooltipOpen, setTooltipOpen] = useState(false);
  const [clickTipOpen, setClickTipOpen] = useState(false);

  const onMouseEnter = () => {
    if (hasTouchScreen) return;
    setTooltipOpen(true);
  }

  const onMouseLeave = () => {
    if (hasTouchScreen) return;
    setTooltipOpen(false);
  }

  const onClick = () => {
    if (hasTouchScreen) return;
    setClickTipOpen(!clickTipOpen);
  }

  if (store.previousNames === null || store.previousNames.length === 0) return null;

  return <div>
    <p className={s.previousNamesLabel + ' ' + s.body}>
      <span className={s.span} onMouseEnter={onMouseEnter} onMouseLeave={onMouseLeave} onClick={onClick}><span className={'icon-pastname ' + s.icon} /> Past usernames</span>
    </p>
    <div className={`${s.previousNamesToolTip} truncate ${tooltipOpen || clickTipOpen ? s.opacity : null}`}>{store.previousNames.map((v, i) => <p key={i} className={s.previousName}>{v}</p>)}</div>
  </div>
}

export default PreviousUsernames;