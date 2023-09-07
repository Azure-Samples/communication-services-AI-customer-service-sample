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

const helperChatDetails: ChatDetailsData = {
    threadId: '19:oLZIAWg7WVuw2r4lLyNEq9SIr2mj2x-mLkNLPqssucE1@thread.v2',
    token: 'eyJhbGciOiJSUzI1NiIsImtpZCI6IjVFODQ4MjE0Qzc3MDczQUU1QzJCREU1Q0NENTQ0ODlEREYyQzRDODQiLCJ4NXQiOiJYb1NDRk1kd2M2NWNLOTVjelZSSW5kOHNUSVEiLCJ0eXAiOiJKV1QifQ.eyJza3lwZWlkIjoiYWNzOmJiZWJmZjhmLTQ2N2QtNDJjNy1hYWFiLWQxN2MyZWUyOTJjM18wMDAwMDAxYi0wNDY4LWRkN2UtMjhmNC0zNDNhMGQwMDliYjMiLCJzY3AiOjE3OTIsImNzaSI6IjE2OTM5MDM2OTgiLCJleHAiOjE2OTM5OTAwOTgsInJnbiI6ImFtZXIiLCJhY3NTY29wZSI6ImNoYXQsdm9pcCIsInJlc291cmNlSWQiOiJiYmViZmY4Zi00NjdkLTQyYzctYWFhYi1kMTdjMmVlMjkyYzMiLCJyZXNvdXJjZUxvY2F0aW9uIjoidW5pdGVkc3RhdGVzIiwiaWF0IjoxNjkzOTAzNjk4fQ.Qo0XPAd1Oc6BrU2bM9acPibTMj0Zstzr5tMu86hZqSEDfQm1nCcBrthA3njmiWcrVG1-YLDuDv-HUmXRzoJd0TxN_aCtxa-Snv3fEXYV3IGh8uxq0dBQ13nEOFbStkRRrXvPHZyWYlftEXekT80avD5hz-dJ190Z_t_eN64WavQQS9bsSOclXZkVaCwQ9NtD3PwnC5DK8gv2FdHBTcV4Ajh24ZOVI1BmyzTV_6cCRPzxc9d3pUVnuInI0OiKMXvDIBRw_Rpxj50Q04kedUmY4P9_Ps7EMWjDCeUIhAS6fx-UKv2fyYokjzRhD9y63NWwuvgAGpBVHJArrBYKr5uKpg',
    identity: '8:acs:bbebff8f-467d-42c7-aaab-d17c2ee292c3_0000001b-0468-dd7e-28f4-343a0d009bb3',
    endpointUrl: 'https://acssampledemo.unitedstates.communication.azure.com/'
}
export const getHelperChatDetails = async (): Promise<ChatDetailsData> => {

    try {

        //const getRequestOptions = {

        //    method: 'GET'

        //};

        //const response = await fetch(`${BASE_URL}/api/chat/create/thread/join`, getRequestOptions);

        //if (!response.ok) {
        //    throw new Error(`Failed to fetch chat thread details. Status: ${response.status}`);
        //}

        //const details = await response.json().then((data) => data);

        //return details;

        return helperChatDetails;

    } catch (error) {

        console.error('Failed at getting environment url, Error: ', error);

        throw new Error('Failed at getting environment url');

    }
};

