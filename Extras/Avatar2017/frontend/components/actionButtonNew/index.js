import { useState } from "react";
import { createUseStyles } from "react-jss";

const useBuyButtonStyles = createUseStyles({
  btn: {
    textAlign: 'center',
    padding: '15px',
    fontSize: '21px',
    color: 'white',
    border: '1px solid #02b757',
    margin: '0',
    font: 'inherit',
    display: 'inline-block',
    fontWeight: '500',
    userSelect: 'none',
    cursor: 'pointer',
    height: 'auto',
    whiteSpace: 'nowrap',
    verticalAlign: 'middle',
    lineHeight: '100%',
    borderRadius: '5px',
    textTransform: 'none',
    '&:disabled': {
      opacity: '0.5',
    },
  },
  wrapper: {
    width: '100%',
    border: '1px solid #a7a7a7',
    background: '#e1e1e1',
  },
  wrapper2:{
    display: 'flex',
    justifyContent: 'center'
  },
  defaultBg: {
    backgroundColor: '#02b757',
    borderColor: '#02b757',
    '&:hover': {
      backgroundColor: '#3fc679',
      borderColor: '#3fc679',
    },
  },
});

const ActionButton = props => {
  const s = useBuyButtonStyles();

  return <div className={props.divClassName + ' ' + s.wrapper2}>
    <button
      disabled={props.disabled}
      className={s.btn + ' ' + (props.className || s.defaultBg)}
      onClick={props.onClick}
      title={props.disabled ? props.tooltipText : ''}>{props.label || 'Buy'}</button>
  </div>
}

export default ActionButton;
