Write-Output "building from powershell"

Set-Location ".\DonationMicroServices\Source"
.\DeploymentUtilityMaster.ps1 -a build -app Donation.PersonSimulator.Console
