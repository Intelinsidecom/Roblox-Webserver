import request, { getFullUrl } from "../lib/request";

/**
 * @param {number} badgeId
 * @returns {Promise<BadgeEntry>}
 */
export const getBadgeInfoByID = ({ badgeId }) => {
    return request('GET', getFullUrl('badges', `/v1/badges/${badgeId}`)).then(d => d.data);
}

/**
 * @param {number} badgeId
 * @returns {Promise<BadgeEntryBasic>}
 */
export const getBadgeInfoBasicByID = ({ badgeId }) => {
    return request('GET', getFullUrl('badges', `/v2/badges/${badgeId}/basic`)).then(d => d.data);
}

/**
 * @param {number} userId
 * @param {number|null} cursor
 * @param {number|null} limit
 * @returns {Promise<PekoraCollectionPaginated<BadgeEntry>>}
 */
export const getUserBadges = ({ userId, cursor = null, limit = null }) => {
    let url = `/v1/users/${userId}/badges`;
    if (cursor) {
        url += `?cursor=${cursor}`;
    }
    if (limit && cursor) {
        url += `&limit=${limit}`;
    } else if (limit && !cursor) {
        url += `?limit=${limit}`;
    }
    return request('GET', getFullUrl('badges', url)).then(d => d.data);
}

// might add this? badges api but without the expensive stuff
// /**
//  * @param {number} userId
//  * @param {number|null} cursor
//  * @param {number|null} limit
//  * @returns {Promise<PekoraCollectionPaginated<BadgeEntryBasic>>}
//  */
// export const getUserBadgesBasic = ({ userId, cursor = null, limit = null }) => {
//     let url = `/v1/users/${userId}/badges/basic`;
//     if (cursor) {
//         url += `?cursor=${cursor}`;
//     }
//     if (limit && cursor) {
//         url += `&limit=${limit}`;
//     } else if (limit && !cursor) {
//         url += `?limit=${limit}`;
//     }
//     return request('GET', getFullUrl('badges', url)).then(d => d.data);
// }

/**
 * @param {number} universeId
 * @param {number|null} cursor
 * @param {number|null} limit
 * @returns {Promise<PekoraCollectionPaginated<BadgeEntry>>}
 */
export const getUniverseBadges = ({ universeId, cursor = null, limit = null }) => {
    let url = `/v1/universes/${universeId}/badges`;
    if (cursor) {
        url += `?cursor=${cursor}`;
    }
    if (limit && cursor) {
        url += `&limit=${limit}`;
    } else if (limit && !cursor) {
        url += `?limit=${limit}`;
    }
    return request('GET', getFullUrl('badges', url)).then(d => d.data);
}

/**
 * @param {number} userId
 * @param {number[]} badgeIds
 * @returns {Promise<PekoraCollection<BadgeAwardedDate>>}
 */
export const getBadgeAwardedDates = ({ userId, badgeIds }) => {
    return request('GET', getFullUrl('badges', `/v1/users/${userId}/badges/awarded-dates?badgeIds=${encodeURIComponent(badgeIds.join(','))}`)).then(d => d.data);
}

/**
 * @param {number} badgeId
 * @returns {Promise}
 */
export const removeBadgeFromSelf = ({ badgeId }) => {
    return request(
        'DELETE',
        getFullUrl('badges', `/v1/users/badges/${badgeId}`)
    ).then(d => d.data);
}

/**
 * @param {number} badgeId
 * @param {boolean} enabled
 * @returns {Promise}
 */
export const updateBadge = ({ badgeId, enabled }) => {
    return request(
        'PATCH',
        getFullUrl('badges', `/v1/badges/${badgeId}`),
        {
            enabled
        }
    )
}


