param (
    [Parameter(Mandatory=$true)]
    [string]$SonarToken
)

# Install the SonarScanner tool globally if it's not already installed
Write-Host "Ensuring dotnet-sonarscanner is installed..."
dotnet tool install --global dotnet-sonarscanner

# Start the SonarScanner
Write-Host "Starting SonarQube Scanner for Backend..."
dotnet sonarscanner begin /k:"AirlineBookingSystem_Backend" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="$SonarToken"

# Build the solution
Write-Host "Building the Solution..."
dotnet build AirlineBookingSystem.sln

# End the SonarScanner and upload results
Write-Host "Finishing scan and uploading results to SonarQube..."
dotnet sonarscanner end /d:sonar.login="$SonarToken"

Write-Host "Backend SonarQube scan complete!"
