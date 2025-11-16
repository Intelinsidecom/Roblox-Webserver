import request, { getBaseUrl, getFullUrl } from "../lib/request"
import getFlag from "../lib/getFlag";

export const itemNameToEncodedName = (str) => {
    if (typeof str !== 'string') {
        str = '';
    }
    // https://stackoverflow.com/questions/987105/asp-net-mvc-routing-vs-reserved-filenames-in-windows
    var seoName = str.replace(/'/g, "")
        .replace(/[^a-zA-Z0-9]+/g, "-")
        .replace(/^-+|-+$/g, "")
        .replace(/^(COM\d|LPT\d|AUX|PRT|NUL|CON|BIN)$/i, "") || "unnamed";
    return seoName;
}

const itemPageLate2016Enabled = getFlag('itemPageLate2016Enabled', false);
const csrEnabled = getFlag('clientSideRenderingEnabled', false);

export const getItemUrl = ({ assetId, name }) => {
    return `/catalog/${assetId}/${itemNameToEncodedName(name)}`;
}

export const searchCatalog = ({
                                  category,
                                  subCategory = null,
                                  query = null,
                                  limit,
                                  cursor = null,
                                  sort,
                                  creatorType = null,
                                  creatorId = null,
                                  creatorName = null,
                                  includeNotForSale = null
                              }) => {
    let url = '/v1/search/items?category=' + category + '&limit=' + limit + '&sortType=' + sort;
    if (cursor) {
        url += '&cursor=' + encodeURIComponent(cursor);
    }
    if (query) {
        url += '&keyword=' + encodeURIComponent(query);
    }
    if (subCategory) {
        url += '&subcategory=' + encodeURIComponent(subCategory);
    }
    if (creatorType && creatorId) {
        url += '&creatorTargetId=' + creatorId + '&creatorType=' + creatorType;
    }
    if (!creatorId && creatorName) {
        url += '&creatorName=' + creatorName + '&creatorType=1';
    }
    if (includeNotForSale === true) {
        url += '&includeNotForSale=true';
    }
    return request('GET', getFullUrl('catalog', url)).then(d => d.data);
}

/**
 * Only use this on server-side requests.
 * @param {number} assetId
 * @returns ProductInfoLegacy
 */
export const getProductInfoLegacy = async (assetId) => {
    return request('GET', getFullUrl('api', '/marketplace/productinfo?assetId=' + assetId)).then(d => d.data);
}

/**
 * @typedef {Object} ProductInfoLegacy
 * @property {number} TargetId - Unique identifier for the target of the product (typically the game or asset).
 * @property {string} ProductType - The type of product, e.g., "Game Pass".
 * @property {number} AssetId - ID of the associated asset, if any (0 if not applicable).
 * @property {number} ProductId - Unique identifier for the product.
 * @property {string} Name - Name of the game pass.
 * @property {string} Description - Description of the game pass.
 * @property {number} AssetTypeId - The type of asset (0 if not applicable).
 * @property {Object} Creator - Information about the creator of the game pass.
 * @property {number} Creator.Id - ID of the creator.
 * @property {string} Creator.Name - Username of the creator.
 * @property {string} Creator.CreatorType - Type of creator (e.g., "User").
 * @property {number} Creator.CreatorTargetId - ID used for referencing the creator.
 * @property {number} IconImageAssetId - ID of the icon image used for the game pass.
 * @property {string} Created - ISO 8601 timestamp of when the game pass was created.
 * @property {string} Updated - ISO 8601 timestamp of when the game pass was last updated.
 * @property {number} PriceInRobux - Price of the game pass in Robux.
 * @property {?number} PriceInTickets - Deprecated. Always null (Roblox tickets no longer exist).
 * @property {number} Sales - Number of times the game pass has been sold.
 * @property {boolean} IsNew - Indicates if the game pass is newly created.
 * @property {boolean} IsForSale - Whether the game pass is currently for sale.
 * @property {boolean} IsPublicDomain - Whether the asset is public domain (usually false).
 * @property {boolean} IsLimited - Whether the item is limited.
 * @property {boolean} IsLimitedUnique - Whether the item is a limited unique.
 * @property {number} Remaining - Number of items remaining (usually 0 for non-limiteds).
 * @property {number} MinimumMembershipLevel - Minimum membership level required to buy (0 = none).
 * @property {number} ContentRatingTypeId - Content rating (0 = all audiences).
 */

export const getItemDetails = async (assetIdArray) => {
    if (assetIdArray.length === 0) return { data: { data: [] } }
    while (true) {
        try {
            const res = await request('POST', getFullUrl('catalog', '/v1/catalog/items/details'), {
                items: assetIdArray.map(v => {
                    return {
                        itemType: 'Asset',
                        id: v,
                    }
                })
            });
            for (const item of res.data.data) {
                if (typeof item.isForSale === 'undefined') {
                    item.isForSale = (item.unitsAvailableForConsumption !== 0 && typeof item.price === 'number' && typeof item.lowestPrice === 'undefined');
                }
            }
            return res;
        } catch (e) {
            // @ts-ignore
            if (e.response && e.response.status === 429 && process.browser) {
                await new Promise((res) => setTimeout(res, 2500));
                continue;
            }
            throw e;
        }
    }
}

/**
 * @param {number[]} assetIdArray
 * @returns {Promise<PekoraCollection<AssetDetailsEntry>>}
 */
export const getItemDetailsNew = async (assetIdArray) => {
    if (assetIdArray.length === 0) return Promise.resolve({ data: [] });
    
    try {
        /** @type RawAssetDetails[] */
        const response = (await request("POST", getFullUrl("catalog", "/v1/catalog/items/details"), {
            items: assetIdArray.map(v => {
                return {
                    itemType: 'Asset',
                    id: v,
                }
            })
        })).data.data;
        
        for (const item of response) {
            if (typeof item.isForSale === 'undefined') {
                item.isForSale = (item.unitsAvailableForConsumption !== 0 && typeof item.price === 'number' && typeof item.lowestPrice === 'undefined');
            }
        }
        
        return response;
    } catch (e) {
        // @ts-ignore
        if (e.response && e.response.status === 429 && process.browser) {
            await new Promise((res) => setTimeout(res, 2500));
            return getItemDetailsNew(assetIdArray);
        }
        throw e;
    }
}

/**
 * @param {number} assetId
 * @param {number} assetTypeId
 * @param {number} limit
 * @returns {Promise<PekoraCollection<RecommendedItemEntry>>}
 */
export const getRecommendations = ({ assetId, assetTypeId, limit }) => {
    return request('GET', getFullUrl('catalog', '/v1/recommendations/asset/' + assetTypeId + '?contextAssetId=' + assetId + '&numItems=' + limit)).then(d => d.data);
}

export const getComments = async ({ assetId, offset }) => {
    return request('GET', getBaseUrl() + '/comments/get-json?assetId=' + assetId + '&startIndex=' + offset + '&thumbnailWidth=100&thumbnailHeight=100&thumbnailFormat=PNG&cachebuster=' + Math.random()).then(d => d.data);
}

export const createComment = async ({ assetId, comment }) => {
    let result = await request('POST', getBaseUrl() + '/comments/post', {
        text: comment,
        assetId: assetId,
    });
    if (typeof result.data.ErrorCode === 'string') {
        throw new Error(result.data.ErrorCode);
    }
    return result.data;
}

export const addOrRemoveFromCollections = ({ assetId, addToProfile }) => {
    return request('POST', getBaseUrl() + '/asset/toggle-profile', {
        assetId,
        addToProfile,
    })
}

export const getIsFavorited = async ({ assetId, userId }) => {
    return await request('GET', getFullUrl('catalog', '/v1/favorites/users/' + userId + '/assets/' + assetId + '/favorite')).then(d => d.data);
}

export const createFavorite = async ({ assetId, userId }) => {
    return await request('POST', getFullUrl('catalog', '/v1/favorites/users/' + userId + '/assets/' + assetId + '/favorite'));
}

export const deleteFavorite = async ({ assetId, userId }) => {
    return await request('DELETE', getFullUrl('catalog', '/v1/favorites/users/' + userId + '/assets/' + assetId + '/favorite'));
}

export const getAudio = async ({ audioId }) => {
    return await request('GET', `${getBaseUrl()}/asset/?id=${audioId}`).then(d => d.data);
}

export const getAudioURL = async ({ audioId }) => {
    return `${getBaseUrl()}/asset/?id=${audioId}`;
}

export const getModerationStatus = ({ assetID }) => {
    return request('GET', `${getBaseUrl()}/asset/status?assetId=${assetID}`).then(d => d?.data?.moderationStatus);
}

/**
 * @typedef {Object} RawAssetDetails
 * @property {number} id - Unique identifier for the item.
 * @property {number} assetType - Type ID of the asset.
 * @property {string} name - Name of the item.
 * @property {string} description - Description of the item.
 * @property {string[]} genres - Array of genres the item belongs to.
 * @property {"User" | "Group"} creatorType - Type of creator.
 * @property {number} creatorTargetId - ID of the user or group who created the item.
 * @property {string} creatorName - Name of the creator.
 * @property {string|null} offsaleDeadline - When the item will go off sale, if applicable.
 * @property {("Limited" | "LimitedUnique")[]} itemRestrictions - Any special item restrictions.
 * @property {number} saleCount - Total number of sales.
 * @property {"Asset" | string} itemType - Type of the item.
 * @property {number} favoriteCount - Number of times the item was favorited.
 * @property {boolean} isForSale - Whether the item is currently for sale.
 * @property {boolean} commentsEnabled - Whether comments are enabled.
 * @property {number|null} price - Original sale price in Robux.
 * @property {number|null} priceTickets - Original sale price in Tickets (legacy).
 * @property {number|null} lowestPrice - Current lowest resale price.
 * @property {null|string} priceStatus - Status of the price, if applicable.
 * @property {Object|null} lowestSellerData - Information about the current lowest seller.
 * @property {number} lowestSellerData.userId - ID of the seller.
 * @property {string} lowestSellerData.username - Username of the seller.
 * @property {number} lowestSellerData.userAssetId - ID of the specific user-owned asset.
 * @property {number} lowestSellerData.price - Resale price set by the seller.
 * @property {number} lowestSellerData.assetId - ID of the asset being sold.
 * @property {number|null} unitsAvailableForConsumption - Units available (for items like gear or use-once assets).
 * @property {number} serialCount - Total number of serialized copies.
 * @property {boolean} is18Plus - Whether the item is age-restricted.
 * @property {"ReviewApproved" | string} moderationStatus - Moderation status of the item.
 * @property {string} createdAt - ISO timestamp of when the item was created.
 * @property {string} updatedAt - ISO timestamp of the last update.
 */
