[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
    [ValidateSet('build', 'push','Nothing','deployDonationRestApiEntrace')]
    [string]$action = "deployDonationRestApiEntrace"
)

if($null -eq (Get-Module Util)) {
    Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\..\Util.psm1" -Force
}

$dockerFilName = ".\Source\Donation.RestApi.Entrance\Dockerfile"
$appName = GetAppNameFromProject
$containerImageName = $appName
$appVersion = GetProjectVersion

Write-HostColor "Donation Automation Deployment Utility" Yellow
Write-Host "$action appName:$($appName) - $($appVersion), containerImageName:$containerImageName" -ForegroundColor DarkYellow

switch($action) {
    build { # Build the continer
        Write-Host "First go up to folder before building container" -ForegroundColor DarkGray
        pushd
        cd ..\..
        docker build -t "$containerImageName"-f "$dockerFilName" .
        popd
    }
    push {
        ..\deployContainerToAzureContainerRegistry.ps1 -a push -containerImage "$containerImageName" -cls $false
    }
    deployDonationRestApiEntrace {
        ..\Deployment.Kubernetes.ps1 -a deployToProd -appName $appName -appVersion $appVersion -cls $false
    }
}

<#
Run locally on container
    docker run -d -p 80:80 --name donation.restapi.entrance donation.restapi.entrance
# Does not work    
    docker run -d -p 443:443 --name donation.restapi.entrance donation.restapi.entrance

https://zimmergren.net/azure-container-instances-dotnet-core-api-application-gateway-https/

Deployment into a Kubernetes
https://docs.microsoft.com/en-us/dotnet/standard/containerized-lifecycle-architecture/design-develop-containerized-apps/build-aspnet-core-applications-linux-containers-aks-kubernetes

# Deploy in ACR    
     ../deployContainerToAzureContainerRegistry.ps1 -a push -containerImage donation.restapi.entrance
     
     # https://www.youtube.com/watch?v=q8nXv56gWms
     ../deployContainerToAzureContainerRegistry.ps1 -a createAppService -containerImage donation.restapi.entrance
     # https://donation-restapi-entrance.azurewebsites.net
     ../deployContainerToAzureContainerRegistry.ps1 -a createWebApp -containerImage donation.restapi.entrance
     ../deployContainerToAzureContainerRegistry.ps1 -a configContainerWebApp -containerImage donation.restapi.entrance

     https://donation-restapi-entrance.azurewebsites.net/api/Info
     https://donation-restapi-entrance.azurewebsites.net:443/api/Donation

     ../deployContainerToAzureContainerRegistry.ps1 -a instantiate -containerImage donation.restapi.entrance
     ../deployContainerToAzureContainerRegistry.ps1 -a deleteInstance -containerImage donation.restapi.entrance
  http://donation-restapi-entrance-instance-0.eastus.azurecontainer.io:8080/api/info   
  http://10.0.6.215:8080/api/info
#>
