import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  button: {
    fontSize: '16px',
    fontWeight: 500,
    color: 'var(--primary-color)!important',
    textAlign: 'center',
    padding: '4px',
    textDecoration: 'none!important',
    '&:hover': {
      color: 'var(--primary-color)!important',
      textDecoration: 'underline!important',
    }
  },
})

const SmallTextLink = (props) => {
  const s = useStyles();
  return <a className={s.button} href={props.href} onClick={props.onClick}>{props.children}</a>
}

export default SmallTextLink;