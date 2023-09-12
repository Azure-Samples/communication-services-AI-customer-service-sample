// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { AzureCommunicationTokenCredential, CommunicationUserIdentifier } from '@azure/communication-common';
import {
  ChatComposite,
  fromFlatCommunicationIdentifier,
  useAzureCommunicationChatAdapter
} from '@azure/communication-react';
import React, { useMemo, useState } from 'react';
import '../styles/Chat.css';

import { initializeIcons } from '@fluentui/react';

const DISPLAY_NAME = 'Customer';

initializeIcons();

interface ChatProps {
  threadId: string;
  token: string;
  userId: string;
  endpointUrl: string;
}
function Chat(props: ChatProps): JSX.Element {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const { endpointUrl, userId, token, threadId } = props;
  const displayName = DISPLAY_NAME;
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
      userId: fromFlatCommunicationIdentifier(userId) as CommunicationUserIdentifier,
      displayName,
      credential,
      threadId
    }),
    [endpointUrl, userId, displayName, credential, threadId]
  );
  const chatAdapter = useAzureCommunicationChatAdapter(chatAdapterArgs);

  if (chatAdapter) {
    return (
      <div className="chat-container">
        {isOpen ? (
          <div className="chat-popup">
            <div className="chat-header">
              <h4 className="chat-title">Customer Support</h4>
              <button onClick={() => setIsOpen(false)}>X</button>
            </div>
            <div className="chat-control">
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

export default Chat;
