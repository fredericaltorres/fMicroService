cls
Write-Host "Remove current system and data"
.\DeploymentUtilityAll.ps1 -a deleteDeployment -app all
.\DeploymentUtilityAll.ps1 -a initData

pause 

Write-Host "Deploying..."
.\DeploymentUtilityAll.ps1 -a deploy -app Donation.RestApi.Entrance
Start-Sleep -s 30
.\DeploymentUtilityAll.ps1 -a deploy -app Donation.PersonSimulator.Console

