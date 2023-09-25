#!/bin/sh

if [ -z "$AZD_PREPDOCS_RAN" ] || [ "$AZD_PREPDOCS_RAN" = "false" ]; then
    echo 'Running "PrepareDocs.dll"'

    pwd

    dotnet run --project "app/prepdocs/PrepareDocs/PrepareDocs.csproj" -- \
      './data/*.pdf' \
      --storageconnstring "$AZURE_STORAGE_BLOB_CONNSTRING" \
      --container "$AZURE_STORAGE_BLOB_CONTAINER" \
      --searchendpoint "$AZURE_SEARCH_SERVICE_ENDPOINT" \
      --searchkey  "$AZURE_SEARCH_SERVICE_ENDPOINT_KEY" \
      --searchindex "$AZURE_SEARCH_INDEX" \
      --formrecognizerendpoint "$AZURE_AI_SERVICE_ENDPOINT" \
      --formrecognizerkey "$AZURE_AI_SERVICE_KEY" \
      -v

    azd env set AZD_PREPDOCS_RAN "true"
else
    echo "AZD_PREPDOCS_RAN is set to true. Skipping the run."
fi
