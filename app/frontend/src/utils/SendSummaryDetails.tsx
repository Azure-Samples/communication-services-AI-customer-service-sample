// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import config from '../appsettings.json';
const BASE_URL = config.baseUrl;

export interface SummaryEmailData {
  address: string;
  body: string;
}

export interface FinalSummary {
  result: string;
}

export const SendSummaryDetails = async (summaryDetails: SummaryEmailData) => {
  try {
    const response = await fetch(`${BASE_URL}/api/conversation/emailSummary/sendEmail`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(summaryDetails)
    });
    if (response.ok) {
      return await response.json();
    } else {
      console.error('Failed to send summary data');
      return { result: 'Failed' };
    }
  } catch (error) {
    console.error('An error occurred:', error);
    return { result: 'Failed' };
  }
};

export const getCustomerCommunicationDetails = async (threadId: string | null): Promise<FinalSummary> => {
  try {
    const getRequestOptions = {
      method: 'GET'
    };
    const response = await fetch(`${BASE_URL}/api/conversation/emailSummary/${threadId}`, getRequestOptions);

    if (!response.ok) {
      throw new Error(`Failed to fetch conversation summary details. Status: ${response.status}`);
    }

    const details = await response.json().then((data) => data);

    return details;
  } catch (error) {
    console.error('Failed at getting environment url, Error: ', error);

    throw new Error('Failed at getting environment url');
  }
};
