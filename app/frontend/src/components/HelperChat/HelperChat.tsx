// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { AzureCommunicationTokenCredential, CommunicationUserIdentifier } from '@azure/communication-common';
import {
  ChatComposite,
  fromFlatCommunicationIdentifier,
  useAzureCommunicationChatAdapter
} from '@azure/communication-react';
import React, { useMemo, useState } from 'react';
import '../../styles/HelperChat.css';

import { initializeIcons } from '@fluentui/react';

const DISPLAY_NAME = 'Agent';

initializeIcons();

interface HelperChatProps {
  threadId: string;
  token: string;
  userId: string;
  endpointUrl: string;
}
function HelperChat(props: HelperChatProps): JSX.Element {
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
      <div className="helper-chat-container">
        {isOpen ? (
          <div className="helper-chat-popup">
            <div className="helper-chat-header">
              <h4 className="helper-chat-title">Help</h4>
              <button onClick={() => setIsOpen(false)}>X</button>
            </div>
            <div className="helper-chat-control">
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

export default HelperChat;
