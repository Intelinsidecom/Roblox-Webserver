import searchUsersStore from "../stores/searchGroupsStore";
import {useEffect} from "react";
import InputRow from "./inputRow";
import GroupsRow from "./groupsRow";
import {createUseStyles} from "react-jss";
const useStyles = createUseStyles({
  row: {
    background: 'var(--white-color)',
    minHeight: '100vh',
  }
})

const Container = props => {
  const store = searchUsersStore.useContainer();
  const s = useStyles();
  useEffect(() => {
    store.setKeyword(props.keyword);
    store.setData(null);
  }, [props]);

  return <div className={'row '  +s.row}>
    <div className='col-12'>
      <InputRow />
      <GroupsRow />
    </div>
  </div>
}

export default Container;