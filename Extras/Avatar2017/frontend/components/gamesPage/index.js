import React, { useEffect } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../lib/getFlag";
import AuthenticationStore from "../../stores/authentication";
import GamesPageStore from "../../stores/gamesPage";
import AdBanner from "../ad/adBanner";
import Selector from "../selector";
import { getQueryParams } from "../../lib/getQueryParams";
import GamesList from './components/games';

const useStyles = createUseStyles({
  authContainer: {
    '@media(min-width: 1300px)': {
      marginLeft: '180px',
    },
    '@media(max-width: 994px)': {
      margin: 0,
      padding: 0,
    },
  },
  selectorSort: {
    width: '200px',
    float: 'left',
    '@media(max-width: 994px)': {
      width: '100%',
      paddingRight: 'calc(.5* var(--bs-gutter-x))',
      marginLeft: '6px!important'
    },
  },
  gamesContainer: {
    backgroundColor: 'var(--background-color)',
    paddingTop: '8px',
    marginLeft: '15px',
    marginRight: '15px',
    '@media(max-width: 994px)': {
      paddingTop: '0',
    },
  },

  selectorContainer: {
    '@media(max-width: 994px)': {
      marginBottom: '6px',
    },
  },

  container: {
    paddingRight: 0,
  },
})

const Games = props => {
  const query = getQueryParams();
  const store = GamesPageStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const s = useStyles();
  const showGenre = getFlag('gameGenreFilterSupported', false) && !query.sortFilter && !query.keyword;
  const showSortDropdown = getFlag('gameCustomSortDropdown', false) && !query.sortFilter && !query.keyword;
  const selectorContainerClass = showSortDropdown ? s.selectorContainer : '';

  useEffect(() => {
    //store.setQuery(null);
    //store.setSortToken(null);
    if (query.keyword)
      store.setQuery(query.keyword);
    if (query.sortFilter)
      store.setSortToken(query.sortFilter);

    store.loadGames({
      // @ts-ignore
      query: query.keyword,
      genreFilter: store.genreFilter,
      // @ts-ignore
      sortToken: query.sortFilter,
    });
  }, [store.genreFilter, query.sortFilter, query.keyword]);

  // if (!store.sorts || !store.games || !store.icons) return null;
  return <div className={'row ' + (auth.isAuthenticated ? s.authContainer : '')}>
    {/*<div className='col-12'>
      <AdBanner context='gamesPage' />
    </div>*/}
    <div className={`col-12 ps-0 pb-0 ${s.container}`}>
      <div className={'row pb-2 ' + s.gamesContainer}>
        <div className={`${selectorContainerClass} col-12`}>
          {showSortDropdown &&
            <div className={`row ${s.selectorSort}`}>
              <Selector
                onChange={(newValue) => {
                  // TODO
                  console.log('[info] use sort', newValue);
                }}
                options={[
                  {
                    name: 'Default',
                    value: 'default',
                  },
                  {
                    name: 'Popular',
                    value: 'popular',
                  },
                  {
                    name: 'Top Earning',
                    value: 'top-earning',
                  },
                  {
                    name: 'Top Rated',
                    value: 'top-rated',
                  },
                  {
                    name: 'Recommended',
                    value: 'recommended',
                  },
                  {
                    name: 'Top FavouriteButton',
                    value: 'top-favorite',
                  },
                  {
                    name: 'Top Paid',
                    value: 'top-paid',
                  },
                  {
                    name: 'Builders Club',
                    value: 'builders-club',
                  },
                ]} />
            </div>
          }
          {showGenre &&
            <div className={s.selectorSort + ' ms-2'}>
              <Selector
                onChange={(newValue) => {
                  // TODO
                  console.log('[info] use genre', newValue);
                  store.setGenreFilter(newValue.value);
                }}
                options={store.selectorSorts} />
            </div>
          }
        </div>
        <div className='col-12'>
          <GamesList />
        </div>
      </div>
    </div>
  </div>
}

export default Games;