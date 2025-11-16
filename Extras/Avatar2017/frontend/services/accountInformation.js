import request, { getBaseUrl, getFullUrl } from "../lib/request"

export const getUserRobloxBadges = ({ userId }) => {
  return request('GET', getFullUrl('accountinformation', `/v1/users/${userId}/roblox-badges`)).then(d => d.data)
}

export const setUserDescription = ({ newDescription }) => {
  return request('POST', getFullUrl('accountinformation', `/v1/description`), {
    description: newDescription,
  })
}

/**
 * @param {number} userId
 * @param {boolean?} returnUrls
 * @returns {Promise<UserConnection>}
 */
export const getUserConnections = ({ userId, returnUrls = false }) => {
  return request('GET', getFullUrl('accountinformation', `/v1/users/${userId}/promotion-channels?alwaysReturnUrls=${returnUrls}`)).then(d => d.data)
}

/**
 * @param {UserConnection} connections
 * @returns {Promise<void>}
 */
export const setUserConnections = ({ connections }) => {
  return request('POST', getFullUrl('accountinformation', `/v1/promotion-channels`), connections);
}

/**
 * @typedef UserConnection
 * @property {string?} twitter
 * @property {string?} discord
 * @property {string?} telegram
 * @property {string?} tiktok
 * @property {string?} youtube
 * @property {string?} twitch
 * @property {string?} github
 * @property {string?} roblox
 */
