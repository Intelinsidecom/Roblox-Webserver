import dayjs from "dayjs";
import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { createComment, getComments } from "../../../../services/catalog";
import AuthenticationStore from "../../../../stores/authentication";
import useButtonStyles from "../../../../styles/buttonStyles";
import ActionButton from "../../../actionButton";
import CreatorLink from "../../../creatorLink";
import ReportAbuse from "../../../reportAbuse";
import PlayerImage from "../../../playerImage";
import getFlag from "../../../../lib/getFlag";
import PlayerHeadshot from "../../../playerHeadshot";
import Link from "../../../link";

const useCreateCommentStyles = createUseStyles({
    createCommentTextArea: {
        width: '100%',
        background: '#ecf4ff',
        border: '2px solid #dee1ec',
        borderRadius: '4px',
        '&:focus-visible': {
            outline: 'none',
        },
    },
    buttonWrapper: {
        position: 'relative',
        width: '80px',
        float: 'right',
        marginTop: '-45px',
        marginRight: '13px',
    },
    continueButton: {
        //fontSize: '14px',
        padding: '8px!important',
        width: '172px',
    },
    createCommentContainer: {
        paddingBottom: '0',
    },
    commentBox: {
        width: 'calc(100% - 172px - 12px)',
        fontSize: '16px',
        fontWeight: 300,
        lineHeight: '1.3em',
    },
    btnDiv: {
        margin: '0 0 0 12px',
    },
    input: {
        border: '1px solid var(--text-color-secondary)',
        color: 'var(--text-color-primary)',
        fontSize: '16px',
        fontWeight: 300,
        lineHeight: '100%',
        height: '43px',
        padding: '5px 12px',
        width: '100%',
        borderRadius: '3px',
        backgroundColor: 'var(--white-color)',
        display: 'block',
        '&:focus': {
            boxShadow: 'none',
            borderColor: 'var(--primary-color)',
            outline: 0,
        }
    },
    marginTop: {
        marginTop: '12px'
    },
    commentMsg: {
        minHeight: '1.3em',
        position: 'relative',
        lineHeight: '1.3em',
        marginTop: '40px',
        width: 'calc(100% - 172px - 12px)',
        '@media (max-width: 991px)': {
            marginTop: '45px',
            width: '100%',
        },
    },
    displayNone: {
        display: 'none',
    },
    textError: {
        fontSize: '12px',
        fontWeight: 400,
        color: '#D86868',
        lineHeight: '1.3em',
    },
    characterCount: {
        position: 'absolute',
        right: 0,
        fontSize: '14px',
        fontWeight: 400,
        color: 'var(--text-color-primary)',
        lineHeight: '1.3em',
    },
});

const CreateComment = props => {
    const s = useCreateCommentStyles();
    const buttonStyles = useButtonStyles();
    const authStore = AuthenticationStore.useContainer();
    const textAreaRef = useRef(null);
    const [textAreaRemainingChar, setTextAreaRemainingChar] = useState(200)
    const [locked, setLocked] = useState(null);
    const [error, setError] = useState(null);
    //const topMargin = error ? s.marginTop : '';
    const topMargin = ''
    const textError = error ? '' : s.displayNone;
    const postComment = e => {
        e.preventDefault();
        const text = textAreaRef.current.value;
        if (!text || text.length < 1) {
            return setError('Your comment is too short!');
        }
        if (text.length > 200) {
            return setError('Your comment is too long! It must be under 200 characters.');
        }
        setLocked(true);
        createComment({
            assetId: props.assetId,
            comment: text,
        }).then(() => {
            window.location.reload(); // todo: if we ever want to add spa support, then we need to add current comment to top of comments list
        }).catch(e => {
            const errorMessage = (() => {
                switch (e.message) {
                    case 'VerifyEmail':
                        return 'Your account must have a verified email before you can comment.';
                    case 'RequiresCaptcha':
                        return 'You must pass the captcha.';
                    case 'UserTooNew':
                        return 'Your account is too new to post comments. Try again later.';
                    case 'FloodedGlobally':
                    case 'FloodedPerAsset':
                        return 'You are being rate limited. Try again later.';
                    case 'Moderated':
                        return 'Your comment was moderated. Try again.';
                    default:
                        return 'An unknown error occurred posting your comment. Error: ' + e.message;
                }
            })();
            setError(errorMessage);
        }).finally(() => {
            setLocked(false);
        });
    }

    const calculateRemainingChar = () => {
        setTextAreaRemainingChar(200 - textAreaRef?.current?.value?.length)
    }

    return <div className={`${s.createCommentContainer}`}>
        {//error && <p className='text-danger mb-0'>{error}</p>}
        }<div className={`${s.commentBox} float-left ${topMargin}`}>
            <input onChange={calculateRemainingChar} className={s.input} placeholder="Write a comment!" maxLength={200} ref={textAreaRef} />
        </div>
        <div className={`float-right ${topMargin}`}>
            <ActionButton divClassName={s.btnDiv} buttonStyle={buttonStyles.newContinueButton} onClick={postComment} disabled={locked} className={`${s.continueButton}`} label="Post Comment" />
        </div>
        <div className={s.commentMsg}>
            <span className={`${textError} ${s.textError}`}>{error}</span>
            <span className={`${s.characterCount}`}>{textAreaRemainingChar + ' characters remaining'}</span>
        </div>
    </div>
    /*<div className='row mt-4 mb-4'>
        <div className='col-3 pe-4'>
            <PlayerHeadshot name={authStore.username} id={authStore.userId} />
        </div>
        <div className='col-9'>
            {error && <p className='text-danger mb-0'>{error}</p>}
            <textarea disabled={locked} maxLength={200} rows={8} className={s.createCommentTextArea} ref={textAreaRef} />
            <div className={s.buttonWrapper}>
                <ActionButton onClick={onClick} disabled={locked} className={buttonStyles.continueButton + ' ' + s.continueButton} label="Continue" />
            </div>
        </div>
        <div className='col-12'>
            <div className='divider-top-thick divider-light mt-3' />
        </div>
    </div>*/
}

