import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import LibraryStore from "../../../../stores/libraryPage";
import LibraryCard from "./libraryCard";
import LibraryPagination from "./libraryPagination";
import OldSelect from "../../../oldSelect";

const useResultStyles = createUseStyles({
  pageTitle: {
    fontSize: '28px',
    fontWeight: 500,
  },
  pageTitleAlt: {
    fontSize: '14px',
    fontWeight: 700,
    opacity: 1,
    letterSpacing: -0.2,
  },
  subtitleText: {
    fontSize: '12px',
    fontWeight: 500,
  },
  selectWrapper: {
    width: '200px',
    display: 'inline-block',
  },
  sortWrapper: {
    float: 'right',
  },
  sortByLabel: {
    paddingRight: '4px',
    fontWeight: 600,
    color: 'var(--text-color-primary)',
  },
  sortByLabelWrapper: {
    display: 'inline-block',
  },
});

const CatalogPageResults = props => {
  const s = useResultStyles();
  const store = LibraryStore.useContainer();
  const [isQueried, setQueried] = useState(false);

  useEffect(() => {
    if (store.query && store.query !== '') {
      setQueried(true);
    } else {
      setQueried(false);
    }
  }, [store.query])

  const GetTitle = () => {
    const currentOffset = Math.trunc(store.page / store.limit + 1) || 0;
    const limit = store.limit < store.results.data.length ? store.limit : store.results.data.length;

    const moreAvailable = store.nextCursor !== null;

    return <>
      <h3 className={s.pageTitleAlt}>{store.category.toUpperCase() + (store.query && store.query !== '' ? ' » ' + store.query : '') || 'ERROR'}</h3>
      <p className={s.subtitleText}>Showing {store.results.data.length === 0 ? '0' : currentOffset.toLocaleString()} {moreAvailable ? '- ' + limit.toLocaleString() : ''} of {(typeof store.total === 'number' ? store.total : 'many').toLocaleString()} results</p>
    </>
  }

  if (!store.results)
    return null;

  return <div className='row' style={store.locked ? { opacity: '0.25' } : undefined}>
    <div className='col-6'>
      <GetTitle />
    </div>
    <div className='col-6'>
      <div className={s.sortWrapper}>
        <div className={s.sortByLabelWrapper}>
          <span className={s.sortByLabel}>Sort By: </span>
        </div>
        <div className={s.selectWrapper}>
          <OldSelect onChange={(newSort) => {
            store.setSort(parseInt(newSort, 10));
          }} options={[
            {
              key: '0',
              value: 'Relevance',
            },
            {
              key: '3',
              value: 'Recently updated',
            },
            {
              key: '4',
              value: 'Price (Low to High)',
            },
            {
              key: '5',
              value: 'Price (High to Low)',
            },
          ]} disabled={store.locked}></OldSelect>
        </div>
      </div>
    </div>
    <div className='col-12'>
      <div className='row'>
        {
          store.results ?
            store.results.data.map((v, i) => {
              return <LibraryCard key={v.id} {...v}></LibraryCard>
            })
            : null
        }
      </div>
      <LibraryPagination />
    </div>
  </div>
}

export default CatalogPageResults;
