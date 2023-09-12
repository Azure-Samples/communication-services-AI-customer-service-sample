// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import config from '../appsettings.json';
const BASE_URL = config.baseUrl;
export interface ChatDetailsData {
  threadId: string;
  token: string;
  identity: string;
  endpointUrl: string;
}

export const getChatDetails = async (): Promise<ChatDetailsData> => {
  try {
    const getRequestOptions = {
      method: 'GET'
    };
    const response = await fetch(`${BASE_URL}/api/customer/startconversation`, getRequestOptions);

    if (!response.ok) {
      throw new Error(`Failed to fetch chat thread details. Status: ${response.status}`);
    }

    const details = await response.json().then((data) => data);
    return details;
  } catch (error) {
    console.error('Failed to start conversation, Error: ', error);
    throw new Error('Failed to start conversation');
  }
};

export const getHelperChatDetails = async (): Promise<ChatDetailsData> => {
  try {
    const getRequestOptions = {
      method: 'GET'
    };
    const response = await fetch(`${BASE_URL}/api/agent/chatassistant`, getRequestOptions);
    if (!response.ok) {
      throw new Error(`Failed to fetch helper chat details. Status: ${response.status}`);
    }
    const details = await response.json().then((data) => data);
    return details;
  } catch (error) {
    console.error('Failed to retrieve chatassistant data, Error: ', error);
    throw new Error('Failed to retrieve chatassistant data');
  }
};
