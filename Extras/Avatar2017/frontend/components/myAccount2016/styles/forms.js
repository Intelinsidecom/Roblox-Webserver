import { createUseStyles } from "react-jss";

const useFormStyles = createUseStyles({
  accountInfoLabel: {
    marginBottom: '6px',
    color: 'var(--text-color-quinary)',
    fontSize: '15px',
  },
  accountInfoValue: {
    color: '#666',
  },
  descInput: {
    width: '100%',
    borderRadius: '4px',
    padding: '6px 8px',
    border: '1px solid var(--text-color-quinary)',
  },
  select: {
    borderRadius: '2px',
    fontSize: '16px',
  },
  disabled: {
    background: 'white!important',
    color: 'var(--text-color-quinary)',
    '&:focus': {
      color: 'var(--text-color-quinary)',
      boxShadow: 'none',
    },
  },
  fakeInput: {
    height: '100%',
    overflow: 'hidden',
  },
  saveButtonWrapper: {
    float: 'right',
    marginTop: '16px',
  },
  saveButton: {
    background: 'white',
    border: '1px solid var(--text-color-quinary)',
    borderRadius: '4px',
    fontSize: '16px',
    padding: '4px 8px',
    cursor: 'pointer',
  },
  genderUnselected: {
    color: 'var(--text-color-quinary)',
    cursor: 'pointer',
  },
});

export default useFormStyles;