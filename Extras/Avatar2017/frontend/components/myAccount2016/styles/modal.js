import { createUseStyles } from "react-jss";

const useModalStyles = createUseStyles({
  input: {
    width: '100%',
    border: '1px solid var(--text-color-quinary)',
    padding: '8px 14px',
    marginTop: '10px',
  },
  confirmWrapper: {
    width: '100px',
    margin: '0 auto',
    display: 'block',
    marginTop: '8px',
  },
  confirmWrapperWide: {
    width: '200px',
  }
});

export default useModalStyles