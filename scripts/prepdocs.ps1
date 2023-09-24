
param(
[string] $AZURE_STORAGE_BLOB_CONNSTRING,
[string] $AZURE_STORAGE_BLOB_CONTAINER,
[string] $AZURE_SEARCH_SERVICE_ENDPOINT,
[string] $AZURE_SEARCH_SERVICE_ENDPOINT_KEY,
[string] $AZURE_SEARCH_INDEX,
[string] $AZURE_AI_SERVICE_ENDPOINT,
[string] $AZURE_AI_SERVICE_KEY
)


Write-Host 'Running "PrepareDocs.dll"'

Get-Location | Select-Object -ExpandProperty Path

dotnet run --project "app/prepdocs/PrepareDocs/PrepareDocs.csproj" -- `
    './data/*.pdf' `
    --storageconnstring $AZURE_STORAGE_BLOB_CONNSTRING `
    --container $AZURE_STORAGE_BLOB_CONTAINER `
    --searchendpoint $AZURE_SEARCH_SERVICE_ENDPOINT `
    --searchkey $AZURE_SEARCH_SERVICE_ENDPOINT_KEY `
    --searchindex $AZURE_SEARCH_INDEX `
    --formrecognizerendpoint $AZURE_AI_SERVICE_ENDPOINT `
    --formrecognizerkey $AZURE_AI_SERVICE_KEY `
    -v
