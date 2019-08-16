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

$sourceFile = "C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.PersonSimulator.Console\appsettings.json"
$file = ".\Donation.PersonSimulator.Console\appsettings.json"
restoreFile $file $sourceFile

.\DeploymentUtilityMaster.ps1 -a build -app Donation.PersonSimulator.Console
