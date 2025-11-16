import {createUseStyles} from "react-jss";
import updatePlaceStore from "../../stores/updatePlaceStore";
import FeedbackStore from "../../../../stores/feedback";
import {useEffect, useRef, useState} from "react";
import useButtonStyles from "../../../../styles/buttonStyles";
import devProductsStore from "../../stores/devProductsStore";
import {parseInt} from "lodash";
import ActionButton from "../../../actionButton";
import {updateDeveloperProduct, uploadAsset} from "../../../../services/develop";
import ItemImage from "../../../itemImage";
import {getModerationStatus} from "../../../../services/catalog";
import {ModerationStatusStr} from "../../../../models/enums";

const useStyles = createUseStyles({
    image: {
        width: "75%",
        height: "75%",
        aspectRatio: "1 / 1",
        padding: 0,
        border: '1px solid #d8d8d8',
    },
    normal: {
        fontSize: 16,
        padding: '1px 10px 3px 10px'
    },
    rightButton: {
        marginLeft: 5,
    },
});

const UpdateDevProduct = () => {
    const store = updatePlaceStore.useContainer();
    const productStore = devProductsStore.useContainer();
    const s = useStyles();
    const buttonStyles = useButtonStyles();
    const feedback = FeedbackStore.useContainer();
    
    const [name, setName] = useState(null);
    const [desc, setDesc] = useState(null);
    const [price, setPrice] = useState(0);
    const fileRef = useRef(null);
    const [imageAssetId, setImageAssetId] = useState(0);
    const [newImage, setNewImage] = useState(false);
    
    useEffect(() => {
        store.setLocked(true);
        /** @type DeveloperProductEntry */
        const info = productStore.products.find(prod => prod.id === productStore.selectedProduct);
        if (!info) {
            productStore.setResult('Product not found in update product page.');
            productStore.setSelectedPage(0);
            store.setLocked(false);
            return null;
        }
        
        getModerationStatus({ assetID: info.iconImageAssetId }).then(status => {
            if (status !== ModerationStatusStr.ReviewApproved) {
                productStore.setResult('This Developer Product is not approved yet. Wait until a moderator approves it.');
                productStore.setSelectedPage(0);
                store.setLocked(false);
                return null;
            }
        });
        setName(info.name);
        setDesc(info.Description);
        setPrice(info.priceInRobux);
        setImageAssetId(info.iconImageAssetId);
        store.setLocked(false);
    }, []);
    
    const feed = (str) => {
        feedback.addFeedback(str);
        store.setLocked(false);
    }
    
    if (store.locked) {
        return <div className='row mt-4 justify-content-center align-items-center w-100 h-100'>
            <div className='flex h-100 justify-content-center align-items-center'>
                <span className='icon-loading-legacy' style={{ width: '48px!important' }}/>
            </div>
        </div>
    }
    
    return <div className="row mt-4">
        <div className="col-12">
            <h2 className="fw-bolder mb-4">Edit Developer Product</h2>
        </div>
        <div className="col-6">
            <div className="mb-3">
                <p className="mb-0 fw-bold mt-2">Name:</p>
                <input className='w-100' disabled={store.locked} value={name || ''} maxLength={100} onChange={e => setName(e.target.value)} type={'text'}/>
            </div>
            <div className="mb-3">
                <p className="mb-0 fw-bold mt-2">Description:</p>
                <textarea disabled={store.locked} rows={6} className='w-100' maxLength={1000} value={desc || ''} onChange={e => setDesc(e.currentTarget.value)}/>
            </div>
            <div className="mb-3">
                <p className="mb-0 fw-bold mt-2">Price In ROBUX:</p>
                <input disabled={store.locked} value={price} max={999999} onChange={e => setPrice(parseInt(e.target.value))} inputMode={'numeric'} type={'text'}/>
            </div>
            <div className="mb-3">
                <p className="mb-0 fw-bold mt-2">Select image:</p>
                <input disabled={store.locked || newImage} ref={fileRef} accept='image/*' type="file" onChange={e => {
                    e.preventDefault();
                    if (!fileRef?.current?.files?.length) return feed('You must select a file.');
                    let image = fileRef?.current?.files[0];
                    if (image.size >= 8e+7) return feed('The image is too large.');
                    if (image.size === 0) return feed('The image is empty.');
                    
                    store.setLocked(true);
                    uploadAsset({ name: 'Developer Product Icon', assetTypeId: 1, file: image, groupId: null }).then(d1 => d1.data).then(d => {
                        setImageAssetId(d.assetId);
                        setNewImage(true);
                        store.setLocked(false);
                    })
                }}/>
            </div>
            {
                typeof imageAssetId !== "number" || imageAssetId > 0 ? <div className='mb-3'>
                    <ItemImage className={s.image} name={name} id={imageAssetId}/>
                </div> : null
            }
            <div className="mt-4 flex">
                <ActionButton
                    disabled={store.locked}
                    className={s.normal}
                    buttonStyle={buttonStyles.continueButton}
                    label="Update"
                    onClick={e => {
                        e.preventDefault();
                        if (store.locked) return;
                        store.setLocked(true);
                        if (!name || name.length < 3 || name.length > 100) return feed('You must specify a name');
                        if (!desc) setDesc("Developer Product");
                        if ((desc || "").length > 1000) return feed('Your description is too large.');
                        if (!imageAssetId) return feed('You must set an image, or wait for your image to upload.');
                        if (isNaN(price) || typeof price != "number" || price > 1000000 || price < 0) return feed('Your price is invalid.');
                        
                        try {
                            updateDeveloperProduct({
                                universeId: store.details.universeId,
                                productId: productStore.selectedProduct,
                                name,
                                description: desc,
                                price,
                                imageId: imageAssetId
                            }).then(d1 => d1.data).then(() => {
                                productStore.refreshProducts();
                                productStore.setResult(`Product ${productStore.selectedProduct} successfully updated`);
                                productStore.setSelectedPage(0);
                                store.setLocked(false);
                            }).catch(e => {
                                feed(e);
                                store.setLocked(false);
                            });
                        } catch (e) {
                            feed(e.message);
                            store.setLocked(false);
                        }
                    }}
                />
                <ActionButton
                    disabled={store.locked}
                    className={`${s.normal} ${s.rightButton}`}
                    buttonStyle={buttonStyles.cancelButton}
                    label="Cancel"
                    onClick={e => {
                        e.preventDefault();
                        if (store.locked) return;
                        productStore.setSelectedPage(0);
                    }}
                />
            </div>
        </div>
    </div>
};

export default UpdateDevProduct;