import { useRef, useState } from "react";
import { createUseStyles } from "react-jss";

const useSelectorStyles = createUseStyles({
    selectorWrapper: {
        '@media(max-width: 994px)': {
            paddingLeft: 0,
        },
    },
    selectorClosed: {
        padding: '10px 15px',
        textAlign: 'left',
        width: '100%',
        color: '#666',
        background: 'var(--white-color)',
        borderRadius: '4px',
        border: '1px solid var(--text-color-quinary)',
        fontSize: '16px',
        userSelect: 'none',
        cursor: 'pointer',
        '&:hover': {
            background: 'var(--primary-color)',
            color: 'var(--white-color)',
        },
        "& *": {
            color: "inherit",
        }
    },
    selectorOpen: {
        background: 'var(--primary-color)',
        borderColor: 'var(--primary-color)',
        color: 'var(--white-color)',
    },
    selectorCaret: {
        float: 'right',
    },
    selectorMenuOpen: {
        position: 'absolute',
        width: '100%',
        background: 'var(--white-color)',
        zIndex: 3,
    },
    selectOption: {
        padding: '10px 15px',
        marginBottom: 0,
        cursor: 'pointer',
        userSelect: 'none',
        fontSize: '16px',
        '&:hover': {
            boxShadow: '4px 0 0 0 var(--primary-color) inset',
        },
    },
});

/**
 *
 * @param {{options: {name: string; value: any}[]; onChange: (v: any) => void; value?: any; shadow?: boolean; wrapperClass?: string; className?: string;}} props
 * @returns
 */
const Selector = props => {
    const s = useSelectorStyles();
    const [open, setOpen] = useState(false);
    const [selected, setSelected] = useState(() => {
        if (props.value) {
            return props.options.find(v => v.value === props.value)
        }
        return props.options[0]
    });
    const selectorRef = useRef(null);
    
    return <div className={`${s.selectorWrapper}`}>
        <div ref={selectorRef} className={s.selectorClosed + ' ' + (open ? s.selectorOpen : '') + ' ' + props.className || ''} onClick={() => {
            setOpen(!open);
        }}>
            <span>{selected.name}</span>
            <span className={s.selectorCaret}>▼</span>
        </div>
        {
            open && selectorRef.current &&
            <div className={s.selectorMenuOpen} style={{ width: selectorRef.current.clientWidth + 'px', boxShadow: props.shadow ? "0 1px 4px 0 rgba(25,25,25,.3)" : "none" }}>
                {
                    props.options.map(v => {
                        return <p className={s.selectOption} key={v.value} onClick={() => {
                            setSelected(v);
                            setOpen(false);
                            props.onChange(v);
                        }}>{v.name}</p>
                    })
                }
            </div>
        }
    </div>
}

export default Selector;