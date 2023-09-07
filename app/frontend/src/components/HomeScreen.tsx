/* eslint-disable import/no-anonymous-default-export */
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import {
    IImageStyles,
    Icon,
    Image,
    Link,
    List,
    PrimaryButton,
    Spinner,
    Stack,
    Text,
    mergeStyles
} from '@fluentui/react';
import React, { useCallback, useEffect, useState } from 'react';

import { useTheme } from '@azure/communication-react';

import { Chat20Filled } from '@fluentui/react-icons';
import heroSVG from '../assets/hero.svg';
import heroDarkModeSVG from '../assets/hero_dark.svg';
import Navbar from './NavBar/Navbar';
import Chat from './Chat';
import '../styles/HomePage.css';



const imageStyleProps: IImageStyles = {
    image: {
        height: '100%'
    },
    root: {}
};

export interface ScreenProps {
    joinChatHandler(): void;

}

/**
 * HomeScreen has two states:
 * 1. Showing start chat button
 * 2. Showing spinner after clicking start chat
 *
 * @param props
 */
export default (props: ScreenProps): JSX.Element => {
    const { joinChatHandler } = props;
    const spinnerLabel = 'Creating a new chat thread...';
    const startChatButtonText = 'Start chat';

    const [name, setName] = useState('');



    const displayLoadingSpinner = (spinnerLabel: string): JSX.Element => {
        return <Spinner label={spinnerLabel} ariaLive="assertive" labelPosition="top" />;
    };


    const setupAndJoinChatThreadWithNewUser = useCallback(() => {
        const internalSetupAndJoinChatThread = async (): Promise<void> => {

            joinChatHandler();
        };
        internalSetupAndJoinChatThread();
    }, [joinChatHandler]);



    const onCreateThread = async (): Promise<void> => {
        setupAndJoinChatThreadWithNewUser();
    };


    const displayHomeScreen = (): JSX.Element => {
        return (
            <div className="home-container">
                <nav>
                    <div className="logo"><b>Contoso</b> Energy</div>
                    <div className="menu-items">
                        <a href="#">Menu</a>
                        <a href="#">Pay Bill</a>
                        <a href="#">Outages</a>
                        <a href="#">Support</a>
                        <a href="#" className='search'>Search</a>
                    </div>
                    <div className="right-items">
                        <a href="#" className="language">English</a>
                        <a href="#" className="account">Account</a>
                    </div>
                </nav>
                <div className="content">
                    <p className="title">Looking for ways to save? Try solar</p>
                    <hr />
                    <p className="subtitle">You may qualify for tax savings and other benefits.</p>
                    <p className="subtitle">Chat with customer support to learn more</p>

                </div>
                <Chat />
            </div>
        );
    };

    return displayHomeScreen();

};
