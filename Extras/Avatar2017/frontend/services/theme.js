const themeType = {
    obc2016: 'obc2016',
    light: 'light',
    default: 'light',
}

const avPageStyleType = {
    Modern: 'Modern',
    Legacy: 'Legacy',
}

const catalogPageStyle = {
    Modern: 'Modern',
    Legacy: 'Legacy',
}

const isLocalStorageAvailable = (() => {
    // @ts-ignore
    if (!process.browser) return false;
    if (typeof window === 'undefined' || !window.localStorage || !window.localStorage.getItem || !window.localStorage.setItem) return false;
    
    return true;
})()

const getTheme = () => {
    if (!isLocalStorageAvailable) return themeType.default;
    
    let value = localStorage.getItem('rbx_theme_v1');
    // validate
    if (typeof value !== 'string' || !Object.getOwnPropertyNames(themeType).includes(value)) return themeType.default;
    return themeType[value];
}

const setTheme = (themeString) => {
    if (!isLocalStorageAvailable) return;
    localStorage.setItem('rbx_theme_v1', themeString)
}

const getAvPageStyle = () => {
    if (!isLocalStorageAvailable) return avPageStyleType.default;
    
    let value = localStorage.getItem('rbx_av_page_style_v1');
    // validate
    if (typeof value !== 'string' || !Object.getOwnPropertyNames(avPageStyleType).includes(value)) return avPageStyleType.default;
    return avPageStyleType[value];
}

const setAvPageStyle = (themeString) => {
    if (!isLocalStorageAvailable) return;
    localStorage.setItem('rbx_av_page_style_v1', themeString)
}

const getCatalogPageStyle = () => {
    if (!isLocalStorageAvailable) return catalogPageStyle["Modern"];
    
    let value = localStorage.getItem('rbx_cat_page_style_v1');
    // validate
    if (typeof value !== 'string' || !Object.getOwnPropertyNames(catalogPageStyle).includes(value)) return catalogPageStyle["Modern"];
    return catalogPageStyle[value];
}

const setCatalogPageStyle = (themeString) => {
    if (!isLocalStorageAvailable) return;
    localStorage.setItem('rbx_cat_page_style_v1', themeString);
}

export {
    getTheme,
    setTheme,
    
    getAvPageStyle,
    setAvPageStyle,
    
    getCatalogPageStyle,
    setCatalogPageStyle,
    
    themeType,
    avPageStyleType,
    catalogPageStyle,
}