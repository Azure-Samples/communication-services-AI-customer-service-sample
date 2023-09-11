// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import React, { useCallback, useMemo, useState } from "react";
import Modal from "react-modal";
import {
  AzureCommunicationTokenCredential,
  CommunicationUserIdentifier,
} from "@azure/communication-common";
import {
  useAzureCommunicationCallAdapter,
  CallComposite,
  CallAdapterLocator,
  CallAdapter,
  CallCompositeOptions,
} from "@azure/communication-react";
import "../../styles/VideoWindow.css";
import LoadingSpinner from "../LoadingSpinner/LoadingSpinner";

type VideoWindowProps = {
  userId: CommunicationUserIdentifier;
  token: string;
  callLocator: CallAdapterLocator;
  displayName: string;
  refreshAssistantPanelData(): void;
};

const VideoWindow: React.FC<VideoWindowProps> = (props: VideoWindowProps) => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  // A well-formed token is required to initialize the chat and calling adapters.
  const credential = useMemo(() => {
    try {
      return new AzureCommunicationTokenCredential(props.token);
    } catch {
      console.error("Failed to construct token credential");
      return undefined;
    }
  }, [props.token]);

  // Memoize arguments to `useAzureCommunicationCallAdapter` so that
  // a new adapter is only created when an argument changes.
  const callAdapterArgs = useMemo(
    () => ({
      userId: props.userId,
      displayName: props.displayName,
      credential: credential,
      locator: props.callLocator,
    }),
    [props.userId, props.displayName, props.callLocator, credential]
  );
  // Call composite options
  const callOptions: CallCompositeOptions = useMemo(() => {
    return {
      callControls: {
        participantsButton: false,
        screenShareButton: false,
      },
    };
  }, []);

  const afterCallAdapterCreate = useCallback(
    async (adapter: CallAdapter): Promise<CallAdapter> => {
      adapter.joinCall({ microphoneOn: false, cameraOn: false });
      return adapter;
    },
    []
  );
  const callAdapter = useAzureCommunicationCallAdapter(
    callAdapterArgs,
    afterCallAdapterCreate
  );

  if (credential === undefined) {
    return (
      <div>
        <div className="sendSummaryContainer">
          Failed to construct credential. Provided token is malformed.
        </div>
      </div>
    );
  }

  const handleModalClose = () => {
    setIsModalOpen(false);
    props.refreshAssistantPanelData();
  };

  if (callAdapter) {
    return (
      <div>
        <div className="send-summary-container">
          <button
            className="send-summary-button"
            onClick={() => setIsModalOpen(true)}
          >
            Answer Call
          </button>
        </div>
        <Modal
          isOpen={isModalOpen}
          className="video-dialog"
          overlayClassName="overlay"
          ariaHideApp={false}
        >
          <div className="titlebar">
            Customer
            <button className="close-modal" onClick={handleModalClose}>
              X
            </button>
          </div>
          <div className="call-control">
            <CallComposite adapter={callAdapter} options={callOptions} />
          </div>
        </Modal>
      </div>
    );
  }
  return (
    <div>
      <LoadingSpinner />
    </div>
  );
};

export default VideoWindow;
