// Copyright (c) Microsoft Corporation.

// Licensed under the MIT license.

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

        const response = await fetch(`${BASE_URL}/api/chat/create/thread/join`, getRequestOptions);

        if (!response.ok) {
            throw new Error(`Failed to fetch chat thread details. Status: ${response.status}`);
        }

        const details = await response.json().then((data) => data);

        return details;

    } catch (error) {

        console.error('Failed at getting environment url, Error: ', error);

        throw new Error('Failed at getting environment url');

    }
};

export const getHelperChatDetails = async (): Promise<ChatDetailsData> => {
    try {
        const getRequestOptions = {
            method: 'GET'
        };
        const response = await fetch(`${BASE_URL}/api/agent/join/thread`, getRequestOptions);
        if (!response.ok) {
            throw new Error(`Failed to fetch helper chat details. Status: ${response.status}`);
        }
        const details = await response.json().then((data) => data);
        return details;

    } catch (error) {
        console.error('Failed at getting environment url, Error: ', error);
        throw new Error('Failed at getting environment url');
    }
};

