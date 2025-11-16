import getFlag from "../lib/getFlag";
import request, { getBaseUrl, getFullUrl } from "../lib/request"
import description from "../components/gameDetails/components/description";

export const uploadAsset = ({ name, assetTypeId, file, groupId, description = null, universeId = null }) => {
    let formData = new FormData();
    formData.append('name', name);
    formData.append('assetType', assetTypeId);
    formData.append('file', file);
    if (groupId) {
        formData.append('groupId', groupId);
    }
    if (description != null) {
        formData.append('description', description);
    }
    if (universeId != null) {
        formData.append('universeId', universeId);
    }
    return request('POST', getBaseUrl() + '/develop/upload', formData);
}

export const uploadAssetVersion = async ({ assetId, file }) => {
    let formData = new FormData();
    formData.append('assetId', assetId);
    formData.append('file', file);
    return request('POST', getBaseUrl() + '/develop/upload-version', formData);
    /*return new Promise((resolve, reject) => { // this breaks csrf im pretty sure -- zyth
      let form = new FormData();
      form.append('assetId', assetId);
      form.append('file', file);
  
      let xhr = new XMLHttpRequest();
  
      xhr.open('POST', getBaseUrl() + '/develop/upload-version', true);
  
      xhr.upload.onprogress = function(event) {
        if (event.lengthComputable) {
          let percentComplete = (event.loaded / event.total) * 100;
          console.log('Upload progress: ' + percentComplete.toFixed(2) + '%');
        }
      };
  
      xhr.responseType = 'json';
  
      xhr.onload = function() {
        if (xhr.status === 200) {
          resolve(xhr.response);
        } else {
          reject('Error uploading file: ' + xhr.statusText);
        }
      };
  
      xhr.onerror = function() {
        reject('Network error while uploading file.');
      };
  
      xhr.send(form);
    });*/
};

export const getCreatedAssetDetails = (assetIds) => {
    return request('POST', getFullUrl('itemconfiguration', '/v1/creations/get-asset-details'), {
        assetIds,
    })
}

export const getCreatedItems = ({ assetType, limit, cursor, groupId }) => {
    let url = '/v1/creations/get-assets?assetType=' + assetType + '&limit=' + limit + '&cursor=' + encodeURIComponent(cursor);
    if (groupId) {
        url = url + '&groupId=' + encodeURIComponent(groupId);
    }
    return request('GET', getFullUrl('itemconfiguration', url)).then(assets => {
        if (assets.data.data.length !== 0) {
            return getCreatedAssetDetails(assets.data.data.map(v => v.assetId)).then(d => {
                assets.data.data = d.data.sort((a, b) => a.assetId > b.assetId ? -1 : 1)
                return assets.data;
            })
        }
        return assets.data;
    })
}

export const updateAsset = async ({
                                      assetId,
                                      name,
                                      description,
                                      genres,
                                      isCopyingAllowed,
                                      enableComments,
                                      isForSale = false,
                                      verbose = false
                                  }) => {
    return await request('PATCH', getFullUrl('develop', `/v1/assets/${assetId}`), {
        name,
        description,
        genres,
        isCopyingAllowed,
        enableComments,
        isForSale
    }, verbose);
}

export const setAssetPrice = async ({ assetId, priceInRobux, priceInTickets }) => {
    let obj = {
        priceInRobux,
    };
    if (getFlag('sellItemForTickets', false)) {
        obj.priceInTickets = priceInTickets;
    }
    return await request('POST', getFullUrl('itemconfiguration', `/v1/assets/${assetId}/update-price`), obj);
}

/**
 * @typedef AssetRestriction
 * @property {number} assetId
 * @property {boolean} isLimited
 * @property {boolean} isLimitedUnique
 * @property {boolean} exists
 */

/**
 * @param {number[]} assetIds
 * @returns {Promise<AssetRestriction[]>}
 */
export const getAssetRestrictions = async (assetIds) => {
    let req = await request("GET", getFullUrl('itemconfiguration',
        `/v1/assets/restrictions?assetIds=${encodeURIComponent(assetIds)}`)
    );
    return req.data.data;
}

