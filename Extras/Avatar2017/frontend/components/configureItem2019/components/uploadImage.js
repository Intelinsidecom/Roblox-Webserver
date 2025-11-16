import configureItemStore from "../stores/configureItemStore";
import assetTypes from "../../characterCustomizerPage/assetTypes";
import {useEffect, useState} from "react";
import {getAllGenres} from "../../../services/develop";

const Genre = props => {
    const store = configureItemStore.useContainer();
    const [genres, setGenres ] = useState([]);
    
    useEffect(() => {
        getAllGenres().then(data => setGenres(data));
    }, []);
    
    
    return <div></div>
}

export default Genre;