Write-Output "************************"
Write-Output "Building from powershell"
Write-Output "************************"

Set-Location ".\DonationMicroServices\Source"

function restoreFile([string]$file, [string]$sourceFile) {
    if(-not (Test-Path $file)) {
        Write-Output "Restore file $file"
        copy $sourceFile $file
    }
}
# Restorer appsettings.json file for Donation.PersonSimulator.Console
$sourceFile = "C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.PersonSimulator.Console\appsettings.json"
$file = ".\Donation.PersonSimulator.Console\appsettings.json"
restoreFile $file $sourceFile

# Restorer appsettings.json file for Donation.RestApi.Entrance
$sourceFile = "C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.RestApi.Entrance\appsettings.json"
$file = ".\Donation.RestApi.Entrance\appsettings.json"
restoreFile $file $sourceFile

# Restorer appsettings.json file for Donation.RestApi.Entrance
$sourceFile = "C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.QueueProcessor.Console\appsettings.json"
$file = ".\Donation.QueueProcessor.Console\appsettings.json"
restoreFile $file $sourceFile

.\DeploymentUtilityMaster.ps1 -a build -app Donation.PersonSimulator.Console
