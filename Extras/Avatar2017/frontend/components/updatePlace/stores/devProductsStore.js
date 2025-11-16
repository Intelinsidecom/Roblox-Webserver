import {createContainer} from "unstated-next";
import {useState} from "react";
import {getDeveloperProducts} from "../../../services/develop";
import UpdatePlaceStore from "./updatePlaceStore";
import FeedbackStore from "../../../stores/feedback";

const DevProductsStore = createContainer(() => {
    const updatePlaceStore = UpdatePlaceStore.useContainer();
    const feedback = FeedbackStore.useContainer();
    // 0 = loading, 1 = failed, array is susccess
    const [products, setProducts] = useState(0);
    // 0 = dev prod list, 1 = create dev prod, 2 = update dev prod
    const [selectedPage, setSelectedPage] = useState(0);
    const [selectedProduct, setSelectedProduct] = useState(null);
    const [result, setResult] = useState(null);
    
    const refreshProducts = () => {
        setProducts(0);
        try {
            getDeveloperProducts({ universeId: updatePlaceStore.details.universeId }).then(d => {
                setProducts(d.sort((a ,b) => a.id - b.id));
            });
        } catch (e) {
            setProducts(1);
            feedback.addFeedback(e.message);
        }
    }
    
    return {
        products,
        setProducts,
        
        selectedPage,
        setSelectedPage,
        
        selectedProduct,
        setSelectedProduct,
        
        result,
        setResult,
        
        refreshProducts
    }
});

export default DevProductsStore;