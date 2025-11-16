/**
 * @typedef PlaceDetails
 * @property {string} builder
 * @property {number} builderId
 * @property {string} builderType
 * @property {string} created
 * @property {string|null} description
 * @property {string} genre
 * @property {string} imageToken
 * @property {boolean} isPlayable
 * @property {number} maxPlayerCount
 * @property {string} name
 * @property {number} placeId
 * @property {number|null} price
 * @property {string} reasonProhibited
 * @property {number} universeId
 * @property {number} universeRootPlaceId
 * @property {string} updated
 * @property {number} year
 */

/**
 * @typedef UniverseDetails
 * @property {boolean} createVipServersAllowed
 * @property {string} created
 * @property {Object} creator
 * @property {number} creator.id
 * @property {string} creator.name
 * @property {string} creator.type
 * @property {boolean} creator.hasVerifiedBadge
 * @property {string|null} description
 * @property {number} favoritedCount
 * @property {string} genre
 * @property {number} id
 * @property {boolean} isAllGenre
 * @property {boolean} isFavoritedByUser
 * @property {boolean} isGenreEnforced
 * @property {boolean} isPublic
 * @property {number} maxPlayers
 * @property {string} name
 * @property {number} playing
 * @property {number|null} price
 * @property {number} rootPlaceId
 * @property {string|null} sourceDescription
 * @property {string} sourceName
 * @property {boolean} studioAccessToApisAllowed
 * @property {string} universeAvatarType
 * @property {string} updated
 * @property {number} visits
 */

/**
 * @typedef AssetThumbnail
 * @property {string|null} imageUrl
 * @property {string} state
 * @property {number} targetId
 * @property {string} version
 */

/**
 * @typedef GamepassEntry
 * @property {number} id
 * @property {string} name
 * @property {number} productId
 * @property {number} price
 * @property {boolean} isForSale
 * @property {boolean} isOwned
 * @property {number} sales
 */

/**
 * @typedef UserGameEntry
 * @property {number} id
 * @property {string} name
 * @property {string} description
 * @property {number} rootPlaceId
 * @property {PlaceEntry} rootPlace
 * @property {string} created
 * @property {string} updated
 * @property {number} placeVisits
 */

/**
 * @typedef PlaceEntry
 * @property {number} id
 * @property {string} type
 */

/**
 * @typedef BadgeEntry
 * @property {number} id
 * @property {string} name
 * @property {string} description
 * @property {boolean} enabled
 * @property {string} moderationStatus
 * @property {Date} created
 * @property {Date} updated
 * @property {BadgeStatistics} statistics
 * @property {BadgeAwardingUniverse} awardingUniverse
 */

/**
 * @typedef BadgeEntryBasic
 * @property {number} assetId
 * @property {number} universeId
 * @property {boolean} enabled
 */

/**
 * @typedef BadgeStatistics
 * @property {number} pastDayAwardedCount
 * @property {number} awardedCount
 * @property {number} winRatePercentage
 */

/**
 * @typedef BadgeAwardingUniverse
 * @property {number} id
 * @property {string} name
 * @property {number} rootPlaceId
 */

/**
 * @typedef BadgeAwardedDate
 * @property {number} badgeId
 * @property {date} awardedDate
 */

/**
 * @typedef DeveloperProductEntry
 * @property {number} id
 * @property {string} name
 * @property {string} Description
 * @property {number} shopId
 * @property {number} iconImageAssetId
 * @property {number} priceInRobux
 * @property {number} sales
 */