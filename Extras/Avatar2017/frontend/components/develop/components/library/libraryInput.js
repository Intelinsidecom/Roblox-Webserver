import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../../../stores/libraryPage";
import buttonStyles from '../../../../styles/buttons.module.css';
import { getQueryParams } from "../../../../lib/getQueryParams";

const useInputStyles = createUseStyles({
  input: {
    width: '100%',
    padding: '2px',
    border: '1px solid #a7a7a7',
  },
  select: {
    width: '100%',
    padding: '2px',
    border: '1px solid #a7a7a7',
    paddingLeft: '2px',
    appearance: 'none',
  },
  col: {
    padding: 0,
    paddingLeft: '2px',
  },
  caret: {
    position: 'absolute',
    marginLeft: '-21px',
    fontSize: '8px',
    transform: 'rotate(90deg)',
    marginTop: '6px',
    border: '1px solid black',
    paddingLeft: '3px',
    paddingRight: '1px',
    background: 'linear-gradient(90deg, rgba(224,224,224,1) 0%, rgba(255,255,255,1) 100%)',
  },
  button: {
    fontSize: '14px',
    fontWeight: '600',
    height: '100%',
    width: 'auto',
    padding: '1px 8px',
  },
  buttonContainer: {
    width: 'auto'
  },
})

/**
 * Catalog page search input
 * @param {*} props 
 */
const CatalogPageInput = props => {
  const s = useInputStyles();
  const store = CatalogPageStore.useContainer();
  const query = getQueryParams();
  const input = useRef(null);
  const [category, setCategory] = useState("Audio");

  useEffect(() => {
    store.setCategory(category)
  }, [category])

  useEffect(() => {
    // for keyword updates from url param
    if (query.keyword)
      store.setQuery(query.keyword)
    if (input.current)
      input.current.value = store.query;
  }, [store.query, query.keyword]);

  return <div className='row' style={{ marginBottom: '50px' }}>
    <div className='col-12 col-lg-8 offset-lg-4'>
      <div className='row' style={{ flexDirection: 'row-reverse', marginRight: 0 }}>
        {// in reverse order
        }
        <div className={`${s.col} ${s.buttonContainer}`}>
          <button disabled={store.locked} className={`${buttonStyles.legacyButton} ${s.button}`} onClick={(e) => {
            e.preventDefault();
            store.setQuery(input.current.value);
          }}>Search</button>
        </div>
        <div className={`col-6 col-md-3 ${s.col}`}>
          <select disabled={store.locked} className={s.select} value={category}
          onChange={e => {setCategory(e.target.value)}}>
            <option value='Models'>Models</option>
            <option value='Audio'>Audio</option>
            <option value='Videos'>Videos</option>
            <option value='Decals'>Decals</option>
            <option value='Meshes'>Meshes</option>
            <option value='Plugins'>Plugins</option>
          </select>
          <span className={s.caret}>►</span>
        </div>
        <div className={`col-12 col-md-6 ${s.col}`}>
          <input type='text' className={`${s.input}`} ref={input} />
        </div>
      </div>
    </div>
  </div>
}

export default CatalogPageInput;