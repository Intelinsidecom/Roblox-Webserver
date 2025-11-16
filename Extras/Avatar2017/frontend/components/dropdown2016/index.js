import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import useCardStyles from "../userProfile/styles/card";
import Link from "../link";

const useStyles = createUseStyles({
    wrapper: {
        //marginTop: '-20px',
        cursor: 'pointer',
        userSelect: 'none',
        lineHeight: '100%',
        //display: 'flex',
        '&:hover': {},
    },
    dropdown: {
        width: '125px',
        background: 'white',
        position: 'absolute',
        right: '-20px',
        borderRadius: '2px',
    },
    dropdownEntry: {
        width: '100%',
        '&:hover': {
            background: 'var(--white-color)',
        },
    },
    dropdownText: {
        marginBottom: 0,
        fontSize: '16px',
        padding: '8px 10px',
        color: 'rgb(33, 37, 41)',
    },
    dropdownDots: {
        letterSpacing: '3px',
        fontWeight: 100,
    },
    
    dropdownButton: {
        background: 'transparent',
        border: '0 transparent',
        padding: 0,
        color: 'var(--text-color-primary)',
        userSelect: 'none',
        cursor: 'pointer',
        fontWeight: '500',
        height: 'auto',
        textAlign: 'center',
        whiteSpace: 'nowrap',
        verticalAlign: 'middle',
        margin: 0,
        lineHeight: '18px',
        width: '100%',
    },
    dropdownIcon: {
        backgroundPosition: '0 -616px',
        backgroundSize: '200% auto',
        width: '28px',
        height: '28px',
        margin: 0,
        padding: 0,
        float: 'right',
        display: 'inline-block',
        backgroundImage: 'url(/img/generic.svg)',
        backgroundRepeat: 'no-repeat',
        verticalAlign: 'middle',
        '&:hover': {
            backgroundPosition: '-28px -616px',
        }
    },
    
    dropdownNew: {
        color: 'var(--text-color-primary)',
        width: 'auto',
        right: '5px',
        left: 'auto',
        backgroundColor: 'var(--white-color)',
        boxShadow: '0 -5px 20px rgba(25, 25, 25, 0.15)!important',
        maxHeight: '266px',
        borderRadius: '4px',
        backgroundClip: 'padding-box',
        float: 'left',
        fontSize: '16px',
        margin: 0,
        padding: 0,
        position: 'absolute',
        minWidth: '105px',
        overflowX: 'hidden',
        overflowY: 'auto',
        listStyle: 'none',
        textAlign: 'left',
        zIndex: '1123'
    },
    
    dropdownItem: {
        color: 'var(--text-color-primary)',
        padding: 0,
        margin: 0,
        whiteSpace: 'nowrap',
        width: '100%',
        listStyle: 'none',
        display: 'list-item',
        '&:hover': {
            backgroundColor: 'var(--background-color) !important',
            boxShadow: 'inset 4px 0 0 0 var(--primary-color)',
            color: 'var(--text-color-primary)'
        }
    },
    dropdownItemLink: {
        padding: '10px 12px',
        display: 'block',
        clear: 'both',
        lineHeight: '1.428571429',
        whiteSpace: 'nowrap',
        border: 'none',
        background: 'transparent',
        width: '100%',
        textAlign: 'left',
        textDecoration: 'none',
        cursor: 'pointer',
        fontSize: '16px',
        userSelect: 'none',
        color: 'var(--text-color-primary)',
        '&:hover': {
            color: 'var(--text-color-primary)'
        }
    },
    centered: {
        right: '20px'
    }
});

/**
 * @typedef DropdownOption
 * @property {string} name
 * @property {string} [url]
 * @property {(e: any) => void} [onClick]
 */

/**
 * Dropdown, in 2016 style.
 * @param {{
 * onlyDropdown?: boolean;
 * options: DropdownOption[];
 * centered?: boolean;
 * wrapperClass?: string;
 * dropdownClass?: string;
 * closeOnClick?: boolean;
 * }} props
 * @returns JSX.Element
 */
const Dropdown2016 = props => {
    const [isOpen, setIsOpen] = useState(false);
    const [firstOpen, setFirstOpen] = useState(false);
    const cardStyles = useCardStyles();
    const s = useStyles();
    
    useEffect(() => {
        const closeOnClick = () => {
            if (!isOpen || firstOpen) {
                setFirstOpen(false);
                return;
            }
            setIsOpen(false);
        };
        if (!props.closeOnClick) {
            document.removeEventListener('click', closeOnClick);
            return;
        }
        document.addEventListener('click', closeOnClick);
        return () => {
            document.removeEventListener('click', closeOnClick);
        };
    }, [isOpen, firstOpen]);
    
    return <div className={`${s.wrapper} ${props?.wrapperClass}`}>
        {/*<p className={'mb-0 ' + s.dropdownDots} onClick={() => {
      setIsOpen(!isOpen);
    }}>...</p>*/}
        
        {!props.onlyDropdown && <button className={s.dropdownButton}>
      <span className={s.dropdownIcon} onClick={() => {
          setIsOpen(!isOpen);
          setFirstOpen(true);
      }}></span>
        </button>}
        {
            isOpen || props.onlyDropdown ? <ul
                className={`${s.dropdownNew} ${cardStyles.card} ${props.centered ? s.centered : ''} ${props.dropdownClass}`}>
                {
                    props.options.map(v => {
                        /*return <a href={v.url || '#'} onClick={v.onClick}>
                          <div className={s.dropdownEntry} key={v.name}>
                            <p className={s.dropdownText}>{v.name}</p>
                          </div>
                        </a>*/
                        return <li className={s.dropdownItem} key={v.name}>
                            <Link href={v.url || '#'} onClick={v.onClick}>
                                <a className={s.dropdownItemLink} onClick={v.onClick} href={v.url || '#'}>{v.name}</a>
                            </Link>
                        </li>
                    })
                }
            </ul> : null
        }
    </div>
}

export default Dropdown2016;