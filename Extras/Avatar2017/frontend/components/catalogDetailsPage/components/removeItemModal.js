import React, { useState } from 'react';
import { createUseStyles } from 'react-jss';
import { setResellableAssetPrice, takeResellableAssetOffSale } from '../../../services/economy';
import useButtonStyles from '../../../styles/buttonStyles';
import ActionButton from '../../actionButton';
import OldModal from '../../oldModal';
import CatalogDetailsPage from '../stores/catalogDetailsPage';
import ItemImage from '../../itemImage';
import SelectUserAsset from './selectUserAsset';
import {removeBadgeFromSelf} from "../../../services/badges";

const useModalStyles = createUseStyles({
  inlineSelect: {
    display: 'inline-block'
  },
  select: {},
  inlineRow: {},
  buttonRow: {
    marginTop: '20px',
  },
});

const RemoveItemModal = () => {
  const buttonStyles = useButtonStyles();
  const store = CatalogDetailsPage.useContainer();
  const s = useModalStyles();
  const [toSell, setToSell] = useState(null);
  const [locked, setLocked] = useState(false);
  const [error, setError] = useState(null);

  if (!store.ownedCopies || !store.removeItemModalOpen) return null;

  return <OldModal title="Remove from Inventory">
    {error && <div className='row'><div className='col-12 text-danger mb-0'>{error}</div></div>}
    <div className='row'>
      <div className='col-3'>
        <ItemImage id={store.details.id}></ItemImage>
      </div>
      <div className='col-9 mt-4'>
        {delistable.length > 1 &&
          <SelectUserAsset selected={toSell} setSelected={setToSell} userAssets={delistable} locked={locked}></SelectUserAsset>
        }
        <p className='mb-0'>Are you sure you want to remove this badge from your inventory?</p>
      </div>
    </div>
    <div className='row mt-4'>
      <div className='col-12'>
        <div className={`${s.buttonRow} row`}>
          <div className='col-10 offset-1'>
            <div className='row'>
              <div className='col-8 pe-1'>
                <ActionButton disabled={locked} label='Remove from Inventory' className={buttonStyles.buyButton} onClick={(e) => {
                  e.preventDefault();
                  setLocked(true);
                  removeBadgeFromSelf({
                    badgeId: store.details.id,
                  }).then(() => {
                    window.location.reload();
                  }).catch(e => {
                    setLocked(false);
                    setError(e.message);
                  })
                }}></ActionButton>
              </div>
              <div className='col-4 ps-1'>
                <ActionButton disabled={locked} label='Cancel' className={buttonStyles.cancelButton} onClick={(e) => {
                  e.preventDefault();
                  store.setRemoveItemModalOpen(false);
                }}></ActionButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </OldModal>
}

export default RemoveItemModal;