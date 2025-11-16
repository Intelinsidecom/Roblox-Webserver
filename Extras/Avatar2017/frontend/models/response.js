/**
 * @template T
 * @typedef {Object} PekoraCollectionPaginated
 * @property {string|null} nextPageCursor
 * @property {string|null} previousPageCursor
 * @property {T[]} data
 */

/**
 * @template T
 * @typedef {Object} PekoraCollection
 * @property {T[]} data
 */

/**
 * @typedef {Object} AssetType
 * @property {number} id
 * @property {string} name
 */

/**
 * @typedef {Object} Asset
 * @property {number} id
 * @property {string} name
 * @property {string} type
 * @property {AssetType} assetType
 */

/**
 * @typedef DataInv
 * @property {number} TotalItems
 * @property {string} nextPageCursor
 * @property {string} previousPageCursor
 * @property {number} ItemsPerPage
 * @property {DataInvItem[]} Items
 */

/**
 * @typedef DataInvItem
 * @property {Item} Item
 */

/**
 * @typedef Item
 * @property {number} AssetId
 * @property {string} Name
 * @property {number} AssetType
 */
