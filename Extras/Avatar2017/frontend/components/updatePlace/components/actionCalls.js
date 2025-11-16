import { createUseStyles } from 'react-jss';
import { useState, useEffect, useRef } from 'react';
import ActionButton from '../../../components/actionButton';
import ConfirmUploadModal from './confirmUploadModal';
import {
    uploadAutoGenGameIcon,
    uploadAutoGenGameThumbnail,
    uploadGameIcon,
    uploadGameThumbnail
} from '../../../services/develop'
import useButtonStyles from '../../../styles/buttonStyles';

/*const useStyles = createUseStyles({
    uploadContainer:{
        display: 'flex',
        flexDirection: 'column'
    },
    feedback:{
        padding:'15px',
        backgroundColor: '#E2EEFE',
        border: '1px solid #6586A3',
        fontSize: '16px',
        fontWeight: '400',
        lineHeight: '1.4em', 
    },
    noteText:{
        marginTop: '5px',
        fontSize: '10px',
        fontWeight: '500',
        lineHeight: '1.4em',
        display: 'block',
        width: '100%',
        fontStyle: 'italic',
        color: '#d2d2d2'
    },
    loading:{
        background: 'url(/loading.gif)',
        backgroundRepeat: 'no-repeat',
        aspectRatio: '164/48',
        width: '100%!important',
    },

})*/

const useStyles = createUseStyles({
    formContainer: {
        fontWeight: '400',
    },
    input: {
        marginRight: '3%',
        marginBottom: '15px'
    },
    noteText: {
        marginTop: '5px',
        marginBottom: '20px',
        fontSize: '10px',
        fontWeight: '500',
        lineHeight: '1.4em',
        display: 'block',
        width: '100%',
        fontStyle: 'italic',
        color: '#A1A2A4'
    },
    actionButton: {
        borderRadius: '1px',
        fontSize: '14px',
        fontWeight: '400',
        lineHeight: 'normal',
        padding: '5px',
        margin: 0,
        width: 'auto!important'
    },
    autoText:{
        fontSize: '12px'
    },
    autoUl:{
        padding: 0,
        margin: 0,
        marginTop: '5px',
        listStyle: 'square',
        '& li':{
            padding: 0,
            margin: 0,
            marginBottom: '5px',
        }
    },
    feedback:{
        padding:'15px',
        backgroundColor: '#E2EEFE',
        border: '1px solid #6586A3',
        fontSize: '16px',
        fontWeight: '400',
        lineHeight: '1.4em', 
    },
})

const Icon = props => {
    const s = useStyles();
    const butStyles = useButtonStyles();

    /**
   * @type {React.Ref<HTMLInputElement>}
   */
    const fileRef = useRef(null);

    const [modalOpen, setModalOpen] = useState(false);
    const [loading, setLoading] = useState(false);

    const [mediaType, setMediaType] = useState('custom')

    const handleMediaChange = e => {
        setMediaType(e.target.value)
    }
    
    const feed = string => {
        props.feedback.addFeedback(string);
        setModalOpen(false);
        setLoading(false);
    }
    
    const onSubmit = () => {
        //e.preventDefault();
        if (loading) return;
        if (mediaType === 'custom') {
            if (!fileRef.current.files.length) return feed('You must select a file.');
            let image = fileRef.current.files[0];
            if (image.size >= 8e+7) return feed('The image is too large.');
            if (image.size === 0) return feed('The image is empty.');
            setLoading(true);
            setModalOpen(false);
            
            uploadGameIcon({
                placeId: props.placeId,
                file: image,
            }).then(() => {
                setLoading(false);
                props.refreshIcon();
                //window.location.reload();
            }).catch(e => {
                props.feedback.addFeedback(e.message);
                setLoading(false);
            })
        } else {
            setLoading(true);
            setModalOpen(false);
            
            uploadAutoGenGameIcon({
                placeId: props.placeId
            }).then(() => {
                setLoading(false);
                props.refreshIcon();
                //window.location.reload();
            }).catch(e => {
                props.feedback.addFeedback(e.message);
                setLoading(false);
            })
        }
    }

    const modalPopup = e => {
        e.preventDefault();
        setModalOpen(true);
    }

    const onClose = () => {
        setModalOpen(false);
    }

    return <form className={s.formContainer}>
        {modalOpen && <ConfirmUploadModal
            onClose={onClose}
            onConfirm={onSubmit}
            title="Add Icon"
            message="Are you sure you want to add this icon? This will delete your existing icon."
        ></ConfirmUploadModal>
        }
        <input // Custom Image
            className={s.input}
            id='customImage'
            name='option'
            value='custom'
            type='radio'
            checked={mediaType === 'custom'}
            onChange={handleMediaChange}
        />
        <label htmlFor='customImage'>Image</label>
        <br />
        <input // Auto generated
            className={s.input}
            id='autoImage'
            name='option'
            value='auto'
            type='radio'
            checked={mediaType === 'auto'}
            onChange={handleMediaChange}
        />
        <label htmlFor='autoImage'>Auto generated Image</label>
        {mediaType === 'auto' ?
            <>
            {/*<p className={s.autoText}>If you want an auto generated image, you can either:<br/><ul className={s.autoUl}><li>Go into studio and re-publish your game</li><li>Re-upload your game through the website</li></ul>and it will overwrite the current thumbnail with your new auto generated thumbnail.</p>*/}
            {/*    <p className={s.noteText}>Being able to generate a thumbnail with a button will be added soon.</p>*/}
                {/*<ActionButton className={s.actionButton} buttonStyle={butStyles.continueButton} label='Save Changes' onClick={modalPopup} />
            */}
                <ActionButton className={s.actionButton} buttonStyle={butStyles.continueButton} label='Set Icon' onClick={modalPopup} />
            </>
            :
            <>
                <p style={{ marginBottom: '5px' }}>Select image:</p>
                <input style={{ marginBottom: '15px', maxWidth: '100%' }} accept='image/*' ref={fileRef} type="file" />
                <ActionButton className={s.actionButton} buttonStyle={butStyles.continueButton} label='Upload Image' onClick={modalPopup} />
                <p className={s.noteText}>Icon must be square.</p>
                {/*<p className={s.noteText}>Note that publishing your game after setting a custom thumbnail WILL OVERWRITE your previous thumbnail with an auto-generated image.</p>*/}
            </>
        }
    </form>

    /*return <div className={s.uploadContainer}>
        {modalOpen && <ConfirmUploadModal
            onClose={setModalOpen(false)}
            onConfirm={onSubmit}
        />
        }
        {feedback && <p className={s.feedback}>{feedback}</p>}
        <p>Select image:</p>
        <input accept='image/*' ref={fileRef} type="file"/>
        {!loading && <ActionButton label='Upload Image' onClick={modalPopup} />}
        <p className={s.noteText}>Icon must be square.</p>
        {loading && <span className={s.loading} src='/loading.gif'></span>}
    </div>*/
};

export default Icon;