import {
    AzureCommunicationTokenCredential,
    CommunicationUserIdentifier,
} from '@azure/communication-common';
import {
    ChatComposite,
    fromFlatCommunicationIdentifier,
    useAzureCommunicationChatAdapter,
} from '@azure/communication-react';
import React, {
    CSSProperties,
    ElementRef,
    useEffect,
    useMemo,
    useRef,
    useState,
} from 'react';
import '../styles/Chat.css'

import { initializeIcons } from '@fluentui/react';
import { ChatDetailsData, getChatDetails } from '../utils/ChatClientDetails';


const DISPLAY_NAME = 'Customer';

initializeIcons();

function Chat(): JSX.Element { 
    const [isOpen, setIsOpen] = useState<boolean>(false);
    const { endpointUrl, userId, token, displayName, threadId } =
        useAzureCommunicationServiceArgs();
    const credential = useMemo(() => {
        try {
            return new AzureCommunicationTokenCredential(token);
        } catch {
            console.error('Failed to construct token credential');
            return undefined;
        }
    }, [token]);

    const chatAdapterArgs = useMemo(
        () => ({
            endpoint: endpointUrl,
            userId: fromFlatCommunicationIdentifier(
                userId
            ) as CommunicationUserIdentifier,
            displayName,
            credential,
            threadId,
        }),
        [endpointUrl, userId, displayName, credential, threadId]
    );
    const chatAdapter = useAzureCommunicationChatAdapter(chatAdapterArgs);

    if (!!chatAdapter) {
        return (
            <div className="chat-container">
                {isOpen ? (
                    <div className="chat-popup">
                        <div className="chat-header">
                            <h4 className="chat-title">Customer Support</h4>
                            <button onClick={() => setIsOpen(false)}>X</button>
                        </div>
                        <div className="chat-control" >
                            <ChatComposite adapter={chatAdapter} options={{ topic: false }} />
                        </div>
                    </div>
                ) : (
                    <button className="open-chat-button" onClick={() => setIsOpen(true)}>
                        Customer Support
                    </button>
                )}
            </div>
        );
    }
    
    return <h3>Initializing...</h3>;
}


function useAzureCommunicationServiceArgs(): {
    endpointUrl: string;
    userId: string;
    token: string;
    displayName: string;
    
    threadId: string;
} {
    const [threadId, setThreadId] = useState('');
    const [data, setData] = useState<ChatDetailsData>();

    useEffect(() => {
        getChatDetails()
            .then(apiData => {
                setData(apiData);
                localStorage.setItem("chatThreadId", apiData.threadId);
            })
            .catch(error => {
                console.error('Error fetching data:', error);
            });
    }, []);

      useEffect(() => {
        if (data !== undefined) {
          setThreadId(data.threadId);
        }
      }, [data]);

    return {
        endpointUrl: data !== undefined ? data.endpointUrl:'',
        userId: data !== undefined ? data.identity : '',
        token: data !== undefined ? data.token : '',
        displayName: DISPLAY_NAME,
        threadId: data !== undefined ? data.threadId : '',
    };
}

export default Chat;
