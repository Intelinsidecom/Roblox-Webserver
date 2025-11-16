import gameDetailsStore from "../../stores/gameDetailsStore";
import { useEffect, useState } from "react";
import { multiGetGameVotes, voteOnGame } from "../../../../services/games";
import { createUseStyles } from "react-jss";
import VoteModal from './voteModal';

const useStyles = createUseStyles({
  redBg: {
    background: '#CE645B',
    width: '100%',
    height: '5px',
  },
  greenBg: {
    background: '#52A846',
    height: '5px',
  },
  borderLeft: {
    borderLeft: '1px solid var(--text-color-quinary)',
  },
  thumbsText: {
    fontSize: '12px',
  },


  votingContainer: {
    height: '30px',
    position: 'relative'
  },
  voteDetails: {
    width: '100%',
  },
  voteContainer: {
    height: '2px',
    width: '100%',
    margin: '6px 0 0 0',
  },
  voteBackground: {
    backgroundColor: 'var(--text-color-secondary)',
    width: '100%',

    position: 'absolute',
    top: 0,
    left: 0,
    height: '2px',
  },
  hasVotes: { backgroundColor: 'var(--bad-color)' },
  votePercentage: {
    backgroundColor: '#02b757',
    position: 'absolute',
    top: 0,
    left: 0,
    height: '2px',
  },

  segment: {
    backgroundColor: 'var(--white-color)',
    height: '6px',
    width: '2px',
    position: 'absolute',
    top: 0,
  },
  seg1: { left: '18%' },
  seg2: { left: '38%' },
  seg3: { left: '58%' },
  seg4: { left: '78%' },

  countLeft: {
    float: 'left',
    color: '#02b757',
  },
  countRight: {
    float: 'right',
    textAlign: 'right',
    color: '#E2231A'
  },
  voteText: {
    float: 'none',
    color: 'inherit',
    lineHeight: '20px',
    fontSize: '12px'
  },
  upvote: {
    left: 0,
    top: '-30px',
    position: 'absolute',
  },
  downvote: {
    right: 0,
    top: '-30px',
    position: 'absolute',
  },
  likeIcon: {
    color: '#02b757',
    backgroundPosition: '0 -84px',
    float: 'left',
    lineHeight: '20px',
    fontSize: '12px',
    backgroundImage: 'url(/img/games.svg)',
    backgroundRepeat: 'no-repeat',
    backgroundSize: 'auto auto',
    width: '28px',
    height: '28px',
    display: 'inline-block',
    verticalAlign: 'middle',
    cursor: 'pointer',
  },
  dislikeIcon: {
    color: '#E2231A',
    backgroundPosition: '0 -112px',
    float: 'none',
    lineHeight: '20px',
    fontSize: '12px',
    backgroundImage: 'url(/img/games.svg)',
    backgroundRepeat: 'no-repeat',
    backgroundSize: 'auto auto',
    width: '28px',
    height: '28px',
    display: 'inline-block',
    verticalAlign: 'middle',
    cursor: 'pointer',
  },
  likeIconSelected: { backgroundPosition: '-28px -84px' },
  dislikeIconSelected: { backgroundPosition: '-28px -112px' },

  voteNumbers: {
    display: 'block',
    '&::before': {
      content: " ",
      display: 'table',
    },
    '&::after': {
      content: " ",
      display: 'table',
    },
  },
});

function abbreviate(num) {
  if (num >= 1000000000) {
    return (num / 1000000000).toFixed(1) + 'B+';
  } else if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + 'M+';
  } else if (num >= 1000) {
    return (num / 1000).toFixed(1) + 'K+';
  } else {
    return num.toString();
  }
}

