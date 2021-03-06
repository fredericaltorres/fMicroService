﻿<#
    .SYNOPSIS
        Handle building .NET Core application and executing it as a Docker container in Azure
    .DESCRIPTION
        Build .NET project, 
        Create docker container image, 
        Push container image in Azure Container Registry
        Instanciate one or more container instance in Azure Container Instance
        Get log from one or more container instance running in Azure Container Instance
        Stop and delete one or more container instance running in Azure Container Instance
    .NOTES
        This script uses 
        - Azure Container Registry to store container image
        - Azure Container Instance to execute container instances
        - Azure CLI (Command Line Interface)
        - Does not use Kubenetes.
        Frederic Torres 2019
#>
[CmdletBinding()]
param(
    # Action to execute
    [Parameter(Mandatory=$true)] 
    [Alias('a')]
    [ValidateSet('build', 'push', 'instantiate', 'deleteInstance', 'getLog', 
                'createAppService', 'createWebApp', 'configContainerWebApp')]
    [string]$action,

    # Ths action 'instantiate', 'deleteInstance', 'getLog' may apply to a specific container instance.    
    # The container instance is named using the $containerInstanceName + $containerInstanceIndex.
    # This way the script can instanciate and delete more than one container instance.
    [Parameter(Mandatory=$false)]
    [int]$containerInstanceIndex = 0,

    # The container image name contained in the Azure Container Registry
    [Parameter(Mandatory=$true)]
    [string]$containerImage,    

    # The name of the container instance, if not passed the name is set to the $containerInstanceName + $containerInstanceIndex.
    [Parameter(Mandatory=$false)]
    $containerInstanceName = "",

    # Azure Container Registry Information - The registry must be created manually using the Azure Portal
    [Parameter(Mandatory=$false)]
    [string]$acrName = "FredContainerRegistry",
    [Parameter(Mandatory=$false)]
    [string]$myResourceGroup = "FredContainerRegistryResourceGroup",
    [Parameter(Mandatory=$false)] # The full login server name for your Azure container registry.  az acr show --name $acrName --query loginServer --output table
    [string]$acrLoginServer = "fredcontainerregistry.azurecr.io",
    [Parameter(Mandatory=$false)]
    [string]$azureContainerRegistryPassword = $env:azureContainerRegistryPassword,

    # Container Hardware Configuration, for more hardware configuration search for:az container create
    [Parameter(Mandatory=$false)] [int]$containerInstanceCpu	= 1,
    [Parameter(Mandatory=$false)] [int]$containerInstanceMemory = 1,

    # Default HTTP port to use - NOT USED YET
    [Parameter(Mandatory=$false)] [int]$containerInstancePort	= 8080,

    # Clear the scren by default
    [Parameter(Mandatory=$false)]
    [Alias('cls')]
    [bool]$clearScreen = $true
)

if($null -eq (Get-Module Util)) {
    Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\Util.psm1" -Force
}

$containerImage = $containerImage.toLower()
if($containerInstanceName -eq "") { # If container name instance is empty use the name of the container image + ".instance"
    $containerInstanceName = $containerImage + ".instance"
}
$containerInstanceName += "-$containerInstanceIndex" # always post fix the container instance name with an index to allow to handle multiple container instance
$containerInstanceName = $containerInstanceName.replace(".", "-").toLower() # Required by Azure

if($azureContainerRegistryPassword -eq $null -or $azureContainerRegistryPassword.Length -eq 0) {
    throw "Parameter `$azureContainerRegistryPassword is required"
}

$appName                = GetAppNameFromProject
$dockerFilName          = ".\Source\$appName\Dockerfile"

function GetContainerInstanceUrlFromJsonMetadata($jsonString) {
<#
    .Synopsis
        Once a container is created retreived the container default url.
        For web app or api app only.
#>
    $jsonContent = $jsonString | ConvertFrom-Json
    $fqdn = $jsonContent.ipAddress.fqdn
    $ip = $jsonContent.ipAddress.ip
    $port = $jsonContent.ipAddress.ports.port
    $url = "http://$fqdn`:$containerInstancePort"
    return $url
}

function Write-Host-Color([string]$message, $color = "Cyan") {
<#
    .Synopsis
        Write to the console in color
#>
    Write-Host $message -ForegroundColor $color
}

if($clearScreen) {
    cls
}
else {
    Write-Host "" 
}

Write-Host "deployContainerToAzureContainerRegistry -Action:$action" -ForegroundColor Yellow
Write-Host "Build project $(GetProjectName), version:$(GetProjectVersion)" -ForegroundColor DarkYellow
$newTag = "$acrLoginServer/$containerImage`:$(GetProjectVersion)" # Compute the container image with the .NET Core project version
$wepAppName = $containerImage.replace(".","-")
$azureLoginName = $acrName

