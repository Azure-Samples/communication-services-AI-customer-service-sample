// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import config from '../appsettings.json';
const BASE_URL = config.baseUrl;

export const clearCacheHistory = async (): Promise<boolean> => {
  try {
    const getRequestOptions = {
      method: 'GET'
    };
    const response = await fetch(`${BASE_URL}/api/debug/clearCache`, getRequestOptions);

    if (!response.ok) {
      throw new Error(`Failed to clear cache history. Status: ${response.status}`);
    }

    const details = await response.json();
    return details;
  } catch (error) {
    console.error('Failed to clear cache, Error: ', error);
    throw new Error('Failed to clear cache');
  }
};
