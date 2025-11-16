import SearchGroups from "../../components/searchGroups";
import {useRouter} from "next/dist/client/router";

const SearchGroupsPage = props => {
  const router = useRouter();
  return <SearchGroups keyword={router.query.keyword} />
}

export default SearchGroupsPage;