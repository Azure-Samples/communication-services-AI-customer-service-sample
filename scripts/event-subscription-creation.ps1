
param(
[string] $resourceGroupName,
[string] $subscriptionId,
[string] $communicationServiceName,
[string] $hostUrl

)

$webhookEndpoint ="$hostUrl/api/events"

# Create an Event Grid subscription
az eventgrid event-subscription create `
  --name 'acssupportchat1' `
  --endpoint-type webhook `
  --endpoint $webhookEndpoint `
  --event-delivery-schema EventGridSchema `
  --included-event-types Microsoft.Communication.ChatMessageReceivedInThread Microsoft.Communication.RouterWorkerOfferIssued Microsoft.Communication.RouterWorkerOfferAccepted Microsoft.Communication.CallEnded `
  --source-resource-id "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Communication/CommunicationServices/$communicationServiceName" `

