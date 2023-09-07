import { useEffect, useMemo, useState } from "react";
import { AzureCommunicationTokenCredential, CommunicationUserIdentifier } from "@azure/communication-common";
import { GroupLocator } from "@azure/communication-calling";
import VideoWindow from "./VideoWindow";
import ChatWindow from "./ChatWindow";
import SummaryWindow from './SummaryWindow';
import '../../styles/AgentPage.css';
import { ConversationalInsights, getSummaryDetails } from "../../utils/SummaryList";
import Section from "./OpenAIAssistant";
import HelperChat from "../HelperChat/HelperChat";

export interface AgentPageProps {
    token: string;
    agentId: string;
    callId: string;
    displayName: string;
    endpoint: string;
    threadId: string;
}

export const AgentPage = (props: AgentPageProps): JSX.Element => {
    const ACSUserid: CommunicationUserIdentifier = { communicationUserId: props.agentId }
    const callLocator: GroupLocator = { groupId: props.callId !== undefined ? props.callId : '' };

    const [assistantPanelData, setassistantPanelData] = useState<ConversationalInsights>();
    const chatThreadId = localStorage.getItem("chatThreadId");

    useEffect(() => {
        loadAssistantPanelData();
    }, []);

    /*Refresh assistant panel data on closing call. */
     const refreshAssistantPanelData = () => {
        loadAssistantPanelData();
    }

     const loadAssistantPanelData = () => {
        getSummaryDetails(chatThreadId).then(apiData => {
            setassistantPanelData(apiData);
        });
    }

    return (
        <div>
            <nav>
                <div className="agent-logo"><b>Contoso</b> Energy | Technician</div>
            </nav>
            <div id="conversation">
                <div className="agent-header">
                    <h2><span>Icon</span>Conversation</h2>
                </div>
                <div style={{ width: '100%', position: 'absolute', top: '0', bottom: '0', display: 'flex', flexDirection: 'column' }}>
                    <ChatWindow {...props} userId={ACSUserid} />
                    <VideoWindow refreshAssistantPanelData={()=> {
                        refreshAssistantPanelData();
                    } } {...props} userId={ACSUserid} callLocator={callLocator} />
                </div>
            </div>
            <div id="copilot">
                <div className="agent-header">
                    <h2><span>Icon</span>Assistant</h2>
                </div>
                {assistantPanelData && assistantPanelData.summaryItems.map((section, index) => (
                    <Section
                        key={index}
                        title={section.title}
                        details={section.description}
                    />
                ))}
                <SummaryWindow />
                <div>
                    <HelperChat/>
                </div>
            </div>
        </div>
    );
};
