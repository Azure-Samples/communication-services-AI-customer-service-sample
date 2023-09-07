import config from '../../appsettings.json';

const BASE_URL = config.baseUrl;

export interface AgentPageData {
    AgentId: string;
    Token: string;
    CallId: string;
    ThreadId: string;
    EndPointUrl: string;
}

export const getAgentPageData = async (): Promise<AgentPageData> => {
    try {
        const getRequestOptions = {
            method: 'GET'
        };
        const response = await fetch(`${BASE_URL}/api/agent/GetAgentData`, getRequestOptions);
        if (!response.ok) {
            throw new Error(`Failed to to get agent page data. Status: ${response.status}`);
        }
        const details = await response.json().then((data) => data);
        return details;

    } catch (error) {
        console.error('Failed to get agent page data. Error: ', error);
        throw new Error('Failed to get agent page data');
    }
};
