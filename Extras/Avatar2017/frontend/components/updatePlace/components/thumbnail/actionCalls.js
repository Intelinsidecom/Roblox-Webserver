import { createUseStyles } from 'react-jss';
import { useState, useRef } from 'react';
import {uploadAutoGenGameThumbnail, uploadGameThumbnail} from "../../../../services/develop";
import useButtonStyles from "../../../../styles/buttonStyles";
import ConfirmUploadModal from "./confirmUploadModal";
import Robux2011 from "../../../robux2011";
import ActionButton from "../../../actionButton";
import {Random} from "../../../../lib/utils";
import {FeedbackType} from "../../../../models/feedback";

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
        props.feedback.addFeedback(string, FeedbackType.ERROR);
        setModalOpen(false);
        setLoading(false);
    }
    
    const onSubmit = () => {
        if (loading) return;
        if (mediaType === 'custom') {
            if (!fileRef?.current?.files?.length) return feed('You must select a file.');
            let image = fileRef?.current?.files[0];
            if (image.size >= 8e+7) return feed('The image is too large.');
            if (image.size === 0) return feed('The image is empty.');
            setLoading(true);
            setModalOpen(false);
            
            uploadGameThumbnail({
                universeId: props.universeId,
                file: image,
            }).then(() => {
                // timeout cuz it take ssome time to delete for some reason
                setTimeout(() => {
                    props.feedback.addFeedback("Thumbnail successfully added.", FeedbackType.SUCCESS);
                    setLoading(false);
                    props.refreshIcon(true);
                }, Random(28, 19) * 100)
                //window.location.reload();
            }).catch(e => {
                props.feedback.addFeedback(e.message, FeedbackType.ERROR);
                setLoading(false);
            })
        } else {
            setLoading(true);
            setModalOpen(false);
            
            uploadAutoGenGameThumbnail({
                universeId: props.universeId
            }).then(() => {
                setTimeout(() => {
                    props.feedback.addFeedback("Thumbnail successfully added.", FeedbackType.SUCCESS);
                    setLoading(false);
                    props.refreshIcon(true);
                }, Random(28, 19) * 100)
                //window.location.reload();
            }).catch(e => {
                props.feedback.addFeedback(e.message, FeedbackType.ERROR);
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
            title="Add Thumbnail"
            message="This thumbnail will be moderated. Are you sure you want to submit this thumbnail?"
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
        <label htmlFor='customImage'>Image (<Robux2011>10</Robux2011>)</label>
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
        <label htmlFor='autoImage'>Auto generated Image (Free)</label>
        {
            mediaType === 'auto' ?
                <>
                    <ActionButton className={s.actionButton} buttonStyle={butStyles.continueButton} label='Add Thumbnail' onClick={modalPopup} />
                </> :
                <>
                    <p style={{marginBottom: '5px'}}>Select image:</p>
                    <input style={{marginBottom: '15px', maxWidth: '100%'}} accept='image/*' ref={fileRef} type="file"/>
                    <ActionButton className={s.actionButton} buttonStyle={butStyles.continueButton} label='Upload Image'
                                  onClick={modalPopup}/>
                    {/*<p className={s.noteText}>Thumbnail must be 16:9 aspect ratio.</p>*/}
                </>
        }
        {
            /*mediaType === 'auto' ?
            <>
                <p className={s.autoText}>If you want an auto generated image, you can either:<br/><ul className={s.autoUl}><li>Go into studio and re-publish your game</li><li>Re-upload your game through the website</li></ul>and it will overwrite the current thumbnail with your new auto generated thumbnail.</p>
                <p className={s.noteText}>Being able to generate a thumbnail with a button will be added soon.</p>
                {/*<ActionButton className={s.actionButton} buttonStyle={butStyles.continueButton} label='Save Changes' onClick={modalPopup} />
            * /}</>
            :
            <>
                <p style={{ marginBottom: '5px' }}>Select image:</p>
                <input style={{ marginBottom: '15px', maxWidth: '100%' }} accept='image/*' ref={fileRef} type="file" />
                <ActionButton className={s.actionButton} buttonStyle={butStyles.continueButton} label='Upload Image' onClick={modalPopup} />
                <p className={s.noteText}>Icon must be square.</p>
                <p className={s.noteText}>Note that publishing your game after setting a custom thumbnail WILL OVERWRITE your previous thumbnail with an auto-generated image.</p>
            </>
        */}
    </form>
};

export default Icon;