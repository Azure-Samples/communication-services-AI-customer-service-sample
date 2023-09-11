/* eslint-disable import/no-anonymous-default-export */
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { setLogLevel } from '@azure/logger';
import { initializeIcons } from '@fluentui/react';
import React, { useEffect, useState } from 'react';
import HomeScreen from './components/HomeScreen';
import { initializeFileTypeIcons } from '@fluentui/react-file-type-icons';
import { AgentPage } from './components/Agent/Agent';
import { CallScreen } from './components/Customer/CallScreen';
import { CommunicationUserIdentifier } from '@azure/communication-common';
import { CallAdapterLocator } from '@azure/communication-react';
import { GroupLocator } from '@azure/communication-calling';
import { CustomerDetails, getCustomerDetails } from './utils/CallAuthenicationDetails';
import { AgentPageData, getAgentPageData } from './components/Agent/InitAgentData';

setLogLevel('warning');

initializeIcons();
initializeFileTypeIcons();

// Extract "callerType" query parameter from the URL
const queryParams = new URLSearchParams(window.location.search);
let callerType: string | null | undefined = queryParams.get('callerType');
let loadpage = "";
if (callerType?.trim().toLowerCase() === "customer") {
    loadpage = "Customer";
}
else if (callerType?.trim().toLowerCase() === "agent") {
    loadpage = "Agent";
}
else {
    loadpage = "home";
}

const webAppTitle = document.title;

export default (): JSX.Element => {
    const [page] = useState(loadpage);
    const [customerData, setCustomerData] = useState<CustomerDetails>();
    const [agentData, setAgentData] = useState<AgentPageData>();

    useEffect(() => {
        if (customerData === undefined && loadpage === "Customer") {
            getCustomerDetails()
                .then(apiData => {
                    setCustomerData(apiData);
                })
                .catch(error => {
                    console.error('Error fetching data:', error);
                });
        }
        
        if (agentData === undefined && loadpage === "Agent") 
        {
            getAgentPageData()
                .then(apiData => {
                    setAgentData(apiData);
                })
                .catch(error => {
                    console.error('Error fetching data:', error);
            });
        }
    },[agentData,customerData]);

    let customerCallLocator: CallAdapterLocator
    const renderPage = (): JSX.Element => {
        switch (page) {
            case 'home': {
                document.title = `Home - ${webAppTitle}`;
                return <HomeScreen/>;
            }
            
            case 'Agent': {
                if (agentData !== undefined) {
                    const displayName = "Technician";
                    document.title = `Technician - ${webAppTitle}`;
                    let threadId = agentData?.ThreadId ?? '';
                    let callId = agentData?.CallId ?? '';

                    return <AgentPage
                      token={agentData.Token}
                      agentId={agentData.AgentId}
                      displayName={displayName}
                      callId={callId}
                      threadId={threadId}
                      endpoint={agentData.EndPointUrl} />;
                }
                else {
                    return <h3>Initializing...</h3>
                }
            }

            case 'Customer': {
                if (customerData !== undefined) {
                    const createGroupId = (): GroupLocator => ({ groupId: customerData.CallId });
                    const ACSUserid: CommunicationUserIdentifier = {
                        communicationUserId: customerData.UserId
                    }
                    const token = customerData.Token;
                    const displayName = "Customer";
                    document.title = `Customer - ${webAppTitle}`;
                    customerCallLocator = createGroupId();
                    return (
                        <CallScreen token={token} userId={ACSUserid} displayName={displayName} callLocator={customerCallLocator} />
                    );
                }
                else {
                    return <h3>Initializing...</h3>
                }
            }
            default:
                document.title = `error - ${webAppTitle}`;
                throw new Error('Page type not recognized');
        }
    };

    return renderPage();
};
