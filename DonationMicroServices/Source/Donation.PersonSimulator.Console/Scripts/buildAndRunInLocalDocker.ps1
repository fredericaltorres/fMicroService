function Write-Host-Color([string]$message, $color = "Cyan") {
    Write-Host ""
    Write-Host $message -ForegroundColor $color
}

Write-Host "buildAndRunInLocalDocker" -ForegroundColor Yellow

Write-Host-Color "Building .net project"
dotnet publish -c Release

Write-Host-Color "Building Docker image"
docker build -t azureservicebuspubconsole .

# Write-Host-Color "Creating Container Instance..."
# $containerInstanceId = docker create fcoreconsoleazurestorage
# Write-Host-Color "containerInstanceId: $containerInstanceId "

# Write-Host-Color "Starting Container Instance..."
# docker start $containerInstanceId 

# Write-Host-Color "Accessing Logs from Container Instance."
# docker logs $containerInstanceId 
