// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Events
{
    public class CallEndedEvent
    {
        public StartedBy startedBy { get; set; }

        public string serverCallId { get; set; }

        public Group group { get; set; }

        public bool isTwoParty { get; set; }

        public string correlationId { get; set; }

        public bool isRoomsCall { get; set; }
    }

    public class Group
    {
        public string id { get; set; }
    }

    public class StartedBy
    {
        public CommunicationIdentifier communicationIdentifier { get; set; }

        public string role { get; set; }
    }

    public class CommunicationIdentifier
    {
        public string rawId { get; set; }

        public CommunicationUser communicationUser { get; set; }
    }

    public class CommunicationUser
    {
        public string id { get; set; }
    }
}
