[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [Alias('a')]
    [ValidateSet('build', 'push','buildAndPush','buildPushAndDeploy','deploy','deleteDeployment','getLogs', 'initData', 'info')]
    [string]$action = "initData",

    [Parameter(Mandatory=$false)]
    [ValidateSet('all','Donation.QueueProcessor.Console','Donation.RestApi.Entrance','Donation.PersonSimulator.Console')]
    [string]$app = "all"
)

Write-Output "************************"
Write-Output "Building from powershell - action:$action, app :$app "
Write-Output "************************"

Set-Location ".\DonationMicroServices\Source"

function restoreFile([string]$file, [string]$sourceFile) {
    if(-not (Test-Path $file)) {
        Write-Output "Restore file $file"
        Copy-Item $sourceFile $file
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

.\DeploymentUtilityMaster.ps1 -a $action -app $app

