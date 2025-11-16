import configureItemStore from "../stores/configureItemStore";
import {ModerationStatusStr} from "../../../models/enums";

const UpdateBadge = () => {
  const store = configureItemStore.useContainer();
  
  return <div className='row'>
    <div className='col-12'>
      <h3>Configure Your Badge</h3>
      <hr className='mt-0 mb-2' />
      <p className='ps-2 pe-2'>Check the box below to allow your Badge to be given to players. Uncheck if you'd like to disable your Badge's ability to be given to players.</p>
    </div>
    <div>
      <input type='checkbox'
             disabled={store.locked || store.details.moderationStatus !== ModerationStatusStr.ReviewApproved}
             checked={store.enabled} onChange={e => {
        store.setEnabled(e.currentTarget.checked);
      }}/>
      <p className='d-inline ms-2'>Enable this badge</p>
      {
          store.details.moderationStatus !== ModerationStatusStr.ReviewApproved &&
          <p className='d-inline ms-2 text-danger'>(You cannot enable your badge, as it has not been approved yet.)</p>
      }
    </div>
  </div>
}

export default UpdateBadge;