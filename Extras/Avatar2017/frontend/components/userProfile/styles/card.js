import { createUseStyles } from "react-jss";

const useCardStyles = createUseStyles({
  card: {
    borderRadius: 0,
    //boxShadow: '0 1px 4px 0 rgb(25 25 25 / 30%)',
    boxShadow: 'none',
    background: 'white',
    margin: '0 0 18px',
    border: 0,
  },
});

export default useCardStyles;