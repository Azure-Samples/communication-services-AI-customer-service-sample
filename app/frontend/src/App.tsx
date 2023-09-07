/* eslint-disable import/no-anonymous-default-export */
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { setLogLevel } from '@azure/logger';
import { initializeIcons, Spinner } from '@fluentui/react';
import React, { useEffect, useState } from 'react';
import HomeScreen from './components/HomeScreen';
import { initializeFileTypeIcons } from '@fluentui/react-file-type-icons';
import Chat from './components/Chat';
import Navbar from './components/NavBar/Navbar';
import { AgentPage } from './components/newAgent/Agent';
import { CallScreen } from './components/Customer/CallScreen';
import { CommunicationUserIdentifier } from '@azure/communication-common';
import { useIsMobile } from './utils/useIsMobile';
import { CallAdapterLocator } from '@azure/communication-react';
import { GroupLocator } from '@azure/communication-calling';
import { v1 as generateGUID } from 'uuid';
import { CallUserDetails, getCallUserDetails } from './utils/CallAuthenicationDetails';
import { AgentPageData, getAgentPageData } from './components/newAgent/InitAgentData';


setLogLevel('warning');

initializeIcons();
initializeFileTypeIcons();

// Extract "callerType" query parameter from the URL
const queryParams = new URLSearchParams(window.location.search);
// const accessToken = queryParams.get('accessToken');
// const userId = queryParams.get('userId');
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
    const [page, setPage] = useState(loadpage);
    const [callerData, setCallerData] = useState<CallUserDetails>();
    const [agentData, setAgentData] = useState<AgentPageData>();

    useEffect(() => {
        if (callerData === undefined && loadpage === "Customer") {
            getCallUserDetails()
                .then(apiData => {
                    setCallerData(apiData);
                })
                .catch(error => {
                    console.error('Error fetching data:', error);
                });
        }
        if (agentData === undefined) 
        {
            getAgentPageData()
                .then(apiData => {
                    setAgentData(apiData);
                })
                .catch(error => {
                    console.error('Error fetching data:', error);
            });
        }
    },[]);

    let callLocator: CallAdapterLocator
    const createGroupId = (): GroupLocator => ({ groupId: agentData?.CallId !== undefined ? agentData.CallId : '' });
       const isLandscape = (): boolean => window.innerWidth < window.innerHeight;
      const isMobileSession = useIsMobile();
      const isLandscapeSession = isLandscape();
      useEffect(() => {
        if (isMobileSession && isLandscapeSession) {
          console.log('ACS Calling sample: Mobile landscape view is experimental behavior');
        }
      }, [isMobileSession, isLandscapeSession]);

    const renderPage = (): JSX.Element => {
        switch (page) {
            case 'home': {
                document.title = `Home - ${webAppTitle}`;
                return <HomeScreen
                    joinChatHandler={() => {
                        setPage('chat');
                    }} />;
            }
            
            case 'Agent': {
                if (agentData !== undefined) {
                    const displayName = "Technician";
                    document.title = `Technician - ${webAppTitle}`;
                    let threadId = agentData?.ThreadId ?? '';
                    let callId = agentData?.CallId ?? '';
                    console.log('Loading Technician view');
                    console.log('ThreadId: %s, CallId: %s, UserId: %s, EndPointUrl: %s', agentData.ThreadId, agentData.CallId, agentData.AgentId, agentData.EndPointUrl);

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
                if (callerData !== undefined && agentData?.CallId !== undefined) {

                    const ACSUserid: CommunicationUserIdentifier = {
                        communicationUserId: callerData.UserId
                    }
                    const token = callerData.Token;
                    const displayName = "Customer";
                    document.title = `Customer - ${webAppTitle}`;
                    console.log('Customer');
                    console.log(callLocator);
                    callLocator = createGroupId();
                    return (
                        <CallScreen token={token} userId={ACSUserid} displayName={displayName} callLocator={callLocator} />
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
