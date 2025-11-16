import React, {useEffect, useRef} from "react";
import {createUseStyles} from "react-jss";
import AvatarInfoStore from "../stores/avatarInfoStore";
import AvatarPageStore from "../stores/avatarPageStore";

const useStyles = createUseStyles({
    submenuButton: {
        "-webkit-transition": "all 200ms ease",
        transition: "all 200ms ease",
        cursor: "pointer",
        borderRadius: 3,
        margin: "0 3px",
        padding: 7,
        float: "left",
        lineHeight: '1.3em',
        fontWeight: 300,
        "&:hover,&.active": {
            color: "#fff",
            backgroundColor: "var(--primary-color)",
        },
        "@media(max-width: 767px)": {
            margin: 0,
        }
    },
    submenuButtonActive: {
        color: "#fff",
        backgroundColor: "var(--primary-color)",
    },
    
    submenuNestContainer: {
        paddingTop: 12,
        display: "flex",
        flexWrap: "wrap",
    },
    submenuNestLabel: {
        width: 90,
        marginRight: 12,
        padding: "7px 0",
        float: "left",
        borderRight: "1px solid #e3e3e3",
        color: "#b8b8b8",
        fontWeight: 400,
        fontSize: 16,
        textAlign: "start",
    },
    submenuColumn: {
        flexDirection: 'column',
    },
});

/**
 * @typedef SubmenuData
 * @property {string} name
 * @property {number} typeId
 * @property {number} tabId
 * @property {number?} tabType
 * @property {bool} active
 */
/**
 * @typedef SubmenuNestedData
 * @property {string} label
 * @property {SubmenuData[]} items
 */
const example = {
    id: "BodyParts",
    name: "Body Parts",
    typeId: 32,
    active: false
}
const nested = [
    {
        label: "Accessories",
        items: [example],
    }
]

/**
 *
 * @param {number} mode
 * @param {SubmenuData[] | SubmenuNestedData[]} data
 * @param {any} onButtonClick
 * @returns {Element}
 * @constructor
 */
const AvatarSubmenu = ({data, onButtonClick, mode = SUBMENU_MODE.DEFAULT}) => {
    const s = useStyles();
    const ref = useRef(null);
    const page = AvatarPageStore.useContainer();
    
    useEffect(() => {
        if (mode === SUBMENU_MODE.NESTED) {
            let par = ref.current.parentElement;
            if (par) par.classList.add(s.submenuColumn);
        }
    }, [ref]);
    
    return <>
        {
            mode === SUBMENU_MODE.DEFAULT &&
            data.map(item =>
                <div
                    className={`
                    ${s.submenuButton}
                    ${
                        (page.activeTab === 0 && (page.listItemMetadata?.assetType === item.typeId || page.listItemMetadata?.recentType === item.typeId))
                        ? "active" : ""}
                    `}
                    onClick={e => onButtonClick(item, e)}
                >
                    {item.name}
                </div>
            )
        }
        {
            mode === SUBMENU_MODE.NESTED &&
            data.map(item => {
                /** @type SubmenuNestedData */
                let nest = item;
                return <div className={s.submenuNestContainer}>
                    <span className={s.submenuNestLabel}>{nest.label}</span>
                    {
                        nest.items.map(item =>
                            <div
                                className={`${s.submenuButton} ${
                                    (page.listItemMetadata?.assetType === item.typeId || page.listItemMetadata?.recentType === item.typeId) && s.submenuButtonActive
                                }`}
                                onClick={e => onButtonClick(item, e)}
                            >
                                {item.name}
                            </div>
                        )
                    }
                </div>
            })
        }
        <div ref={ref}></div>
    </>
}

export const SUBMENU_MODE = Object.freeze({
    DEFAULT: 0,
    NESTED: 1,
})

export default AvatarSubmenu;