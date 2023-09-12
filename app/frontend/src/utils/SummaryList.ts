// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import config from '../appsettings.json';
const BASE_URL = config.baseUrl;

export interface ConversationalInsights {
  threadId: string;
  topicName: string;
  summaryItems: SummaryItem[];
}

export interface SummaryItem {
  title: string;
  description: string;
}

export const getSummaryDetails = async (threadId: string | null): Promise<ConversationalInsights> => {
  try {
    const getRequestOptions = { method: 'GET' };
    // eslint-disable-next-line no-template-curly-in-string
    const response = await fetch(`${BASE_URL}/api/conversation/insights/${threadId}`, getRequestOptions);
    if (!response.ok) {
      throw new Error(`Failed to fetch summary details. Status: ${response.status}`);
    }

    const values = await response.json();

    return values;
  } catch (error) {
    console.error('Failed at getting environment url, Error: ', error);
    throw new Error('Failed at getting environment url');
  }
};
