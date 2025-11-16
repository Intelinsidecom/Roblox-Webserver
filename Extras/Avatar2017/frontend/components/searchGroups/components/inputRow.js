import searchUsersStore from "../stores/searchGroupsStore";
import {createUseStyles} from "react-jss";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import {useEffect, useState} from "react";

const useStyles = createUseStyles({
  column: {
    display: 'inline-block',
  },
  input: {
    width: '100%',
  },
  searchButton: {
    fontSize: '16px',
    paddingLeft: '6px',
    paddingRight: '6px',
  },
})

const InputRow = props => {
  const store = searchUsersStore.useContainer();
  const s = useStyles();
  const btnStyles = useButtonStyles();
  const [query, setQuery] = useState('');
  useEffect(() => {
    if (store.keyword === query && query !== '') return;
    setQuery(store.keyword);
  }, [store.keyword]);

  const onClick = e => {
    store.setKeyword(query);
  }

  return <div className='row'>
    <div className='col-2 col-lg-1'>
      <p className='fw-bold'>Search:</p>
    </div>
    <div className='col-8 col-lg-9'>
      <input disabled={store.locked} value={query} onChange={(e) => {
        setQuery(e.currentTarget.value);
      }} className={s.input} type='text' placeholder='Group Name Query' />
    </div>
    <div className='col-2'>
      <ActionButton disabled={store.locked} onClick={onClick} className={btnStyles.buyButton + ' ' + btnStyles.normal + ' ' + s.searchButton} label='Search Groups' />
    </div>
  </div>
}

export default InputRow;