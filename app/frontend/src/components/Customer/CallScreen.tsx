// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import "../../styles/CallScreen.css";
import {
  AzureCommunicationTokenCredential,
  CommunicationUserIdentifier,
} from "@azure/communication-common";

import {
  CallAdapterLocator,
  CallAdapterState,
  useAzureCommunicationCallAdapter,
  CommonCallAdapter,
  CallAdapter,
} from "@azure/communication-react";

import React, { useCallback, useMemo, useRef } from "react";
import { CallCompositeContainer } from "./CallCompositeContainer";

export interface CallScreenProps {
  token: string;
  userId: CommunicationUserIdentifier;

  callLocator: CallAdapterLocator;
  displayName: string;
}

export const CallScreen = (props: CallScreenProps): JSX.Element => {
  const { token } = props;
  const callIdRef = useRef<string>();

  const subscribeAdapterEvents = useCallback((adapter: CommonCallAdapter) => {
    adapter.on("error", (e) => {
      // Error is already acted upon by the Call composite, but the surrounding application could
      // add top-level error handling logic here (e.g. reporting telemetry).
      console.log("Adapter error event:", e);
    });

    adapter.onStateChange((state: CallAdapterState) => {
      if (state?.call?.id && callIdRef.current !== state?.call?.id) {
        callIdRef.current = state?.call?.id;
        console.log(
          `Call Recording: ${state?.call?.recording.isRecordingActive}`
        );
        if (state?.call?.id && callIdRef.current !== state?.call?.id) {
          if (state?.call?.state === "Connecting") {
          }
          console.log(`Call State: ${state?.call?.state}`);
          console.log(
            `Call Recording: ${state?.call?.recording.isRecordingActive}`
          );
          console.log(`Call State: ${state?.call?.state}`);
          console.log(
            `Call Recording: ${state?.call?.recording.isRecordingActive}`
          );
          callIdRef.current = state?.call?.id;
          console.log(`Call Id: ${callIdRef.current}`);
        }
      }
    });
  }, []);

  const afterCallAdapterCreate = useCallback(
    async (adapter: CallAdapter): Promise<CallAdapter> => {
      adapter.joinCall({ microphoneOn: false, cameraOn: false });
      subscribeAdapterEvents(adapter);
      return adapter;
    },
    [subscribeAdapterEvents]
  );

  const credential = useMemo(() => {
    return new AzureCommunicationTokenCredential(token);
  }, [token]);

  return (
    <AzureCommunicationCallScreen
      afterCreate={afterCallAdapterCreate}
      credential={credential}
      {...props}
    />
  );
};

type AzureCommunicationCallScreenProps = CallScreenProps & {
  afterCreate?: (adapter: CallAdapter) => Promise<CallAdapter>;
  credential: AzureCommunicationTokenCredential;
};

const AzureCommunicationCallScreen = (
  props: AzureCommunicationCallScreenProps
): JSX.Element => {
  const { afterCreate, callLocator: locator, userId, ...adapterArgs } = props;

  if (!("communicationUserId" in userId)) {
    throw new Error(
      "A MicrosoftTeamsUserIdentifier must be provided for Teams Identity Call."
    );
  }

  const adapter = useAzureCommunicationCallAdapter(
    {
      ...adapterArgs,
      userId,
      locator,
    },
    afterCreate
  );

  return (
    <div className="main-container">
      <div className="header-content">
        <p>Customer Support</p>
      </div>
      <div className="top-content">
        <div>
          <CallCompositeContainer {...props} adapter={adapter} />
        </div>
      </div>
      <div className="footer-content">Contoso Energy</div>
    </div>
  );
};
