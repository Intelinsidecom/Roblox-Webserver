import {createUseStyles} from "react-jss";
import {useEffect, useState} from "react";
import {getGameTemplates} from "../../../../services/games";

const useStyles = createUseStyles({
    subtitle: {},
    templatesContainer: {
        display: "flex",
        flexDirection: "row",
        flexWrap: "wrap",
    },
    templateContainer: {
        width: '25%',
        padding: '10px 15px',
        border: '1px solid transparent',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        '&:hover': {
            cursor: 'pointer',
            borderColor: '#AAA5A8',
        }
    },
    templateSelected: {
        borderColor: '#AAA5A8',
    },
    templateThumbContainer: { // TODO: container might not be necessary but idrc to test rn
        display: 'flex',
        width: '100%',
        height: '100%',
        '& img': {
            width: '100%',
            height: '100%',
            aspectRatio: '741 / 432',
            display: 'inline-block',
        }
    },
    templateName: {
        fontSize: 12,
        padding: 5,
        paddingBottom: 3,
        textAlign: 'center',
    },
})

const Templates = props => {
    const s = useStyles();
    // 0 = loading, 1 = failed, 2 = empty array, array = suiccess
    const [templates, setTemplates] = useState(0);
    
    useEffect(() => {
        if (typeof templates === 'number' && templates === 0) {
            setTemplates(0);
            getGameTemplates().then(d => {
                if (Array.isArray(d)) {
                    if (d.length <= 0) {
                        setTemplates(2)
                        return;
                    }
                    setTemplates(d);
                } else {
                    setTemplates(1);
                }
            })
        }
    }, []);
    
    return <div className='container'>
        <span className={s.subtitle}>GAME TEMPLATES</span>
        <div className={s.templatesContainer}>
            {templates === 0 ? <span>Loading Templates...</span> :
                templates === 2 ? <span>No Templates Found</span> : Array.isArray(templates) ?
                    templates.map(template =>
                        <div
                            className={`${s.templateContainer} ${props.template === template.universe.rootPlaceId ? s.templateSelected : undefined}`}
                            onClick={e => {
                                e.preventDefault();
                                props.setTemplate(template.universe.rootPlaceId);
                            }}>
                            <div className={s.templateThumbContainer}>
                                <img src={`/thumbs/asset.ashx?assetId=${template.universe.rootPlaceId}`}/>
                            </div>
                            <span className={s.templateName}>{template.universe.name}</span>
                        </div>
                    )
                    :
                    <span>Couldn't load templates, try again later.</span>
            }
        </div>
    </div>
}

export default Templates;