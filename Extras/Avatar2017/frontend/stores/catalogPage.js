import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import getFlag from "../lib/getFlag";
import { getItemDetails, searchCatalog } from "../services/catalog";
import {useRouter} from "next/dist/client/router";

const stringToCategory = str => {
  // these are from catalog.roblox.com/v1/search/navigation-menu-items
  switch (str.toLowerCase().trim()) {
    case 'collectible':
    case 'collectibles':
      return 2;
    case 'featured':
      return 0;
    case 'accessories':
      return 11;
    case 'clothing':
      return 3;
    case 'gears':
    case 'gear':
      return 5;
    case 'bodyparts':
      return 4;
  }
  throw new Error('Invalid category "' + str + '"');
}

const stringToSubCategory = str => {
  // these are from catalog.roblox.com/v1/search/navigation-menu-items
  switch (str.toLowerCase().trim()) {
    case 'items':
    case 'hats':
      return 0; // todo: what do we put here?
    case 'all':
      return 0;
    case 'face':
    case 'faces':
      return 10;
    case 'packages':
      return 37; // todo: is this correct?
    case 'shirts':
      return 12;
    case 'tshirts':
      return 13;
    case 'pants':
      return 14;
    // gear categories
    case 'gear':
      return 0;
    case 'building':
      return 8;
    case 'explosive':
      return 3;
    case 'melee':
      return 1;
    case 'musical':
      return 6;
    case 'navigation':
      return 5;
    case 'powerup':
      return 4;
    case 'ranged':
      return 2;
    case 'social':
      return 7;
    case 'transport':
      return 9;
  }
  throw new Error('Invalid subcategory "' + str + '"');
}

export const CringeRobloxHats = [
  "Swaggy Bacon Hair Crown",
  "OMG So RARE Fedora",
  "Noob Slayer 9000 Helmet",
  "Rainbow Farting Unicorn Cap",
  "Yeet Master Beanie",
  "Drippy Dominus Wannabe",
  "Edgy Emo Skull Hood",
  "E-boy Cloud Goggles",
  "Trollface Top Hat",
  "Hot Cheeto Queen Tiara",
  "Obby King’s Ultimate Cap",
  "Rich Kid Flex Crown",
  "TikTok Clout Visor",
  "Slender's Signature Fedora",
  "Tryhard Gamer Headband",
  "Lava-Lamp-In-My-Brain Helmet",
  "Sussy Baka Among Us Hat",
  "Meme Lord Snapback",
  "Gamer Dorito Crown",
  "UwU Cat Ear Headphones",
  "Boomer Dad Baseball Cap",
  "LMAO LED Party Hat",
  "Epic Fail Propeller Cap",
  "Shrek’s Swamp Beanie",
  "Golden Toilet Plunger Hat",
  "Toxic Sludge Trucker Cap",
  "Anime Protagonist Bandana",
  "Giga Chad Fedora",
  "Cringe-core Rizzler Beanie",
  "Gamer Rage Quit Helmet",
  "Robux Billionaire Crown",
  "E-Girl Softboy Beret",
  "Buff Noob's Gym Cap",
  "Headless Horseman Discount Bucket",
  "Walmart Dominus",
  "AirPods Flex Snapback",
  "Kawaii Desu Pastel Cap",
  "Noob King’s Trash Can Lid",
  "Thug Life Pixel Shades Hat",
  "Cursed Bacon Hair Beret",
  "Shiny Absurdly Expensive Fedora",
  "Bling Bling Bubblegum Crown",
  "Obby Sweater Sweatband",
  "Sweaty Tryhard Bucket Hat",
  "Ice Cream Cone of Doom",
  "Emo Phase Starter Pack Cap",
  "V.I.P. Obby Completionist Hat",
  "My Mom Said I’m Cool Beanie",
  "Literally Just a Traffic Cone",
  "Inflatable Clown Hat of Regret"
];

const CatalogPageStore = createContainer(() => {
  const router = useRouter();
  const [query, setQuery] = useState(router.query.keyword || '');
  const [page, setPage] = useState(1);
  const [limit, setLimit] = useState(getFlag('catalogPageLimit', 28));
  const [category, setCategory] = useState('Featured');
  const [subCategory, setSubCategory] = useState('');
  const [locked, setLocked] = useState(false);
  const [results, setResults] = useState(null);
  const [total, setTotal] = useState(null);
  const [nextCursor, setNextCursor] = useState(null);
  const [previousCursor, setPreviousCursor] = useState(null);
  const [cursor, setCursor] = useState(null);
  const [sort, setSort] = useState(0);
  const [genres, setGenres] = useState([]);


  useEffect(() => {
    setLocked(true);
    let response = null;
    searchCatalog({
      category,
      subCategory,
      query,
      limit,
      cursor,
      sort,
    })
      .then(result => {
        response = result;
        if (response.data.length === 0) {
          return [];
        }
        return getItemDetails(result.data.map(v => v.id));
      })
      .then(assetDetails => {
        let arr = [];
        // do it this way to preserve sort
        for (const item of response.data) {
          let details = assetDetails.data.data.find(v => v.id === item.id);
          if (details) arr.push(details);
        }
        response.data = arr.filter(r => !CringeRobloxHats.includes(r.name));
        setResults(response);
        setNextCursor(response.nextPageCursor);
        setPreviousCursor(response.previousPageCursor);
        let total = response._total;
        setTotal(typeof total === 'number' ? total : null);
      })
      .finally(() => {
        setLocked(false);
      })
  }, [cursor, sort, category, subCategory, genres, query, limit]);

  const clearStatesForNewQuery = () => {
    setCursor(null);
    setPage(1);
  }

  return {
    locked,
    results,
    total,

    nextCursor,
    previousCursor,
    setCursor,

    sort,
    setSort,

    category,
    setCategory: (newCat) => {
      clearStatesForNewQuery();
      setCategory(newCat);
    },
    stringToCategory,

    subCategory,
    setSubCategory: (newSubCat) => {
      clearStatesForNewQuery();
      setSubCategory(newSubCat);
    },
    stringToSubCategory,

    genres,
    setGenres: (newGenres) => {
      clearStatesForNewQuery();
      setGenres(newGenres);
    },

    query,
    setQuery: (newQuery) => {
      clearStatesForNewQuery();
      setQuery(newQuery);
    },

    limit,
    setLimit: (newLimit) => {
      clearStatesForNewQuery();
      setLimit(newLimit);
    },

    page,
    setPage,
  }
});

export default CatalogPageStore;