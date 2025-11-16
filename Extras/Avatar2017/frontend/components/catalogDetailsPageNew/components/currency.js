import { createUseStyles } from "react-jss"

const useStyles = createUseStyles({
    currencyIcon: {
        marginTop: '-3px',
        float: 'left',
        marginRight: '3px',
        backgroundImage: 'url(/img/branded.svg)',
        backgroundRepeat: 'no-repeat',
        backgroundSize: 'auto auto',
        width: '28px',
        height: '28px',
        display: 'inline-block',
        verticalAlign: 'middle',
    },
    currencyText: {
        lineHeight: '1.16em',
        float: 'left',
        fontSize: '20px',
        fontWeight: '700',
    },
})

const Currency = props => {
    const s = useStyles()

    const bgPos = props.isRobux === true ? '0 -112px' : '0 -140px'
    const textColor = props.isRobux === true ? '#CC9E71' : '#02b757'

    return <div>
        <span className={s.currencyIcon} style={{ backgroundPosition: bgPos }} />
        <span className={s.currencyText} style={{ color: textColor }}>{props.currencyAmount}</span>
    </div>
}

export default Currency