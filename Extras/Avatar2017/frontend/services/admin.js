import request, { getBaseUrl } from "../lib/request";

/**
 * @returns {Promise<[]>}
 */
export const getPendingGroupIcons = () => {
    return request("GET", getBaseUrl() + "/admin-api/api/groups/pending-icons")
        .then(d => d.data);
}

/**
 * @returns {Promise<[]>}
 */
export const getPendingAssets = () => {
    return request("GET", getBaseUrl() + "/admin-api/api/assets/pending-assets")
        .then(d => d.data);
}

/**
 * @returns {Promise<[]>}
 */
export const getPendingIcons = () => {
    return request("GET", getBaseUrl() + "/admin-api/api/icons/pending-assets")
        .then(d => d.data);
}

/**
 * @returns {Promise<{count: number;}>}
 */
export const getPendingApplicationCount = () => {
    return request("GET", getBaseUrl() + "/admin-api/api/applications/pending-num")
        .then(d => d.data);
}

/**
 * @returns {Promise<{count: number;}>}
 */
export const getPendingReportCount = () => {
    return request("GET", getBaseUrl() + "/admin-api/api/reports/pending-count")
        .then(d => d.data);
}
