#!/usr/bin/env bash

if ! command -v jq &> /dev/null
then
    echo "the command jq cannot be found. Please install jq to run this script"
    exit 1
fi

if ! command -v zip &> /dev/null
then
    echo "the command zip cannot be found. Please install zip to run this script"
    exit 1
fi

if ! command -v npm &> /dev/null
then
    echo "the command npm cannot be found. Please install npm to run this script"
    exit 1
fi

if [ $? -eq 0 ]; then
    echo "Already logged in."
else
    # If not logged in, log in
    az login
    echo "Logged in successfully."
fi

# Check if the user is logged in or not
az account show > /dev/null 2>&1

# give your subscriptionId and press enter and give preffered location to create Azure resources
read -p "Please enter the subscription id or name: " subscriptionName
read -p "Please enter the region for Azure resources creation: " location
read -p "Please enter the environment name: " environmentName

set -e
az account set --subscription "$subscriptionName"
export subscriptionId=$(az account show --query id -o tsv)
echo $subscriptionId

mainBicepPath="./infra"
scriptsPath="./scripts"
appPath="./app"

echo "Running az deployment... "
echo "Creating resources to resourcegroup rg-$environmentName"

result=($(az deployment sub create \
        --location $location \
        --name bicepDeployment1 \
        --template-file $mainBicepPath/main.bicep \
        --parameters $mainBicepPath/main.parameters.json \
        --parameters environmentName=$environmentName location=$location \
        --query "[ \
            properties.outputs.azurE_LOCATION.value, \
            properties.outputs.azurE_OPENAI_DEPLOYMENT_NAME.value, \
            properties.outputs.azurE_STORAGE_CONTAINER.value, \
            properties.outputs.azurE_STORAGE_ACCOUNT_CONNECTIONSTRING.value, \
            properties.outputs.azurE_SEARCH_SERVICE_KEY.value, \
            properties.outputs.azurE_SEARCH_INDEX.value, \
            properties.outputs.azurE_SEARCH_SERVICE_ENDPOINT.value, \
            properties.outputs.azurE_AI_SERVICE_ENDPOINT.value, \
            properties.outputs.azurE_AI_SERVICE_KEY.value, \
            properties.outputs.acS_CONNECTIONSTRING.value, \
            properties.outputs.acS_ENDPOINT.value, \
            properties.outputs.acS_SERVICE_NAME.value, \
            properties.outputs.apI_ENDPOINT.value, \
            properties.outputs.apI_SERVICE_NAME.value, \
            properties.outputs.weB_ENDPOINT.value, \
            properties.outputs.weB_SERVICE_NAME.value, \
            properties.outputs.azurE_RESOURCE_GROUP.value \
            ]" -o tsv))

# echo "result output properties: ${result[@]}"

export AZURE_STORAGE_CONTAINER="${result[2]}"
export AZURE_STORAGE_ACCOUNT_CONNECTIONSTRING="${result[3]}"
export AZURE_SEARCH_SERVICE_KEY="${result[4]}"
export AZURE_SEARCH_INDEX="${result[5]}"
export AZURE_SEARCH_SERVICE_ENDPOINT="${result[6]}"
export AZURE_AI_SERVICE_ENDPOINT="${result[7]}"
export AZURE_AI_SERVICE_KEY="${result[8]}"
export ACS_CONNECTIONSTRING="${result[9]}"
export ACS_ENDPOINT="${result[10]}"
export ACS_SERVICE_NAME="${result[11]}"
export API_ENDPOINT="${result[12]}"
export API_SERVICE_NAME="${result[13]}"
export WEB_ENDPOINT="${result[14]}"
export WEB_SERVICE_NAME="${result[15]}"
export AZURE_RESOURCE_GROUP="${result[16]}"

#----------prepare documents to search index ------------#
./scripts/prepdocs.sh

#----------deploy frontend application to azure appservice------------#
./scripts/deploy-frontend.sh 

#----------deploy backend application to azure appservice------------#
./scripts/deploy-backend.sh 

#----------create event subscription------------#
./scripts/event-subscription-creation.sh
