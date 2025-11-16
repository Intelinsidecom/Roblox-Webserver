import { createUseStyles } from "react-jss";

const useSubtitleStyles = createUseStyles({
  header: {
    fontWeight: 700,
    fontSize: '20px',
    lineHeight: '1.4em',
    marginBottom: '0',
    //margin: '15px 0 5px',
    padding: '0 0 5px',
  },
});

const Subtitle = props => {
  const s = useSubtitleStyles();
  return <h3 className={s.header}>{props.children}</h3>
}

export default Subtitle;