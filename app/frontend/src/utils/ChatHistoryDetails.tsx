import { ChatMessage } from "@azure/communication-react";
import config from '../appsettings.json';

const BASE_URL = config.baseUrl;

export const getChatHistoryDetails = async (threadId: string | null): Promise<ChatMessage[]> => {
    try {

        const getRequestOptions = {
            method: 'GET'
        };

        const response = await fetch(`${BASE_URL}/api/chat/history?threadId=${threadId}`, getRequestOptions);
        if (!response.ok) {
            throw new Error(`Failed to fetch chat history details. Status: ${response.status}`);
        }

        const details = await response.json().then((data) => data);
        return details;

    } catch (error) {

        console.error('Failed at getting environment url, Error: ', error);

        throw new Error('Failed at getting environment url');

    }
};