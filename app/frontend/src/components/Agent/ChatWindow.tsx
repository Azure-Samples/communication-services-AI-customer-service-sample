// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import React, { useCallback, useMemo, useState } from 'react';
import { ChatAdapter, ChatComposite, useAzureCommunicationChatAdapter } from '@azure/communication-react';
import { AzureCommunicationTokenCredential, CommunicationUserIdentifier } from '@azure/communication-common';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';

type ChatWindowProps = {
  endpoint: string;
  userId: CommunicationUserIdentifier;
  token: string;
  displayName: string;
  threadId: string;
};

const ChatWindow: React.FC<ChatWindowProps> = (props: ChatWindowProps) => {
  const [isError, setIsError] = useState<boolean>(false);

  // A well-formed token is required to initialize the chat and calling adapters.
  const credential = useMemo(() => {
    try {
      return new AzureCommunicationTokenCredential(props.token);
    } catch {
      console.error('Failed to construct token credential');
      return undefined;
    }
  }, [props.token]);

  // load old chat messages right after useAzureCommunicationChatAdapter creates the adapter
  const adapterAfterCreate = useCallback(async (adapter: ChatAdapter): Promise<ChatAdapter> => {
    try {
      const loadMessagesResult = await adapter.loadPreviousChatMessages(100);
      console.log('loaded messages', loadMessagesResult);
    } catch (error) {
      setIsError(true);
    }

    return adapter;
  }, []);

  // Memoize arguments to `useAzureCommunicationCallAdapter` so that
  // a new adapter is only created when an argument changes.
  const chatAdapterArgs = useMemo(
    () => ({
      endpoint: props.endpoint,
      userId: props.userId,
      displayName: props.displayName,
      credential: credential,
      threadId: props.threadId
    }),
    [props.endpoint, props.userId, props.displayName, credential, props.threadId]
  );

  const chatAdapter = useAzureCommunicationChatAdapter(chatAdapterArgs, adapterAfterCreate);

  if (credential === undefined) {
    return (
      <div>
        <div className="agent-chat-control">Failed to construct credential. Provided token is malformed.</div>
      </div>
    );
  }

  if (!isError) {
    if (chatAdapter) {
      return (
        <div className="agent-chat-control">
          <ChatComposite adapter={chatAdapter} options={{ topic: false }} />
        </div>
      );
    }
    return (
      <div>
        <LoadingSpinner />
      </div>
    );
  } else {
    return <div></div>;
  }
};

export default ChatWindow;
