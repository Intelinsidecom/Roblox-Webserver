import updatePlaceStore from "./stores/updatePlaceStore";
import {useEffect} from "react";
import VerticalSelector from "../verticalSelector";
import BasicSettings from "./components/basicSettings";
import {getGameMedia, multiGetPlaceDetails} from "../../services/games";
import Icon from "./components/icon";
import UploadPlace from "./components/uploadPlace";
import Access from "./components/access";
import Thumbnail from "./components/thumbnail";
import DeveloperProductPage from "./components/developerProducts";

const Container = props => {
  const store = updatePlaceStore.useContainer();
  useEffect(() => {
    store.setTab('Basic Settings');
    store.setPlaceId(props.placeId);
    store.refreshPlaceDetails(props.placeId);
  }, [props]);

  const handler = (opt) => {
    return (e) => {
      e.preventDefault();
      store.setTab(opt);
    }
  }
  const options = [
    {
      name: 'Basic Settings',
      url: '#',
      el: () => <BasicSettings />,
    },
    {
      name: 'Upload',
      url: '#',
      el: () => <UploadPlace />,
    },
    {
      name: 'Game Icon',
      url: '#',
      el: () => <Icon />,
    },
    {
      name: 'Thumbnails',
      url: '#',
      el: () => <Thumbnail />,
    },
    {
      name: 'Access',
      url: '#',
      el: () => <Access />,
    },
    {
      name: 'Permissions',
      url: '#',
      disabled: true,
      el: () => null,
    },
    {
      name: 'Version History',
      url: '#',
      disabled: true,
      el: () => null,
    },
    {
      name: 'Developer Products',
      url: '#',
      el: () => <DeveloperProductPage />,
    },
    {
      name: 'Games',
      url: '#',
      disabled: true,
      el: () => null,
    },
  ].filter(v => !!v).map(v => {
    v.onClick = handler(v.name);
    return v;
  });
  const selected = options.find(v => v.name === store.tab);

  return <div className='container ssp card pb-4 br-none'>
    <div className='row'>
      <div className='col-12'>
        <h2 className='fw-bolder ms-2' style={{ fontWeight: '600!important' }}>Configure Place</h2>
      </div>
    </div>
    <div className='row'>
      <div className='col-2'>
        <VerticalSelector selected={store.tab} options={options} />
      </div>
      <div className='col-10'>
        {(selected && store.details) ? selected.el() : null}
      </div>
    </div>
  </div>
}

export default Container;