const useCommentEntryStyles = createUseStyles({
    commentEntryDiv: {
        height: '120px',
        width: '100%',
        background: '#f6f6f5',
        border: '2px solid #e6e6e6',
        borderRadius: '4px',
    },
    commentText: {
        padding: '10px 10px',
    },
    commentCreatedAt: {
        color: '#666',
        fontSize: '12px',
        marginBottom: '8px',
    },
    report: {
        marginTop: '-15px',
        float: 'right',
    },
});

const useNewCommentEntryStyles = createUseStyles({
    commentContainer: {
        display: 'flex',
        flexDirection: 'row',
        paddingTop: '12px',
        //marginBottom: '15px'
    },
    profileContainer: {
        width: '10%',
        float: 'left',
        display: 'flex',
        aspectRatio: '1/1'
    },
    imgContainer: {
        width: 'auto',
        height: '60%',
        aspectRatio: '1/1',
        borderRadius: '100%',
        overflow: 'hidden',
    },
    img: {
        verticalAlign: 'middle',
        overflow: 'clip',
        height: '100%',
        width: '100%',
        aspectRatio: '1/1',
        cursor: 'pointer',
    },
    mainContentContainer: {
        width: '85%',
        float: 'left',
        display: 'flex',
        flexDirection: 'column',
        paddingLeft: '5px'
    },
    reportContainer: {
        width: '5%',
        float: 'right',
        marginLeft: 'auto',
    },
    commentCreatedAt: {
        color: 'var(--text-color-secondary)',
        fontSize: '14px',
        marginTop: 'calc(1em - 5px)'
    },

    reportIcon: {
        marginBottom: '4px',
        backgroundPosition: '0 -84px',
        backgroundImage: 'url(/img/branded.svg)',
        backgroundRepeat: 'no-repeat',
        backgroundSize: 'auto auto',
        width: '28px',
        height: '28px',
        display: 'inline-block',
        verticalAlign: 'middle',
        '&:hover': {
            backgroundPosition: '-28px -84px'
        }
    },
});

const CommentEntry = props => {
    const s = useNewCommentEntryStyles();
    //const createdAt = dayjs(props.PostedDate, 'MMM D[,] YYYY [|] h:mm A')
    //.fromNow();
    //const createdAt = ''

    return <div className={`${s.commentContainer} divider-top`}>
        <div className={s.profileContainer}>
            <div className={s.imgContainer}>
                <Link href={`/users/${props.AuthorId}/profile`}>
                    <a href={`/users/${props.AuthorId}/profile`}>
                        <PlayerHeadshot className={s.img} id={props.AuthorId} />
                    </a>
                </Link>
            </div>
        </div>
        <div className={s.mainContentContainer}>
            <div style={{ marginBottom: '2px', marginTop: '5px' }}>
                <CreatorLink type='User' id={props.AuthorId} name={props.AuthorName} />
            </div>
            <div style={{ marginBottom: 0 }}>
                <p style={{ wordWrap: 'break-word', height: '3em', marginBottom: 0 }}>{props.Text}</p>
            </div>
            <div style={{ marginBottom: '5px' }}>
                <p className={s.commentCreatedAt}>{props.PostedDate}</p>
            </div>
        </div>
        <div className={s.reportContainer}>
            {//<span className={s.reportIcon}></span>
            }
            <ReportAbuse smallText={true} id={props.Id} type='comment' />
        </div>
    </div>
    /*return <div className='row mt-3'>
        <div className='col-3 pe-4'>
            <PlayerImage id={props.AuthorId} />
        </div>
        <div className='col-9'>
            <div className={s.commentEntryDiv}>
                <div className={s.commentText}>
                    <div className='row'>
                        <div className='col-7'>
                            <p className={s.commentCreatedAt}>Posted {createdAt} by <CreatorLink type='User' id={props.AuthorId} name={props.AuthorName} /></p>
                        </div>
                        <div className='col-5'>
                            <div className={s.report}>
                                <ReportAbuse id={props.Id} type='comment' />
                            </div>
                        </div>
                    </div>
                    <p className='mb-0'>{props.Text}</p>
                </div>
            </div>
        </div>
    </div>*/
}

