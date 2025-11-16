import axios from 'axios';
import config from './config';

let _csrf = '';

/**
 * @param {string} service
 * @param {string} url
 * @returns {string}
 */
export const getFullUrl = (service, url) => {
    return config.publicRuntimeConfig.backend.apiFormat.replace(/\{0\}/g, service).replace(/\{1\}/g, url);
}

/**
 * @returns {string}
 */
export const getBaseUrl = () => {
    return config.publicRuntimeConfig.backend.baseUrl;
}

/**
 * @param {string} str
 * @returns {string}
 */
export const getBaseUrl2 = (str) => {
    return config.publicRuntimeConfig.backend.baseUrl + (str.charAt(0) === '/' ? str : '/' + str);
}

/**
 * @param {string} url
 * @returns {string}
 */
export const getUrlWithProxy = (url) => {
    if (config.publicRuntimeConfig.backend.proxyEnabled)
        return '/api/proxy?url=' + encodeURIComponent(url);
    return url;
}

/**
 * @param {string} method
 * @param {string} url
 * @param {any?} data
 * @param {boolean?} verbose
 * @returns {Promise<axios.AxiosResponse<any>>}
 */
const request = async (method, url, data, verbose = false) => {
    const isBrowser = typeof window !== 'undefined';
    try {
        let headers = {
            'x-csrf-token': _csrf,
        }
        if (!isBrowser) {
            // Auth header, if required
            const authHeaderValue = config.serverRuntimeConfig.backend.authorization;
            if (typeof authHeaderValue === 'string')
                headers[config.serverRuntimeConfig.backend.authorizationHeader || 'authorization'] = authHeaderValue;
            // Custom user agent
            headers['user-agent'] = 'Roblox2016/1.0';
        }
        return await axios.request({
            method,
            url: getUrlWithProxy(url),
            data: data,
            headers: headers,
            maxRedirects: 0,
        });
    } catch (e) {
        if (e.response) {
            let resp = e.response;
            console.log(resp.headers)
            if (resp.status === 403 && resp.headers['x-csrf-token']) {
                _csrf = resp.headers['x-csrf-token'];
                return request(method, url, data);
            }
        }
        if (isBrowser) {
            // attempt to make errors easier to diagnose
            if (e.response) {
                // check for regular
                if (e.response.data && e.response.data.errors && e.response.data.errors.length) {
                    let err = e.response.data.errors[0]
                    e.message = e.message + ': ' + (err.code + ': ' + err.message);
                    // TODO: confirm this is causing issues
                    if (verbose && Number(String(e.response.status, "Could not parse response status")[0]) !== 5) {
                        return Promise.resolve(e.response);
                    }
                }
            }
            throw e;
        } else {
            throw new Error(e.message);
        }
    }
}

export default request;
