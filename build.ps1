Write-Output "building from powershell"

Set-Location ".\DonationMicroServices\Source"

$sourceFile = "C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.PersonSimulator.Console\appsettings.json"
$file = ".\Donation.PersonSimulator.Console\appsettings.json"
if(!Test-Path $file) {
    Write-Output "Restore file $file"
    copy $sourceFile $file
}


.\DeploymentUtilityMaster.ps1 -a build -app Donation.PersonSimulator.Console