switch($action) {

    # Build the .NET Core application in release mode
    # Build the container image from the .NET Core project and register the image in the local docker image repository
    build { 
        
        Write-Host-Color "Building .NET project"
        ############dotnet publish -c Release

        Write-Host-Color "`r`nBuild container containerImage:$containerImage"
        #docker build -t $containerImage .

        #docker build -t "donation.webdashboard" .
        # docker build -f "C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.WebDashboard\Dockerfile" 
        #  -t donationwebdashboard:dev --target base  --label "com.microsoft.created-by=visual-studio" "C:\DVT\microservices\fMicroService\DonationMicroServices\Source" 

        pushd
        cd ..\..
        docker build -t $containerImage -f "$dockerFilName" .
        popd

        $exp = "docker images --filter=""reference=$containerImage`:latest"""
        Invoke-Expression $exp        
    }

    # Tag the last image built of the container in the the local docker image repository 
    # Push the image into the Azure Container Registry
    push {
        <#
            Jenkins credentials plug in https://plugins.jenkins.io/azure-credentials
            Create an Azure service principal with Azure CLI https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?toc=%2Fazure%2Fazure-resource-manager%2Ftoc.json&view=azure-cli-latest
                C:\>  az ad sp create-for-rbac --name ServicePrincipalName
                c:\>az ad sp list
            https://docs.microsoft.com/en-us/azure/jenkins/execute-cli-jenkins-pipeline

            how to create service principal portal
            https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal

            https://stackoverflow.com/questions/45979852/using-azure-cli-in-jenkins-pipeline

            Try this: https://docs.microsoft.com/en-us/azure/jenkins/tutorial-jenkins-deploy-web-app-azure-app-service


          #>
        
        Write-Host-Color "AZ LOGIN - pw:$($env:azurePw)"
        az login -u "fredericaltorres@live.com" -p "$($env:azurePw)"
        
        Write-Host-Color "Login to azure registry $acrName"
        az acr login --name $acrName # Log in to container registry

        # Tag image with the loginServer of your container registry. 
        Write-Host-Color  "About to tag container $containerImage with tag:$newTag"
        docker tag $containerImage $newTag 
        docker images

        Write-Host-Color "About to push container $containerImage"
        Write-Host-Color "Tagged $newTag to azure registry $acrName"
        docker push $newTag # Push tagged image from docker into the azure registry logged in

        # Display the list of all this container version registered in the Azure Container Registry
        Write-Host-Color "All version in azure registry for container $containerImage" 
        az acr repository show-tags --name $acrName --repository $containerImage --output table
    }

    # Using the publish container image from this current .NET Core project, instanciate one instance of a container
    # In Azure Container Instance
    instantiate {
        
        $dnsLabel = "$($containerInstanceName)"

        Write-Host-Color "About to instantiate instance of container:$containerInstanceName"
        Write-Host-Color "From image:$newTag"
        Write-Host-Color "Start container with parameter: --environment-variables generationIndex=$containerInstanceIndex"

        # We pass specific parameter to the container instance via environment variable using the command --environment-variables
        $jsonString = az container create --resource-group $myResourceGroup --name $containerInstanceName --image $newTag --cpu $containerInstanceCpu --memory $containerInstanceMemory  --registry-login-server $acrLoginServer --registry-username $azureLoginName --registry-password $azureContainerRegistryPassword --ports $containerInstancePort --os-type Linux --dns-name-label $dnsLabel --environment-variables containerInstanceIndex=$containerInstanceIndex
        write-host "Container MetaData`r`n$jsonString"

        # Retreive metadata about the container instance from the Json blob returned
        $url = GetContainerInstanceUrlFromJsonMetadata $jsonString
        Write-Host-Color "Container Instance URL:$url"
    }

    createAppService {
        Write-Host-Color "About to create appService:$containerImage"
        az appservice plan create -n $containerImage -g $myResourceGroup --sku S1 --is-linux
    }

    createWebApp {
        # az webapp command https://docs.microsoft.com/en-us/cli/azure/webapp?view=azure-cli-latest
        # -p stand for plan or appservice name
        Write-Host-Color "About to create webApp:$wepAppName"
        write-host "az webapp create -g $myResourceGroup -p $containerImage -n $wepAppName --% --runtime `"DOTNETCORE|2.2`" "
        az webapp create -g $myResourceGroup -p $containerImage -n $wepAppName --% --runtime "DOTNETCORE|2.2"
    }

    configContainerWebApp {
        
        Write-Host-Color "About to set container image:$newTag inside webApp:$wepAppName"
        #$jsonString = az container create --resource-group $myResourceGroup --name $wepAppName --image $newTag --cpu $containerInstanceCpu --memory $containerInstanceMemory  --registry-login-server $acrLoginServer --registry-username $azureLoginName --registry-password $azureContainerRegistryPassword --ports $containerInstancePort --os-type Linux --dns-name-label $dnsLabel --environment-variables generationIndex=$containerInstanceIndex
        $jsonString = az webapp config container set --resource-group $myResourceGroup --name $wepAppName `
            --docker-custom-image-name $newTag `
            --docker-registry-server-url "https://$acrLoginServer" `
            --docker-registry-server-user  $azureLoginName `
            --docker-registry-server-password  $azureContainerRegistryPassword
        #--cpu $containerInstanceCpu --memory $containerInstanceMemory  --registry-login-server $acrLoginServer --registry-username $azureLoginName 
        #--registry-password $azureContainerRegistryPassword --ports $containerInstancePort --os-type Linux --dns-name-label $dnsLabel --environment-variables generationIndex=$containerInstanceIndex
    }

    # Request the log of a specific container instance
    getLog {

        Write-Host-Color "About to get the logs instance:$containerInstanceName"
        az container logs --resource-group $myResourceGroup --name $containerInstanceName
    }

    # Stop and delete an instance of the container under a specific name and version
    deleteInstance {

        Write-Host-Color "About to stop container instance:$containerInstanceName"
        az container stop --resource-group $myResourceGroup --name $containerInstanceName

        Write-Host-Color "About to delete container instance:$containerInstanceName"
        $jsonString = az container delete --resource-group $myResourceGroup --name $containerInstanceName --yes
    }
}

Write-Host-Color "Done" "DarkYellow"
