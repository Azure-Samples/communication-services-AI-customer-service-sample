#!/usr/bin/env bash

echo 'Running "PrepareDocs.dll"'

pwd

dotnet run --project "app/prepdocs/PrepareDocs/PrepareDocs.csproj" -- \
    './data/*.pdf' \
    --storageconnstring "$AZURE_STORAGE_ACCOUNT_CONNECTIONSTRING" \
    --container "$AZURE_STORAGE_CONTAINER" \
    --searchendpoint "$AZURE_SEARCH_SERVICE_ENDPOINT" \
    --searchkey  "$AZURE_SEARCH_SERVICE_KEY" \
    --searchindex "$AZURE_SEARCH_INDEX" \
    --formrecognizerendpoint "$AZURE_AI_SERVICE_ENDPOINT" \
    --formrecognizerkey "$AZURE_AI_SERVICE_KEY" \
    -v
