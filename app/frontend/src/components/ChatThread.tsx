import { ChatMessage, FluentThemeProvider, MessageStatus, MessageThread } from '@azure/communication-react';
import React, { useEffect, useState } from 'react';
import { getChatHistoryDetails } from '../utils/ChatHistoryDetails';

export const MessageHistory: () => JSX.Element = () => {
    const [messages, setMessages] = useState<ChatMessage[]>([]);

    const threadId = localStorage.getItem("chatThreadId");

    useEffect(() => {
        getChatHistoryDetails(threadId).then(apiData => {
            apiData.map((m) => m.createdOn = new Date(m.createdOn));
            setMessages(apiData);
        })
    }, [threadId])

    return (
        <FluentThemeProvider>
            <MessageThread
                userId={'1'}
                messages={messages}

                onCancelEditMessage={(id) => {
                    const updated = messages.map((m) =>
                        m.messageId === id ? { ...m, failureReason: undefined, status: undefined } : m
                    );
                    setMessages(updated);
                }}
            />
        </FluentThemeProvider>
    );
}; 