import React from "react";
import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../../../stores/libraryPage";
import Dropdown from "../../../dropdown";

const navigationItems = [
  {
    name: 'Models',
    clickData: 'Models,',
  },
  {
    name: 'Audio',
    clickData: 'Audio,',
  },
  {
    name: 'Videos',
    clickData: 'Videos,',
  },
  {
    name: 'Decals',
    clickData: 'Decals,',
  },
  {
    name: 'Meshes',
    clickData: 'Meshes,',
  },
  {
    name: 'Plugins',
    clickData: 'Plugins,',
  },
];

const useTitleStyles = createUseStyles({
  top: {
    fontSize: '12px',
    color: 'white',
    fontWeight: 600,
    lineHeight: 'normal',
    marginBottom: '-5px',
    paddingLeft: '4px',
    textShadow: '1px 1px 1px black',
  },
  bottom: {
    fontSize: '20px',
    color: 'white',
    fontWeight: 700,
    lineHeight: 'normal',
    paddingLeft: '4px',
    textShadow: '1px 1px 1px black',
  },
  text: {
    lineHeight: '1.65em'
  },
})
const CatalogPageNavigation = () => {
  const store = CatalogPageStore.useContainer();
  const title = useTitleStyles();
  return <>
    <Dropdown
      onClick={(e, clickData) => {
        e.preventDefault();
        console.log('[info] library dropdown clicked with', clickData);
        const [category, subCategory] = clickData.split(',');
        if (category === '') {
          console.log('[info] bad category');
          return;
        }
        store.setCategory(category);
        store.setSubCategory(subCategory);
      }}
      items={navigationItems} selected={store.category} textClass={title.text} />
  </>
}

export default CatalogPageNavigation;