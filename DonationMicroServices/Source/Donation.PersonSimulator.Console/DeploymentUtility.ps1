[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
    [ValidateSet('build', 'push','buildAndPush','buildPushAndDeploy','deploy','deleteDeployment','getLogs')]
    [string]$action = "deploy"
)

if($null -eq (Get-Module Util)) {
    Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\..\Util.psm1" -Force
}

$appName = GetAppNameFromProject
$dockerFilName = ".\Source\$appName\Dockerfile"
$containerImageName = $appName
$appVersion = GetProjectVersion
$scriptTitle = "Donation Automation Deployment Utility -- $appName"
$traceKubernetesCommand = $true
$deployService = $false
Write-HostColor "$scriptTitle" Yellow
Write-Host "$action appName:$($appName) - $($appVersion), containerImageName:$containerImageName" -ForegroundColor DarkYellow

function buildContainer() {
    Write-Host "First go up to folder before building container" -ForegroundColor DarkGray
    pushd
    cd ..\..
    docker build -t "$containerImageName"-f "$dockerFilName" .
    popd
}
function pushContainerImageToRegistry() {
    ..\deployContainerToAzureContainerRegistry.ps1 -a push -containerImage "$containerImageName" -cls $false
}
function deploy() {
    ..\Deployment.Kubernetes.ps1 -a deployToProd -appName $appName -appVersion $appVersion -cls $false -traceKubernetesCommand $traceKubernetesCommand -deployService $deployService
}
switch($action) {
    build {
        buildContainer
    }
    push {
        pushContainerImageToRegistry
    }
    buildAndPush {
        buildContainer
        pushContainerImageToRegistry
    }
    deploy { # Deploy rest api service on 2 pod and loadBalancer
        deploy
    }
    buildPushAndDeploy {
        buildContainer
        pushContainerImageToRegistry
        deploy
    }
    deleteDeployment { # Delete deployment of rest api service and loadBalancer
        ..\Deployment.Kubernetes.ps1 -a deleteDeployments -appName $appName -appVersion $appVersion -cls $false -traceKubernetesCommand $traceKubernetesCommand  -deployService $false
    }
    getLogs { 
        ..\Deployment.Kubernetes.ps1 -a getLogs -appName $appName -appVersion $appVersion -cls $false -traceKubernetesCommand $traceKubernetesCommand  -deployService $false
    }
}
Write-HostColor "$scriptTitle done" Yellow

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
