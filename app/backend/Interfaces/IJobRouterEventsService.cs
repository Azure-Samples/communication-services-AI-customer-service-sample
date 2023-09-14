// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IJobRouterEventsService
    {
        Task HandleEvent(OfferIssuedEvent offerIssuedEvent);

        Task HandleEvent(OfferAcceptedEvent offerAcceptedEvent);
    }
}