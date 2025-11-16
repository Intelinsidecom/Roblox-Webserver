import UpdatePlaceStore from "./stores/updatePlaceStore";
import Container from "./container";
import DevProductsStore from "./stores/devProductsStore";

const UpdatePlace = props => {
  return <UpdatePlaceStore.Provider>
    <DevProductsStore.Provider>
      <Container {...props} />
    </DevProductsStore.Provider>
  </UpdatePlaceStore.Provider>
}

export default UpdatePlace;