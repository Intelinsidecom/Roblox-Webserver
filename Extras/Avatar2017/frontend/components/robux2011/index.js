import { createUseStyles } from "react-jss";

const useRobuxStyles = createUseStyles({
  robuxContainer: {
    display: 'inline-flex',
    flexWrap: 'nowrap',
    alignItems: 'center',
    verticalAlign: 'sub',
  },
  text: {
    color: '#060',
    fontWeight: '400',
    fontSize: '12px',
    marginBottom: 1
  },
  image: {
    background: `url("/img/img-robux.png")`,
    width: '18px',
    height: '12px',
    display: 'inline-block',
    marginLeft: '0',
    //marginTop: '4px',
    marginRight: '2px',
  },
});

const Robux2011 = props => {
  const s = useRobuxStyles();
  return <div className={s.robuxContainer}>
    <span className={s.image}></span>
    <span className={s.text}>{props.children}</span>
  </div>
}

export default Robux2011;