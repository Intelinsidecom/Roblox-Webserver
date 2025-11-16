// reference: https://web.archive.org/web/20150228101148mp_/http://www.roblox.com/Work-at-a-Pizza-Place-place?id=192800
import React, { useEffect } from "react";
import { createUseStyles } from "react-jss";
import AdBanner from "../ad/adBanner";
import AdSkyscraper from "../ad/adSkyscraper";
import Comments from "./components/new/comments";

import HorizontalTabs from '../horizontalTabs';

import Recommendations from "./components/recommendations";
import NewGameOverview from "./components/newGameOverview";
import GameServers from "./components/new/newGameServers";
import Store from "./components/gameStore"
import GameAbout from './components/gameAbout'

import GameDetailsStore from "./stores/gameDetailsStore"

const useStyles = createUseStyles({
  gameContainer: {
    //backgroundColor: 'var(--white-color)',
    background: 'none',
    //padding: '4px 8px',
    //overflow: 'hidden',
    maxWidth: '970px',
    display: 'inline-block',
    position: 'relative',
    width: 'calc(100% - 165px)',
    '@media (max-width: 1500px)': {
      width: '100%',
    },
    //top: '-182px',
    //'@media (max-width: 1511px)': {
      top: 0,
   // },
  },
  horizontalTabs: {
    marginTop: '8px!important',
    marginBottom: '8px!important ',
  },
  container: {
    //padding: 0,
    maxWidth: '1154px!important',
    //flexDirection: 'column',
    //display: 'flex',
    '@media (max-width: 1511px)': {
      maxWidth: '970px!important',
    },
  },
  adSkyscraper: {
    display: 'inline-block!important',
    width: '160px',
    minHeight: '600px',
    '@media (max-width: 1511px)': {
      display: 'none!important'
    },
  },
  innerSkyscraper: {
    display: 'block',
    marginLeft: 0,
  },
  innerBanner: {
    margin: 0,
    //marginBottom: 'calc(-1 * var(--bs-gutter-y))'
    marginBottom: '10.5px'
  },
  containerAd: {
    display: 'flex',
    flexDirection: 'row',
  },
})

const GameDetails = props => {
  const { details } = props;
  const s = useStyles();
  const store = GameDetailsStore.useContainer();
  useEffect(() => {
    store.setDetails(details);
    store.setServers(null);
  }, [props]);

  if (!store.details) return null;
  return <div className={`container ${s.container}`}>
    <AdBanner className={s.innerBanner}></AdBanner>
    <div className={s.containerAd}>
      <div className={s.gameContainer}>
        {/*<div className='row mt-2'>*/}
        <div
        //className='mt-2'
        >
          <div className='col-12 col-lg-12'>
            <NewGameOverview></NewGameOverview>
            {/*<GameOverview></GameOverview>*/}
            <div className={'row mt-4 ' + s.horizontalTabs}>
              <div className={`'col-12'`}>
                <HorizontalTabs options={[
                  {
                    name: 'About',
                    element: <GameAbout></GameAbout>
                  },
                  {
                    name: 'Store',
                    element: <Store assetId={details.id}></Store>
                  },
                  {
                    name: 'Servers',
                    element: <GameServers></GameServers>
                  },
                  /*{
                    name: 'Comments',
                    element: <Comments assetId={details.id}></Comments>
                  },*/
                ]} />
                <Recommendations assetId={details.id}></Recommendations>
                <Comments assetId={details.id} />
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className={`${s.adSkyscraper} d-none d-lg-flex col-2`}>
        <AdSkyscraper className={s.innerSkyscraper}></AdSkyscraper>
      </div>
    </div>
  </div>
}

export default GameDetails;