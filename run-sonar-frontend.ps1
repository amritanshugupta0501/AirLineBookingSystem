param (
    [Parameter(Mandatory=$true)]
    [string]$SonarToken
)

$FrontendDir = Join-Path -Path $PSScriptRoot -ChildPath "airline-frontend"

Write-Host "Starting SonarQube Scanner for Frontend using Docker..."

docker run --rm `
    -e SONAR_HOST_URL="http://host.docker.internal:9000" `
    -e SONAR_LOGIN="$SonarToken" `
    -v "$($FrontendDir):/usr/src" `
    sonarsource/sonar-scanner-cli

Write-Host "Frontend SonarQube scan complete!"
