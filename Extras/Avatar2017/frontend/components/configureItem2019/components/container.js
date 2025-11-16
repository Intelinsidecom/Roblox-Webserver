import ConfigureItemStore from "../stores/configureItemStore";
import {useEffect} from "react";
import {getItemDetails} from "../../../services/catalog";

const Container = ({ assetId }) => {
    const store = ConfigureItemStore.useContainer();
    
    useEffect(() => {
        if (!assetId)
            return;
        
        store.setError(null);
        store.setDetails(null);
        store.setAssetId(assetId);
        getItemDetails([assetId]).then(data => {
            store.setDetails(data.data.data[0]);
        }).catch(e => {
            store.setError(e.message);
        })
    }, [assetId]);
    
    if (!store.assetId || !store.details)
        return null;
    
    return <div className='row flex-column'>
        <h2>Configure Game Pass</h2>
        <div className='section-content col-12'>
        </div>
    </div>
}

export default Container;