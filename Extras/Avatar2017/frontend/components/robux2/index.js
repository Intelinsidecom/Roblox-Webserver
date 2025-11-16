import { createUseStyles } from "react-jss";

const useRobuxStyles = createUseStyles({
  text: {
    color: '#02b757',
    fontWeight: 500,
    fontSize: '16px',
    float: 'left',
    lineHeight: '1em',
    marginLeft: 3
  },
  image: {
    //background: `url("/img/img-robux-2048.png")`,
    //width: '16px',
    //height: '16px',
    float: 'left',
    lineHeight: '1em',
    //marginLeft: '0',
    //marginTop: '4px',
    //marginRight: '2px',
  },
  prefix: {
    float: 'left',
    marginRight: '4px',
  },
});

// New ROBUX icon in use here

const Robux2 = props => {
  const s = useRobuxStyles();
  return <>
    {//props.prefix ? <span className={s.text + ' ' + s.prefix}>{props.prefix}</span> : null
    }
    <span className={`icon-robux-16x16 ${s.image}`}></span>
    <span className={s.text}>{props.children}</span>
  </>
}

export default Robux2;