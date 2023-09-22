
param(
[string] $AZURE_STORAGE_BLOB_CONNSTRING,
[string] $AZURE_STORAGE_BLOB_CONTAINER,
[string] $AZURE_SEARCH_SERVICE_ENDPOINT,
[string] $AZURE_SEARCH_SERVICE_ENDPOINT_KEY,
[string] $AZURE_SEARCH_INDEX,
[string] $AZURE_AI_SERVICE_ENDPOINT,
[string] $AZURE_AI_SERVICE_KEY
)


#if ([string]::IsNullOrEmpty($env:AZD_PREPDOCS_RAN) -or $env:AZD_PREPDOCS_RAN -eq "false") {
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

    #[Environment]::SetEnvironmentVariable('AZD_PREPDOCS_RAN', 'true', [System.EnvironmentVariableTarget]::User)
#} else {
   # Write-Host "AZD_PREPDOCS_RAN is set to true. Skipping the run."
#}
