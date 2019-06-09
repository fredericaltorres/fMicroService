[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
	[ValidateSet('deployToProd', 'deleteDeployments', 'getInfo')]
    [string]$action = "getInfo", 

    [Parameter(Mandatory=$false)]
    [string]$appName = "donation-restapi-entrance", #  fdotnetcorewebapp, donation-restapi-entrance

    [Parameter(Mandatory=$false)]
    [string]$appVersion = "1.0.4", #  1.0.3

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
    [bool]$clearScreen = $true,

    [Parameter(Mandatory=$false)] 
    [bool]$traceKubernetesCommand = $false
)

# https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-aks
#az acr show --name $acrName --resource-group $myResourceGroup --query "id" --output tsv
#az acr show --name $acrName --query loginServer --output tsv

Write-Host "(Deployment.Kubernetes)path to Util.psm1:$PSScriptRoot\Util.psm1"

if($null -eq (Get-Module Util)) {
    Import-Module "$PSScriptRoot\Util.psm1" -Force
}

Import-Module "$PSScriptRoot\KubernetesManager.psm1" -Force

function deployRelease([Hashtable]$context, [string]$message) {

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
    $testUrl = "http://$loadBlancerIp`:$loadBlancerPort$($context.TEST_URL)"
    urlMustReturnHtml $testUrl
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

Write-Host "Deployment.Kubernetes " -ForegroundColor Yellow -NoNewline
Write-HostColor "-action:$action" DarkYellow

# For now pick the first cluster available
$kubernetesManager = GetKubernetesManagerInstance $acrName $acrLoginServer $azureContainerRegistryPassword ($action -eq "initialDeploymentToProd") $traceKubernetesCommand

switch($action) {

    deployToProd {

        $context = @{ ENVIRONMENT = "prod"; APP_VERSION = $appVersion; TEST_URL = "/api/info" }
        deployRelease $context "`r`n*** Deploy initial version v$($context.APP_VERSION) to $($context.ENVIRONMENT) ***"
    }

    getInfo {

        $deploymentName = "$appName-deployment-$appVersion"
        Write-HostColor $kubernetesManager.getForDeploymentInformation($deploymentName)

        $serviceName = "$appName-service-prod"
        Write-HostColor $kubernetesManager.getForServiceInformation($serviceName)
    }

    deleteDeployments {

        Write-Host "Delete all deployments"
        
        $deploymentName = "$appName-deployment-$appVersion"
        $kubernetesManager.deleteDeployment($deploymentName)

        $serviceName = "$appName-service-prod"
        $kubernetesManager.deleteService($serviceName)
    }
}

Write-Host "Deployment.Kubernetes done" -ForegroundColor DarkYellow
