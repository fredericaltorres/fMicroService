[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
	[ValidateSet('initialDeploymentToProd', 'deleteDeployments', 'deployToStaging', 'switchStagingToProd', 'revertProdToPrevious', 'getInfo')]
    [string]$action = "deleteDeployments", 

    [Parameter(Mandatory=$false)]
    [string]$appName = "donation-restapi-entrance",#  fdotnetcorewebapp   

     # Fred Azure Container Registry Information
    [Parameter(Mandatory=$false)]
    [string]$acrName = "FredContainerRegistry", # Consider that the Azure Container `FredContainerRegistry` already exist
    [Parameter(Mandatory=$false)]
    [string]$myResourceGroup = "FredContainerRegistryResourceGroup",
    [Parameter(Mandatory=$false)] # The full login server name for your Azure container registry.  az acr show --name $acrName --query loginServer --output table
    [string]$acrLoginServer = "fredcontainerregistry.azurecr.io",

    [Parameter(Mandatory=$false)] # The Azure Container Registry has default username which is the name of the registry, but there is a password required when pushing a image
    [string]$azureContainerRegistryPassword = $env:azureContainerRegistryPassword,
	
    [Parameter(Mandatory=$false)] 
	[Alias('cls')]
    [bool]$clearScreen = $true
)

# https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-aks
#az acr show --name $acrName --resource-group $myResourceGroup --query "id" --output tsv
#az acr show --name $acrName --query loginServer --output tsv

Import-Module ".\Util.psm1" -Force
Import-Module ".\KubernetesManager.psm1" -Force

function deployRelease($context, $message) {

    Write-HostColor $message DarkYellow

    # Deploy the web app $appName from docker image on 3 pods
    $processedFile = processFile $context ".\Templates\Deployment.{Params}.yaml"
    $deploymentName = $kubernetesManager.createDeployment($processedFile)
    $kubernetesManager.waitForDeployment($deploymentName)

    # Deploy service/loadBalancer for the 3 pods
    $processedFile = processFile $context ".\Templates\Service.{Params}.yaml"
    $serviceName = $kubernetesManager.createService($processedFile)
    $kubernetesManager.waitForService($serviceName)
    
    $waitTime = 30
    Write-HostColor "Waiting $waitTime seconds before testing the web site home url"
    Start-Sleep -s $waitTime # I noticed that it takes some time for the machine to be ready

    # Retreive ip + port and verify home url
    $loadBlancerIp = $kubernetesManager.GetServiceLoadBalancerIP($serviceName)
    $loadBlancerPort = $kubernetesManager.GetServiceLoadBalancerPort($serviceName)
    Write-HostColor "LoadBalancer Ip:$($loadBlancerIp), port:$($loadBlancerPort)" DarkYellow
    urlMustReturnHtml "http://$loadBlancerIp`:$loadBlancerPort"
}

function switchProductionToVersion($context, $message) {

    Write-HostColor $message DarkYellow

    $processedFile = processFile $context ".\Templates\Service.{Params}.yaml"
    $serviceName = $kubernetesManager.applyService($processedFile)
    $kubernetesManager.waitForService($serviceName)
}


if($clearScreen) {
    cls
}
else {
    Write-Host "" 
}

Write-Host "BlueGreenDeployment.Kubernetes " -ForegroundColor Yellow -NoNewline
Write-HostColor "-action:$action" DarkYellow

# For now pick the first cluster available
$kubernetesManager = GetKubernetesManagerInstance $acrName $acrLoginServer $azureContainerRegistryPassword ($action -eq "initialDeploymentToProd")


switch($action) {

    initialDeploymentToProd { 

        $context = @{ ENVIRONMENT = "prod"; APP_VERSION = "1.0.1" }
        deployRelease $context "`r`n*** Deploy initial version v$($context.APP_VERSION) to $($context.ENVIRONMENT) ***"
    }

    deployToStaging {

        $context = @{ ENVIRONMENT = "staging"; APP_VERSION = "1.0.4" }
        deployRelease $context "`r`n*** Deploy version v$($context.APP_VERSION) to $($context.ENVIRONMENT) ***"
    }

    switchStagingToProd {

        # Make the production service/lodBalancer from prod point to the pods of the new version
        $context = @{ ENVIRONMENT = "prod"; APP_VERSION = "1.0.4" }
        switchProductionToVersion $context "`r`n*** Switch $($context.ENVIRONMENT) to version v$($context.APP_VERSION) ***"
    }

    revertProdToPrevious {
        $context = @{ ENVIRONMENT = "prod"; APP_VERSION = "1.0.1" }
        switchProductionToVersion $context "`r`n*** Revert $($context.ENVIRONMENT) to version v$($context.APP_VERSION) ***"
    }

    getInfo {

        $deploymentName = "$appName-deployment-1.0.1"
        Write-HostColor $kubernetesManager.getForDeploymentInformation($deploymentName)

        $serviceName = "$appName-service-prod"
        Write-HostColor $kubernetesManager.getForServiceInformation($serviceName)

        $deploymentName = "$appName-deployment-1.0.4"
        Write-HostColor $kubernetesManager.getForDeploymentInformation($deploymentName)

        $serviceName = "$appName-service-staging"
        Write-HostColor $kubernetesManager.getForServiceInformation($serviceName)
    }

    deleteDeployments {

        Write-Host "Delete all deployments"
        
        $deploymentName = "$appName-deployment-1.0.1"
        $kubernetesManager.deleteDeployment($deploymentName)

        $serviceName = "$appName-service-prod"
        $kubernetesManager.deleteService($serviceName)

        $deploymentName = "$appName-deployment-1.0.3"
        $kubernetesManager.deleteDeployment($deploymentName)

        $serviceName = "$appName-service-staging"
        $kubernetesManager.deleteService($serviceName)
    }
}

Write-Host "Done" -ForegroundColor DarkYellow
