#!/usr/bin/env bash

# Use Your existing variables

echo "Resource Group Name: $AZURE_RESOURCE_GROUP"
echo "Subscription ID: $subscriptionId"
echo "Communication Service Name: $ACS_SERVICE_NAME"
echo "Host URL: $API_ENDPOINT"

# Create an Event Grid subscription using Azure CLI
az eventgrid event-subscription create \
  --name 'acssupportchat1' \
  --endpoint-type webhook \
  --endpoint "$API_ENDPOINT/api/events" \
  --event-delivery-schema EventGridSchema \
  --included-event-types Microsoft.Communication.ChatMessageReceivedInThread Microsoft.Communication.RouterWorkerOfferIssued Microsoft.Communication.RouterWorkerOfferAccepted Microsoft.Communication.CallEnded \
  --source-resource-id "/subscriptions/$subscriptionId/resourceGroups/$AZURE_RESOURCE_GROUP/providers/Microsoft.Communication/CommunicationServices/$ACS_SERVICE_NAME"