const useCommentStyles = createUseStyles({
    loadMore: {
        color: '#0055b3',
        cursor: 'pointer',
    },
    recommendedGamesContainer: {
        padding: 0,
        margin: '0 0 18px',
        width: '100%',
        float: 'left',
        position: 'relative',
        minHeight: '1px',
        display: 'flex',
        flexDirection: 'column',
        '& div, & ul': {
            '&::before,&::after': {
                content: " ",
                display: 'table',
            }
        }
    },
    containerHeader: {
        margin: '3px 0 6px',
        padding: 0,
        '& h3': {
            float: 'left',
            margin: 0,
            textAlign: 'center',
            fontSize: '20px',
            fontWeight: '700',
            lineHeight: '1em',
            padding: '5px 0',
        }
    },
    commentsContainer: {
        padding: '18px',
        position: 'relative',
        marginBottom: '15px',
        display: 'flex',
        flexDirection: 'column',
        width: '100%',
        backgroundColor: 'var(--white-color)',
    },
    noCommentFound: {
        margin: 0,
        '@media (max-width: 991px)': {
            marginTop: '24px',
        },
    },
})

const Comments = (props) => {
    const [comments, setComments] = useState(null);
    const authStore = AuthenticationStore.useContainer();
    const [offset, setOffset] = useState(0);
    const [locked, setLocked] = useState(false);
    const [areMoreAvailable, setAreMoreAvailable] = useState(false);
    const [disabled, setDisabled] = useState(false);
    const s = useCommentStyles();

    useEffect(() => {
        if (offset === 0) {
            setComments(null);
        }
        getComments({
            assetId: props.assetId,
            offset,
        }).then(data => {
            if (getFlag('commentsEndpointHasAreCommentsDisabledProp', false) && data.AreCommentsDisabled) {
                setDisabled(true);
                return;
            }
            setAreMoreAvailable(data.Comments.length >= data.MaxRows)
            //if (comments) {
            //  comments.Comments.reverse().forEach(v => {
            //    data.Comments.unshift(v);
            // })
            // }

            if (comments) {
                const newComments = data.Comments.filter(newComment =>
                    !comments.Comments.some(oldComment => oldComment.Id === newComment.Id)
                );
                setComments({
                    ...data,
                    Comments: [...comments.Comments, ...newComments]
                });
            } else {
                setComments(data);
            }
        })
    }, [props, offset]);

    if (disabled) {
        return null;
        /*return <div className='row'>
            <div className='col-12'>
                <p className='mt-4 fw-600'>Comments are disabled for this item.</p>
            </div>
        </div>*/
    }

    return <div className={`row ${s.recommendedGamesContainer}`}>
        {''//col-lg-9
        }
        <div className={s.containerHeader}>
            <h3>Comments</h3>
        </div>
        <div className={`col-12 ${s.commentsContainer}`}>
            {authStore.isAuthenticated && <CreateComment assetId={props.assetId} />}
            {
                /*comments && comments.length === 0 ? <p>No comments found.</p> : comments && comments.Comments.map(v => {
                    return <CommentEntry key={v.Id} {...v} />
                })*/
                comments && comments.Comments && comments.Comments.length === 0 ? <p className={s.noCommentFound}>No comments found.</p>
                    :
                    comments && comments.Comments && comments.Comments.map(v => {
                        return <CommentEntry key={v.Id} {...v} />
                    })
            }
            {
                !locked && comments && areMoreAvailable && <p className={`mb-0 text-center mt-2 ${s.loadMore}`} onClick={() => {
                    setOffset(offset + comments.MaxRows)
                }}>More</p>
            }
        </div>
        {/*<div className='col-12'>
            {
                !locked && comments && areMoreAvailable && <p className={`mb-0 text-center mt-2 ${s.loadMore}`} onClick={() => {
                    setOffset(offset + comments.MaxRows)
                }}>More</p>
            }
        </div>*/}
    </div >
}

export default Comments;