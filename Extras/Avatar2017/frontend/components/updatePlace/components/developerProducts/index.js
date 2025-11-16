import {createUseStyles} from "react-jss";
import React, {useEffect} from "react";
import useButtonStyles from "../../../../styles/buttonStyles";
import devProductsStore from "../../stores/devProductsStore";
import ActionButton from "../../../actionButton";
import CreatePage from "./createPage";
import UpdatePage from "./updatePage";
import {getGameUrl} from "../../../../services/games";
import updatePlaceStore from "../../stores/updatePlaceStore";

const useEntryStyles = createUseStyles({
    row: {},
    td: {
        paddingTop: '10px',
        paddingBottom: '6px',
        paddingLeft: '5px',
        borderBottom: '1px solid var(--background-color)',
    },
    block: {
        display: 'inline-block',
    },
    image: {
        borderRadius: '50%',
        border: '1px solid var(--text-color-quinary)',
        maxWidth: '30px',
    },
    senderName: {
        position: 'relative',
        top: '-10px',
        marginBottom: 0,
        left: '4px'
    },
    viewDetails: {
        cursor: 'pointer',
    },
    imageBorder: {
        borderRadius: '100%',
        overflow: 'hidden',
    },
});

const DevProdEntry = props => {
    /** @type DeveloperProductEntry */
    const prod = props;
    const store = devProductsStore.useContainer();
    const s = useEntryStyles();
    
    return <tr className={s.row}>
        <td className={s.td} style={{width: '15%'}}>{prod.id}</td>
        <td className={s.td} style={{width: '45%'}}>{prod.name}</td>
        <td className={s.td} style={{width: '20%'}}>{prod.priceInRobux}</td>
        <td className={s.td} style={{width: '10%'}}>{prod.sales}</td>
        <td className={s.td} style={{width: '10%'}}>
            <a className='link2018' onClick={e => {
                e.preventDefault();
                store.setSelectedProduct(prod.id);
                store.setSelectedPage(2);
                store.setResult(null);
            }}>Edit</a>
        </td>
    </tr>
}

const useStyles = createUseStyles({
    table: {
        width: '100%',
    },
    tableHead: {
        borderTop: '1px solid #b9b9b9',
        borderBottom: '2px solid var(--background-color)',
        background: '#f1f1f1',
    },
    tableHeadLabel: {
        paddingTop: '5px',
        paddingBottom: '5px',
        paddingLeft: '5px',
        color: '#5d5a5b',
    },
    tableHeadBorder: {
        borderRight: '2px solid #e1e1e1',
    },
    tableAction: {
        width: '60px',
    },
    button: {
        marginLeft: 8,
        fontSize: 16,
        marginTop: 5,
        padding: '1px 10px 3px 10px'
    },
    normal: {
        fontSize: 16,
    },
});

const DeveloperProductPage = () => {
    const store = updatePlaceStore.useContainer();
    const productStore = devProductsStore.useContainer();
    const s = useStyles();
    const buttonStyles = useButtonStyles();
    
    useEffect(() => {
        productStore.refreshProducts();
    }, []);
    
    // <div className="mb-3">
    //     <p className="mb-0 fw-bold mt-2">Select image:</p>
    //     <input ref={fileRef} type="file"/>
    // </div>
    // <div className="mt-4">
    //     <ActionButton
    //         disabled={store.locked}
    //         className={s.normal + " " + s.continueButton}
    //         label="Upload"
    //         onClick={uploadPlace}
    //     />
    // </div>
    
    if (productStore.selectedPage === 1) {
        return <CreatePage />
    } else if (productStore.selectedPage === 2) {
        return <UpdatePage />
    }
    
    return <div className="row mt-4">
        <div className="col-12 flex justify-content-start" style={{marginBottom: '1.5rem'}}>
            <h2 className="fw-bolder mb-0">Developer Products</h2>
            <ActionButton className={s.button} buttonStyle={buttonStyles.continueButton} label='Create new'
                          onClick={e => {
                              e.preventDefault();
                              productStore.setSelectedPage(1);
                              productStore.setResult(null);
                          }}/>
        </div>
        <div className="col-9">
            {
                typeof productStore.result === 'string' ?
                    <span className='status-confirm w-100 d-inline-block'
                          style={{marginBottom: 10}}>{productStore.result}</span>
                    : null
            }
            {
                Array.isArray(productStore.products) ?
                    <table className={s.table}>
                        <thead className={s.tableHead}>
                        <tr>
                            <th className={s.tableHeadLabel + ' ' + s.tableHeadBorder}>ID</th>
                            <th className={s.tableHeadLabel + ' ' + s.tableHeadBorder}>Name</th>
                            <th className={s.tableHeadLabel + ' ' + s.tableHeadBorder}>Price In ROBUX</th>
                            <th className={s.tableHeadLabel + ' ' + s.tableHeadBorder}>Sales</th>
                            <th className={s.tableHeadLabel + ' ' + s.tableHeadBorder}>Edit</th>
                            {/*<th className={s.tableHeadLabel + ' ' + s.tableAction}>Action</th>*/}
                        </tr>
                        </thead>
                        <tbody>
                        {
                            productStore.products.map(v => <DevProdEntry key={v.id} {...v} />)
                        }
                        </tbody>
                    </table>
                    : productStore.products === 0 ? <p>Loading...</p> : <p>Developer Products failed to load.</p>
            }
        </div>
        <div className='col-9 mt-4'>
            <div className='d-inline-block'>
                <ActionButton disabled={store.locked} buttonStyle={buttonStyles.continueButton} className={s.normal}
                              label='Save'
                              onClick={() => {
                                  window.location.href = getGameUrl({placeId: store.placeId, name: 'placeholder'})
                              }}/>
            </div>
            <div className='d-inline-block mt-5 ms-1'>
                <ActionButton disabled={store.locked} buttonStyle={buttonStyles.cancelButton} className={s.normal}
                              label='Cancel'
                              onClick={() => {
                                  window.location.href = getGameUrl({placeId: store.placeId, name: 'placeholder'})
                              }}/>
            </div>
        </div>
    </div>
};

export default DeveloperProductPage;
