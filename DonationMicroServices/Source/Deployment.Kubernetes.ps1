[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
	[ValidateSet('deployToProd', 'deleteDeployments', 'getInfo', 'getLogs')]
    [string]$action = "getInfo", 

    [Parameter(Mandatory=$false)] # The appName is used as the kubernete deployment/service name and cannot contains '.'
    [string]$appName = "donation.restapi.entrance", #  fdotnetcorewebapp, donation-restapi-entrance

    [Parameter(Mandatory=$false)]
    [string]$appVersion = "1.0.4", #  1.0.3

    [Parameter(Mandatory=$false)]
    [string]$appUrl = "/api/info/getinfo",

    [Parameter(Mandatory=$false)] 
    [bool]$deployService = $true,

    [Parameter(Mandatory=$false)] 
    [bool]$waitForStatefullsets = $false,

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
    [bool]$traceKubernetesCommand = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$APP_ENDPOINT_IP,

    [Parameter(Mandatory=$false)]
    [string]$APP_ENDPOINT_PORT
)

# https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-aks
#az acr show --name $acrName --resource-group $myResourceGroup --query "id" --output tsv
#az acr show --name $acrName --query loginServer --output tsv
$appName = $appName.Replace(".", "-")

if($null -eq (Get-Module Util)) {

    Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\Util.psm1" -Force
}
Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\KubernetesManager.psm1" -Force

function getKubernetesYamlFile($postFixFileName) {

    return "$(getCurrentScriptPath)\Kubernetes.Templates\$appName--$postFixFileName" 
}

function getAppMetadatJsonFileName() {

    return [System.IO.Path]::Combine($env:TEMP, "$($appName.replace(".","-")).json")
}

function deployRelease([Hashtable]$context, [string]$message, [bool]$deployService) {

    Write-HostColor $message DarkYellow

    # Deploy the web app $appName from docker image on x pods
    $processedFile = processFile $context (getKubernetesYamlFile("Deployment.{Params}.yaml")) # TODO:Script
    $json = $kubernetesManager.create($processedFile, $true)
    $kubernetesManager.printKubectlOutpuResourceInfos($json)
    #$kubernetesManager.waitForDeployment($deploymentName)

    if($waitForStatefullsets) {
        # $kubernetesManager.waitForStatefullsets() # not implemented
    }

    if($deployService) {

        # Deploy service/loadBalancer for the x pods    
        $processedFile = processFile $context (getKubernetesYamlFile("Service.{Params}.yaml"))
        $serviceName = $kubernetesManager.createService($processedFile)
        $kubernetesManager.waitForService($serviceName)
        
        $waitTime = 30
        Write-HostColor "Waiting $waitTime seconds before testing the web site home url"
        Start-Sleep -s $waitTime # I noticed that it takes some time for the machine to be ready

        # Retreive ip + port and verify home url
        $loadBlancerIp = $kubernetesManager.GetServiceLoadBalancerIP($serviceName)
        $loadBlancerPort = $kubernetesManager.GetServiceLoadBalancerPort($serviceName)
        Write-HostColor "LoadBalancer Ip:$($loadBlancerIp), port:$($loadBlancerPort)" DarkYellow

        updateJsonFileWithProperty (getAppMetadatJsonFileName) "EndPointIP" $loadBlancerIp
        updateJsonFileWithProperty (getAppMetadatJsonFileName) "EndPointPort" $loadBlancerPort

        $testUrl = "http://$loadBlancerIp`:$loadBlancerPort$($context.TEST_URL)"
        validateUrl $testUrl
    }
}


function deleteRelease([Hashtable]$context, [string]$message, [bool]$deployService) {

    Write-HostColor $message DarkYellow

    # Deploy the web app $appName from docker image on x pods
    $processedFile = processFile $context (getKubernetesYamlFile("Deployment.{Params}.yaml")) # TODO:Script
    $kubernetesManager.delete($processedFile)

    if($deployService) {

        # Deploy service/loadBalancer for the x pods    
        $processedFile = processFile $context (getKubernetesYamlFile("Service.{Params}.yaml"))
        $kubernetesManager.delete($processedFile)        
    }
}

if($clearScreen) {
    cls
}
else {
    Write-Host "" 
}

Write-Host "Deployment.Kubernetes " -ForegroundColor Yellow -NoNewline
Write-HostColor "action:$action, appName:$appName - $appVersion" DarkYellow

# For now pick the first cluster available
$kubernetesManager = GetKubernetesManagerInstance $acrName $acrLoginServer $azureContainerRegistryPassword ($action -eq "deployToProd") $traceKubernetesCommand

$context = @{ ENVIRONMENT = "prod"; APP_VERSION = $appVersion; APP_NAME = $appName; TEST_URL = $appUrl; APP_ENDPOINT_IP = $APP_ENDPOINT_IP; APP_ENDPOINT_PORT = $APP_ENDPOINT_PORT }


switch($action) {

    deployToProd {
        deployRelease $context "`r`n*** Deploy initial version v$($context.APP_VERSION) to $($context.ENVIRONMENT) ***" $deployService
    }
    getInfo {
        $deploymentName = "$appName-deployment-$appVersion"
        Write-HostColor $kubernetesManager.getForDeploymentInformation($deploymentName)

        $serviceName = "$appName-service-prod"
        Write-HostColor $kubernetesManager.getForServiceInformation($serviceName)
    }
    deleteDeployments {

        deleteRelease $context "`r`n*** Delete version v$($context.APP_VERSION) to $($context.ENVIRONMENT) ***" $deployService
    }
    getLogs {

        Write-Host "getLogs"
        $podNames = $kubernetesManager.getPodNames()
        $kubernetesManager.writeLogs($podNames)
    }
}

Write-Host "Deployment.Kubernetes done" -ForegroundColor DarkYellow
