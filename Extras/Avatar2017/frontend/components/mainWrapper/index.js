import { createUseStyles } from "react-jss"
import FeedbackStore from "../../stores/feedback";

const useStyles = createUseStyles({
  main: {
    minHeight: '95vh',
    paddingTop: '12px',
  },
  display: {
    display: 'flex',
    flexDirection: 'column',
    minHeight: '100vh',
  }
})

const MainWrapper = ({ mainFlex, children }) => {
  const s = useStyles();
  const { feedbacks } = FeedbackStore.useContainer();
  
  return <div className={`${s.main} ${mainFlex ? s.display : null}`}>
    { feedbacks.map(({feedback, type, visible}) => {
      return <div className={`alert-pjx ${type === 0 ? 'alert-success' : type === 1 ? 'alert-loading' : 'alert-warning'} ${visible ? 'on' : undefined}`}>
        <span className="alert-text">{feedback || ''}</span>
      </div>
    })}
    {children}
  </div>
}

export default MainWrapper;