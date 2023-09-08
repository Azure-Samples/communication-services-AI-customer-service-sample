// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { CommonCallAdapter, CallComposite, CallCompositeOptions } from '@azure/communication-react';

import { Spinner } from '@fluentui/react';
import { useIsMobile } from '../../utils/useIsMobile';
import React, { useMemo } from 'react';
import { CallScreenProps } from './CallScreen';


export type CallCompositeContainerProps = CallScreenProps & { adapter?: CommonCallAdapter };

export const CallCompositeContainer = (props: CallCompositeContainerProps): JSX.Element => {
  const { adapter } = props;
  //const { currentTheme, currentRtl } = useSwitchableFluentTheme();
  const isMobileSession = useIsMobile();

  // Dispose of the adapter in the window's before unload event.
  // This ensures the service knows the user intentionally left the call if the user
  // closed the browser tab during an active call.
 
  // Call composite options
  const callOptions: CallCompositeOptions = useMemo(() => {
    return {
      callControls: {
        participantsButton: false,
        screenShareButton: false,
      },
    };
  }, []);
  
  if (!adapter) {
    return <Spinner   ariaLive="assertive" labelPosition="top" />;
  }


  return (
    <CallComposite
          adapter={adapter}
          formFactor={isMobileSession ? 'mobile' : 'desktop'}
      options={callOptions}
    />
  );
};
