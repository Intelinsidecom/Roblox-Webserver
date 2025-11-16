import { createUseStyles } from "react-jss";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";

const useStyles = createUseStyles({
  button: {
    color: 'white',
    textAlign: 'center',
  },
  buttonWrapper: {
    background: 'var(--primary-color)',
    width: '100%',
    color: 'white',
    textAlign: 'center',
    padding: '5px 10px',
    borderRadius: '4px',
    '&:hover': {
      background: '#32B5FF',
      boxShadow: '0 1px 3px rgb(150 150 150 / 74%)',
    },
  },
})

/**
 * 
 * @param {{onClick?: (e: any) => void; href?: string; children: any; className?: string;}} props 
 * @returns 
 */
const SmallButtonLink = (props) => {
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const hrefLink = () => {
    window.location.href = props.href || '#'
  }
  //return <a className={s.button} href={props.href} onClick={props.onClick}></a>
  //return <ActionButton buttonStyle={buttonStyles.} disabled={false} onClick={props.onClick || hrefLink} className={`${props.className}`} children={props.children}></ActionButton>
  return <a className={s.button} href={props.href} onClick={props.onClick}><div className={s.buttonWrapper + ' ' + (props.className || '')}>{props.children}</div></a>
}

export default SmallButtonLink;