import {createContainer} from "unstated-next";
import {useEffect, useState} from "react";
import {searchGroups} from "../../../services/groups";
import getFlag from "../../../lib/getFlag";

const SearchGroupsStore = createContainer(() => {
  const [keyword, setKeyword] = useState(null);
  const [data, setData] = useState(null);
  const [locked, setLocked] = useState(true);

  useEffect(() => {
    if (!keyword && !getFlag('searchUsersAllowNullKeyword', false))
      return;
    setLocked(true);
    searchGroups({keyword: encodeURIComponent(keyword), limit: 12, offset: 0}).then(d => {
      setData(d);
      const ids = d.data.map(v => v.id);
    }).finally(() => {
      setLocked(false);
    })
  }, [keyword]);

  return {
    keyword,
    setKeyword,

    data,
    setData,

    locked,
    setLocked,
  };
});

export default SearchGroupsStore;