export const getAllGenres = async () => {
    return (await request('GET', getFullUrl('develop', '/v1/assets/genres'))).data.data;
}

export const setUniverseYear = async ({ universeId, year, verbose = false }) => {
    return await request('PATCH', getFullUrl('develop', `/v1/universes/${universeId}/set-year`), {
        year,
    }, verbose);
}

export const setUniverseMaxPlayers = async ({ universeId, maxPlayers, verbose = false }) => {
    return await request('PATCH', getFullUrl('develop', `/v1/universes/${universeId}/max-player-count`), {
        maxPlayers,
    }, verbose);
}

export const setPlaceRobloxPlaceId = async ({ placeId, robloxPlaceId, verbose = false }) => {
    return await request('PATCH', getFullUrl('develop', `/v1/places/${placeId}/roblox-place-id`), {
        robloxPlaceId,
    }, verbose);
}

export const uploadGameIcon = async ({ placeId, file }) => {
    const form = new FormData();
    form.append('file', file);
    return request('POST', getFullUrl('develop', `/v1/assets/upload-gameicon?placeId=${placeId}`), form)
    // return new Promise((resolve, reject) => {
    //   let form = new FormData();
    //   form.append('file', file);
    //
    //   let xhr = new XMLHttpRequest();
    //
    //   xhr.open('POST',
    //     getFullUrl('develop', `/v1/assets/upload-gameicon?placeId=${placeId}`))
    //
    //   xhr.upload.onprogress = function (event) {
    //     if (event.lengthComputable) {
    //       let percentComplete = (event.loaded / event.total) * 100;
    //       console.log('Upload progress: ' + percentComplete.toFixed(2) + '%');
    //     }
    //   };
    //
    //   xhr.responseType = 'json';
    //
    //   xhr.onload = function () {
    //     if (xhr.status === 200) {
    //       resolve(xhr.response);
    //     } else {
    //       reject('Error uploading file: ' + xhr.statusText);
    //     }
    //   };
    //
    //   xhr.onerror = function () {
    //     reject('Network error while uploading file.');
    //   };
    //
    //   xhr.send(form);
    // });
};

export const uploadAutoGenGameIcon = async ({ placeId }) => {
    return request('POST', getFullUrl('develop', `/v1/places/${placeId}/game-icons/auto-generated`))
};

export const uploadGameThumbnail = async ({ universeId, file }) => {
    const form = new FormData();
    form.append('file', file);
    return request('POST', getFullUrl('develop', `/v1/assets/upload-thumbnail?universeId=${universeId}`), form)
};

export const uploadAutoGenGameThumbnail = async ({ universeId }) => {
    return request('POST', getFullUrl('develop', `/v1/universes/${universeId}/thumbnails/auto-generated`))
};

export const deleteGameThumbnail = async ({ universeId, thumbnailId }) => {
    return request('POST', getFullUrl('develop', `/v1/universes/${universeId}/thumbnails/${thumbnailId}`))
};

// dev products

export const getDeveloperProducts = async ({ universeId }) => {
    return request('GET', getFullUrl('develop', `/v1/universes/${universeId}/developerproducts?pageSize=25`)).then(d => d?.data);
}

export const createDeveloperProduct = async ({ universeId, name, description, priceInRobux, imageId }) => {
    return request('POST', getFullUrl('develop', `/v1/universes/${universeId}/developerproducts?name=${encodeURIComponent(name)}&description=${encodeURIComponent(description)}&priceInRobux=${encodeURIComponent(priceInRobux)}&iconImageAssetId=${imageId}`));
}

export const updateDeveloperProduct = async ({ universeId, productId, name, description, imageId, price }) => {
    return request('POST', getFullUrl('develop', `/v1/universes/${universeId}/developerproducts/${productId}/update`),
        {
            Name: name,
            Description: description,
            IconImageAssetId: imageId,
            PriceInRobux: price
        }
    );
}