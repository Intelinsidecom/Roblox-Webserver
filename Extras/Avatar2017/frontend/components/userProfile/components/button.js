import { createUseStyles } from "react-jss";

import useButtonStyles from "../../../styles/buttonStyles";

const useStyles = createUseStyles({
  btn: {
    padding: '9px!important'
  }
})

const Button = props => {
  const s = useButtonStyles()
  const s2 = useStyles()
  const buttonStyle = props?.noStyling ? '' : s.newCancelButton

  return <button className={`${buttonStyle} ${props.className} ${s2.btn}`} onClick={props.onClick} disabled={props.disabled} style={props.style}>{props.children}</button>
}

export default Button;