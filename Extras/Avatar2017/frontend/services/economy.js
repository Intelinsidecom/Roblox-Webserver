import request from "../lib/request"
import { getFullUrl } from "../lib/request";


export const getResellers = ({ assetId, cursor, limit }) => {
  const url = getFullUrl('economy', '/v1/assets/' + assetId + '/resellers?limit=' + limit + '&cursor=' + encodeURIComponent(cursor || ''));
  return request('GET', url);
}

export const getRobux = ({ userId }) => {
  return request('GET', getFullUrl('economy', '/v1/users/' + userId + '/currency')).then(d => d.data);
}

export const getRobuxGroup = ({ groupId }) => {
  return request('GET', getFullUrl('economy', '/v1/groups/' + groupId + '/currency')).then(d => d.data);
}

export const getResellableCopies = ({ assetId, userId }) => {
  return request('GET', getFullUrl('economy', `/v1/assets/${assetId}/users/${userId}/resellable-copies`)).then(d => d.data);
}

/**
 * Represents a purchase detail request model.
 * @typedef {Object} PurchaseDetailRequestModel
 * @property {boolean} purchased - Defines if the product was purchased.
 * @property {string} reason - Defines the reason for failure.
 * @property {number} productId - Reflects the requested product id for reference.
 * @property {number} statusCode - Represents the status code of the request.
 * @property {string} title - The title associated with the request.
 * @property {string} errorMsg - Error message, if applicable.
 * @property {string} showDivId - The div ID to show.
 * @property {number} shortfallPrice - The shortfall price if the purchase was unsuccessful.
 * @property {number} balanceAfterSale - The balance after the sale.
 * @property {number} expectedPrice - The expected price of the product.
 * @property {number} currency - The currency type identifier.
 * @property {number} price - The price of the asset.
 * @property {number} assetId - The asset identifier.
 * @property {string} assetName - The name of the asset.
 * @property {string} assetType - The type of the asset.
 * @property {string} assetTypeDisplayName - The display name of the asset type.
 * @property {boolean} assetIsWearable - Indicates if the asset is wearable.
 * @property {string} sellerName - The name of the seller.
 * @property {string} transactionVerb - The verb describing the transaction type.
 * @property {boolean} isMultiPrivateSale - Indicates if it is a multi-private sale.
 * @property {PremiumPricingModel} premiumPricing - Premium pricing details.
 */

/**
 * Represents the premium pricing for a product.
 * @typedef {Object} PremiumPricingModel
 * @property {number} premiumDiscountPercentage - The Premium discount percentage for a product.
 * @property {number} premiumPriceInRobux - The Premium price for a product.
 */

/**
 * @param {number} productId
 * @param {number} assetId
 * @param {number} sellerId
 * @param {number} userAssetId
 * @param {number} price
 * @param {number} expectedCurrency
 * @returns {Promise<PurchaseDetailRequestModel>}
 */
export const purchaseItem = ({ productId, assetId, sellerId, userAssetId, price, expectedCurrency }) => {
  return request('POST', getFullUrl('economy', `/v1/purchases/products/${productId}`), {
    assetId,
    expectedPrice: price,
    expectedSellerId: sellerId,
    userAssetId,
    expectedCurrency,
  }).then(d => d.data);
}

export const setResellableAssetPrice = ({ assetId, userAssetId, price }) => {
  if (!Number.isSafeInteger(price) || price < 0 || isNaN(price)) {
    throw new Error('Invalid Price "' + price + '"');
  }
  return request('PATCH', getFullUrl('economy', `/v1/assets/${assetId}/resellable-copies/${userAssetId}`), {
    price,
  }).then(d => d.data);
}

export const takeResellableAssetOffSale = ({ assetId, userAssetId }) => {
  return setResellableAssetPrice({
    assetId,
    userAssetId,
    price: 0,
  });
}

export const getResaleData = ({ assetId }) => {
  return request('GET', getFullUrl('economy', `/v1/assets/${assetId}/resale-data`)).then(d => d.data);
}

export const getTransactions = ({ userId, cursor, type }) => {
  return request('GET', getFullUrl('economy', `/v2/users/${userId}/transactions?cursor=${encodeURIComponent(cursor || '')}&transactionType=${encodeURIComponent(type)}`)).then(d => d.data);
}

export const getGroupTransactions = ({ groupId, cursor, type }) => {
  return request('GET', getFullUrl('economy', `/v2/groups/${groupId}/transactions?cursor=${encodeURIComponent(cursor || '')}&transactionType=${encodeURIComponent(type)}`)).then(d => d.data);
}

export const getTransactionSummary = ({ userId, timePeriod }) => {
  return request('GET', getFullUrl('economy', `/v2/users/${userId}/transaction-totals?timeFrame=${timePeriod}&transactionType=summary`)).then(d => d.data);
}

export const getGroupTransactionSummary = ({ groupId, timePeriod }) => {
  return request('GET', getFullUrl('economy', `/v2/groups/${groupId}/transaction-totals?timeFrame=${timePeriod}&transactionType=summary`)).then(d => d.data);
}


const fNum = (num) => {
  if (!num) return '';
  return num.toLocaleString();
}

export const formatSummaryResponse = (resp, type = 'User') => {
  const isUser = type === 1 || type === 'User';

  const result = [
    isUser && [
      'Builders Club Stipend',
      fNum(resp.premiumStipendsTotal),
    ],
    isUser && [
      'Builders Club Stipend Bonus',
      '',
    ],
    [
      'Sale of Goods',
      fNum(resp.salesTotal),
    ],
    isUser && [
      'Currency Purchase',
      fNum(resp.currencyPurchasesTotal),
    ],
    isUser && [
      'Trade System Trades',
      fNum(resp.tradeSystemEarningsTotal),
    ],
    [
      'Promoted Page Conversion Revenue',
      '',
    ],
    [
      'Game Page Conversion Revenue',
      '',
    ],
    [
      'Pending Sales',
      fNum(resp.pendingRobuxTotal),
    ],
    isUser && [
      'Group Payouts',
      fNum(resp.groupPayoutsTotal),
    ],
  ].filter(v => !!v);
  return result;
};

// Extension - these don't exist on real roblox

export const getMarketActivity = () => {
  return request('GET', getFullUrl('economy', '/v2/currency-exchange/market/activity')).then(d => d.data);
}

export const createCurrencyExchangeOrder = async ({
  currency,
  amount,
  isMarketOrder,
                                                    desiredRate,
}) => {
  return request('POST', getFullUrl('economy', '/v2/currency-exchange/orders/create'), {
    amount,
    sourceCurrency: currency,
    isMarketOrder,
    desiredRate,
  })
}

/**
 * Get open position count for authenticated user
 * @returns {Promise<number>}
 */
export const countOpenPositions = async ({currency}) => {
  return request('GET', getFullUrl('economy', '/v2/currency-exchange/orders/my/count?currency=' + currency)).then(d => d.data.total);
}

export const getOpenPositions = async ({startId, limit,currency}) => {
  return request('GET', getFullUrl('economy', '/v2/currency-exchange/orders/my?limit=' + limit + '&startId=' + startId + '&currency=' + currency)).then(d => d.data);
}

export const closePosition = async({orderId}) => {
  return request('POST', getFullUrl('economy', '/v2/currency-exchange/orders/' + orderId + '/close'));
}