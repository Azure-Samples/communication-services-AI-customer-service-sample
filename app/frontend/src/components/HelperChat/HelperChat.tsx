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
    useEffect,
    useMemo,
    useState,
} from 'react';
import '../../styles/HelperChat.css'

import { initializeIcons } from '@fluentui/react';
import { ChatDetailsData, getHelperChatDetails } from '../../utils/ChatClientDetails';


const DISPLAY_NAME = 'Agent';

initializeIcons();

function HelperChat(): JSX.Element {
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
            <div className="helper-chat-container">
                {isOpen ? (
                    <div className="helper-chat-popup">
                        <div className="helper-chat-header">
                            <h4 className="helper-chat-title">Help</h4>
                            <button onClick={() => setIsOpen(false)}>X</button>
                        </div>
                        <div className="helper-chat-control" >
                            <ChatComposite adapter={chatAdapter} options={{ topic: false }} />
                        </div>
                    </div>
                ) : (
                    <button className="open-helper-chat-button" onClick={() => setIsOpen(true)}>
                        Help
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
    const [helperChatData, setHelperChatData] = useState<ChatDetailsData>();

    useEffect(() => {
        getHelperChatDetails()
            .then(helperChatApiData => {
                setHelperChatData(helperChatApiData);
            })
            .catch(error => {
                console.error('Error fetching data:', error);
            });
    }, []);

    return {
        endpointUrl: helperChatData !== undefined ? helperChatData.endpointUrl : '',
        userId: helperChatData !== undefined ? helperChatData.identity : '',
        token: helperChatData !== undefined ? helperChatData.token : '',
        displayName: DISPLAY_NAME,
        threadId: helperChatData !== undefined ? helperChatData.threadId : '',
    };
}

export default HelperChat;
