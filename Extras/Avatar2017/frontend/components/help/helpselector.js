import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import Link from ".././link";
import { useRouter } from "next/router";

const useSelectorStyles = createUseStyles({
    wrapper: {
        width: 'auto',
        color: 'var(--text-color-primary)',
        display: 'block',
        padding: '10px',
        marginBottom: '10px',
        background: 'none',
        userSelect: 'none',
        '&:hover': {
            color: 'var(--background-color-legacy)',
            backgroundColor: 'var(--primary-color)',
            borderRadius: '5px',
        },
    },
    wrapperSelected: {
        color: 'var(--background-color-legacy)!important',
        backgroundColor: 'var(--primary-color)!important',
        borderRadius: '5px',
    },
    wrapperDisabled: {
        opacity: 0.25,
    },
    text: {
        fontSize: '15px',
        color: 'inherit',
        lineHeight: '1.5',
    },
    textSelected: {
        //fontWeight: '600',
    },
});

const SelectorOption = props => {
    const s = useSelectorStyles();
    const el = <a onClick={props.onClick} className={`${s.wrapper} ${props.selected && s.wrapperSelected} ${props.disabled && s.wrapperDisabled}`}
    //{s.wrapper + (props.selected ? ' ' + s.wrapperSelected : '') + (props.disabled ? ' ' + s.wrapperDisabled : '')}
    >
        <span className={`${s.text} ${props.selected && s.textSelected}`}>{props.name}</span>
    </a>

    if (props.url) {
        return <Link href={props.url}>
            {el}
        </Link>
    }
    return el;
}

const useStyles = createUseStyles({
    row: {
        borderRight: '1px solid var(--text-color-secondary)',
    },
})

/**
 * Vertical selector, as seen on "Help" pages
 * @param {{selected: any; setSelected: any; options: {id: string; name: string; url?: string; disabled?: boolean; el: () => void}[]; default?: string; onChange?: (any) => void;}} props 
 * @returns 
 */
const HelpSelector = props => {
    const s = useStyles();
    const { options, selected, setSelected } = props;
    const router = useRouter();
    /*let hash = null;
    //const windowIsDefined = typeof window != "undefined";
    const hashtagSelected = options.find(v => v.id.toLowerCase() === hash?.toLowerCase());
    const useEffectRun = () => {
        hash = typeof window != undefined ? window.location.hash.substring(1) : null;
        setSelected(
            hashtagSelected
                ? hashtagSelected
                : props.default
                    ? options.find(v => v.id === props.default)
                    : options[0]
        );
    }
    const [selected, setSelected] = useState(
        hashtagSelected
            ? hashtagSelected
            : props.default
                ? options.find(v => v.id === props.default)
                : options[0]);
    useEffect(useEffectRun, [props.default, options]);
    useEffect(() => {
        window.addEventListener('hashchange', useEffectRun);
        return () => {
            window.removeEventListener('hashchange', useEffectRun);
        }
    }, [])*/

    const [hash, setHash] = useState(null);
    //const [selected, setSelected] = useState(options[0]);

    useEffect(() => {
        if (router.asPath.includes('#')) {
            const newHash = router.asPath.split('#')[1];
            setHash(newHash);
            const isHashValid = options.find(v => v.id.toLowerCase() === newHash?.toLowerCase());
            // if hash is an option, set selected to hash. if default is declared, then find
            // default option in options. else, just set selected to the first index in options
            setSelected(isHashValid ? isHashValid :
                //props.default ? options.find(v => v === props.default) :
                options[0]);
        }
    }, [
        router.asPath
        //props.default, 
    ]); // TODO: i wanna add functionality to where if you change hash it'll change what
    // option its on but i cant figure out how to do that without react bitching about window not being defined

    return <div className={'flex mt-4 pe-0'}>
        <div className='col-12 pe-0 me-0'>
            {
                options.map(v => {
                    return <SelectorOption key={v.id} onClick={e => {
                        e.preventDefault();
                        setSelected(v);
                        /*if (props.onChange)
                            props.onChange(v);*/
                        if (typeof window != "undefined")
                            window.location.hash = v.id;
                    }} name={v.name} url={v.url} selected={v.id === selected.id} disabled={v.disabled} />
                })
            }
        </div>
    </div>
}

export default HelpSelector;