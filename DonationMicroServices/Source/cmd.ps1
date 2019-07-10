cls
Write-Host "Remove current system and data"
# .\DeploymentUtilityMaster.ps1 -a deleteDeployment -app all

.\DeploymentUtilityMaster.ps1 -a deleteDeployment -app Donation.RestApi.Entrance
.\DeploymentUtilityMaster.ps1 -a deleteDeployment -app Donation.PersonSimulator.Console
.\DeploymentUtilityMaster.ps1 -a deleteDeployment -app Donation.QueueProcessor.Console
.\DeploymentUtilityMaster.ps1 -a initData
"Done"



pause 

Write-Host "Deploying..."
.\DeploymentUtilityMaster.ps1 -a deploy -app Donation.RestApi.Entrance
Start-Sleep -s 30
.\DeploymentUtilityMaster.ps1 -a deploy -app Donation.QueueProcessor.Console
# Start-Sleep -s 30
.\DeploymentUtilityMaster.ps1 -a deploy -app Donation.PersonSimulator.Console

"Done"






