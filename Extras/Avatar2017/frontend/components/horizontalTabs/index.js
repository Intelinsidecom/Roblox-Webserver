import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../lib/numberUtils";

const useStyles = createUseStyles({
  vTab: {
    flex: '1',
    display: 'inline-block',
    marginBottom: '-1px',
    float: 'left',
  },
  vTabLabel: {
    border: '0',
    backgroundColor: 'var(--white-color)',
    margin: 0,
    padding: '12px 2%',
    fontSize: '16px',
    fontWeight: '400',
    cursor: 'default',
    lineHeight: '1em',
    boxShadow: 'inset 0 -4px 0 0 var(--primary-color)',
    '&:hover': {
      background: '#f2f2f2'
    },
  },
  spanText: {
    display: 'inline-block',
    margin: 0,
    fontWeight: '500',
    lineHeight: '1em',
  },
  vTagSelected: {
  },
  buttonCol: {
    //borderBottom: '2px solid var(--text-color-quinary)',
    color: 'var(--text-color-primary)',
    backgroundColor: 'var(--white-color)',
    textAlign: 'center',
    width: '100%',
    padding: 0,
    border: 0,
    display: 'flex',
    flexDirection: 'row'
  },
  btnBottomSeperator: {
    width: '100%',
    height: '5px',
    background: 'white',
    marginBottom: '-5px',
  },
  vTabUnselected: {
    cursor: 'pointer',
    // 9e9e9e
    '&:not(:hover)': {
      boxShadow: 'none'
    }
  },
  count: {
    background: '#e0f1fc',
    border: '1px solid #84a5c9',
    paddingLeft: '4px',
    paddingRight: '4px',
  },
  selectedElement: {
    marginTop: '6px',
    marginBottom: '6px',
  }
});

/**
 * Vertical tabs in new style
 * @param {{options: {name: string; element: JSX.Element; onClick?: any; count?: number}[]; onChange?: (arg: {name: string; element: JSX.Element; count?: number;}) => void; default?: string; elementClass?: string;}} props
 */
const horizontalTabs = props => {
  const s = useStyles();
  const { options } = props;
  const hash = window.location.hash.substring(1);
  const hashtagSelected = options.find(v => v.name.toLowerCase() === hash.toLowerCase())
  const [selected, setSelected] = useState(
    hashtagSelected
      ? hashtagSelected
      : props.default
        ? options.find(v => v.name === props.default)
        : options[0]);
  useEffect(() => {
    setSelected(
      hashtagSelected
        ? hashtagSelected
        : props.default
          ? options.find(v => v.name === props.default)
          : options[0]
    );
  }, [props.default, options]);
  return <div>
    <div className={`${s.buttonCol} col-12`}>
      {
        options.map(v => {
          const isSelected = v.name === selected.name;
          return <div key={v.name} className={s.vTab} onClick={() => {
            setSelected(v);
            window.location.hash = v.name.toLowerCase()
            if (props.onChange) {
              props.onChange(v);
            }
            if (v.onClick) {
              v.onClick(v);
            }
          }}>
            <p className={`${!isSelected ? s.vTabUnselected : ''} ${s.vTabLabel}`}>
              <span className={s.spanText}>
                {v.name} {typeof v.count === 'number' ? <span className={s.count}>{abbreviateNumber(v.count)}</span> : null}
              </span>
            </p>
            {/*{isSelected && <div className={s.btnBottomSeperator}/>}*/}
          </div>
        })
      }
    </div>
    <div className={'col-12 ' + s.selectedElement + ' ' + props.elementClass}>
      {selected.element}
    </div>
  </div>
}

export default horizontalTabs;