
import config from '../appsettings.json';

const BASE_URL = config.baseUrl;
export interface CallUserDetails {
    UserId: string;
    Token: string;
}

 export const getCallUserDetails = async (): Promise<CallUserDetails> => {

    try {

        const getRequestOptions = {

            method: 'GET'

        };

        const response = await fetch(`${BASE_URL}/api/identity/get/token`, getRequestOptions);
        
        if (!response.ok) {
            throw new Error(`Failed to fetch user details. Status: ${response.status}`);
        }

        const details = await response.json().then((data) => data);

        return details;

    } catch (error) {

        console.error('Failed at getting environment url, Error: ', error);

        throw new Error('Failed at getting environment url');

    }
};