const Vote = props => {
  const s = useStyles();
  const store = gameDetailsStore.useContainer();
  const [votes, setVotes] = useState(null);
  const [feedback, setFeedback] = useState(null);
  const [feedbackTitle, setFeedbackTitle] = useState(null);
  const [locked, setLocked] = useState(false);

  const [voteStatus, setVoteStatus] = useState(null); // null is no vote, false is downvote, true is upvtoe

  const [showVoteModal, setVoteModal] = useState(false);

  const loadVotes = () => {
    if (store.universeDetails && store.universeDetails.id) {
      multiGetGameVotes({ universeIds: [store.universeDetails.id] }).then(data => {
        setVotes(data[0]);
      })
    }
  }
  useEffect(() => {
    loadVotes();
  }, [store.universeDetails]);

  const submitVote = (didUpvote) => {
    if (locked) return;
    setLocked(true);
    // setVoteStatus(null); maybe?
    setFeedback(null);
    setFeedbackTitle(null);

    voteOnGame({ universeId: store.universeDetails.id, isUpvote: didUpvote }).then(result => {
      loadVotes();
    }).catch(e => {
      if (!e.response || !e.response.data || !e.response.data.errors) {
        setFeedbackTitle('Unexpected Error');
        setFeedback('An unknown error has occurred. Try again.');
        setVoteModal(true);
        return
      }
      const status = e.response.status;
      const err = e.response.data.errors[0];
      const code = err.code;
      const msg = err.message;
      if (status === 403 && code === 6) {
        setFeedbackTitle('Join Game');
        setFeedback('You must visit the game before you can vote on it.');
        setVoteModal(true);
      } else if (status === 400 && (code === 3 || code === 2)) {
        setFeedbackTitle('Unable to vote');
        setFeedback('You cannot vote on this game.');
        setVoteModal(true);
      } else if (status === 429 && code === 5) {
        setFeedbackTitle('Too many votes');
        setFeedback('Too many attempts to vote. Try again later.');
        setVoteModal(true);
      } else if (msg) {
        //setFeedbackTitle('Unexpected Error');
        //setFeedback(msg);
        //setVoteModal(true);
        didUpvote ? setVoteStatus(true) : setVoteStatus(false);
      }
    }).finally(() => {
      setLocked(false);
    })
  }

  /*if (votes !== null) {*/
  if (true) {
    const upvotes = votes !== null ? votes.upVotes : 0;
    const downvotes = votes !== null ? votes.downVotes : 0;
    const total = upvotes + downvotes;
    const greenPercent = Math.ceil((upvotes / total) * 100);

    const [upvoteHovering, setUpvoteHovering] =  useState(false);
    const [downvoteHovering, setDownvoteHovering] =  useState(false);

    const upvoteMouseEntering = () => setUpvoteHovering(true);
    const upvoteMouseLeaving = () => setUpvoteHovering(false);
    const upvoteClass = (voteStatus == true || upvoteHovering) ? s.likeIconSelected : '';

    const downvoteMouseEntering = () => setDownvoteHovering(true);
    const downvoteMouseLeaving = () => setDownvoteHovering(false);
    const downvoteClass = (voteStatus == false || downvoteHovering) ? s.dislikeIconSelected : '';

    const hasVotes = (votes !== null && total > 0) ? s.hasVotes : '';

    return <div className={s.votingContainer}>
      {showVoteModal && <VoteModal modalTitle={feedbackTitle} feedbackMsg={feedback} onClose={() => {
        setVoteModal(false);
      }}></VoteModal>}

      <div className={s.upvote} onClick={() => {
        submitVote(true);
      }} onMouseEnter={upvoteMouseEntering} onMouseLeave={upvoteMouseLeaving}>
        <span className={`${s.likeIcon} ${upvoteClass}`}></span>
      </div>

      <div className={s.voteDetails}>
        <div className={s.voteContainer}>
          <div className={`${s.voteBackground} ${hasVotes}`}></div>
          <div className={s.votePercentage} style={{ width: (greenPercent + '%') }}></div>
          <div>
            <div className={s.segment + ' ' + s.seg1}></div>
            <div className={s.segment + ' ' + s.seg2}></div>
            <div className={s.segment + ' ' + s.seg3}></div>
            <div className={s.segment + ' ' + s.seg4}></div>
          </div>
        </div>
        <div className={s.voteNumbers}>
          <div className={s.countLeft}>
            <span className={s.voteText}>{abbreviate(upvotes)}</span>
          </div>
          <div className={s.countRight}>
            <span className={s.voteText}>{abbreviate(downvotes)}</span>
          </div>
        </div>
      </div>

      <div className={s.downvote} onClick={() => {
        submitVote(false);
      }} onMouseEnter={downvoteMouseEntering} onMouseLeave={downvoteMouseLeaving}>
        <span className={`${s.dislikeIcon} ${downvoteClass}`}></span>
      </div>

    </div>
  }

  return null;
}

export default Vote;