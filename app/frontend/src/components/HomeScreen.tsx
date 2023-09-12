// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import React, { useEffect, useState } from 'react';
import Chat from './Chat';
import '../styles/HomePage.css';
import { ChatDetailsData, getChatDetails } from '../utils/ChatClientDetails';
import { clearCacheHistory } from '../utils/CacheHistoryDetails';

/**
 * HomeScreen has two states:
 * 1. Showing start chat button
 * 2. Showing spinner after clicking start chat
 *
 * @param props
 */
export default (): JSX.Element => {
  const [chatData, setChatData] = useState<ChatDetailsData>();
  useEffect(() => {
    getChatDetails()
      .then((apiData) => {
        setChatData(apiData);
        localStorage.setItem('chatThreadId', apiData.threadId);
      })
      .catch((error) => {
        console.error('Error fetching data:', error);
      });
  }, []);

  async function handleClearHistory() {
    const response = await clearCacheHistory();
    if (response) {
      alert('Cache history cleared.');
    } else {
      alert('failed.');
    }
  }

  const displayHomeScreen = (): JSX.Element => {
    /* eslint-disable jsx-a11y/anchor-is-valid */
    return (
      <div className="home-container">
        <nav>
          <div className="logo">
            <b>Contoso</b> Energy
          </div>
          <div className="menu-items">
            <a href="#">Menu</a>
            <a href="#">Pay Bill</a>
            <a href="#">Outages</a>
            <a href="#">Support</a>
            <a href="#" className="search">
              Search
            </a>
          </div>
          <div className="right-items">
            <a href="#" className="language">
              English
            </a>
            <a href="#" className="account">
              Account
            </a>
            <a className="clear-history-btn" onClick={handleClearHistory}>
              Clear History
            </a>
          </div>
        </nav>
        <div className="content">
          <p className="title">Looking for ways to save? Try solar</p>
          <hr />
          <p className="subtitle">You may qualify for tax savings and other benefits.</p>
          <p className="subtitle">Chat with customer support to learn more</p>
        </div>
        {chatData && <Chat {...chatData} userId={chatData.identity} />}
      </div>
    );
  };

  return displayHomeScreen();
};
