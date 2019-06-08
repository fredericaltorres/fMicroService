<#
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

$containerImage = $containerImage.toLower()
if($containerInstanceName -eq "") { # If container name instance is empty use the name of the container image + ".instance"
    $containerInstanceName = $containerImage + ".instance"
}
$containerInstanceName += "-$containerInstanceIndex" # always post fix the container instance name with an index to allow to handle multiple container instance
$containerInstanceName = $containerInstanceName.replace(".", "-").toLower() # Required by Azure

if($azureContainerRegistryPassword -eq $null -or $azureContainerRegistryPassword.Length -eq 0) {
    throw "Parameter `$azureContainerRegistryPassword is required"
}

function GetProjectName() {
<#
    .Synopsis
        Returns the name of the .NET Core project name from the current folder
#>
    $project = gci -path . -rec -Include *.csproj # Assume there is only one csproj in the current directory
    return $project.Name
}

function GetProjectVersion() {
<#
    .Synopsis
        Returns the version of the .NET Core project from the current folder.
        The default version 1.0.0 will not work. Set the version using the IDE.
#>
    $projectName = GetProjectName
    [xml]$doc = get-content($projectName)
    $version = $doc.Project.PropertyGroup.Version
    return $version
}

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
        dotnet publish -c Release

        Write-Host-Color "`r`nBuild container containerImage:$containerImage"
        docker build -t $containerImage .
        $exp = "docker images --filter=""reference=$containerImage`:latest"""
        Invoke-Expression $exp        
    }

    # Tag the last image built of the container in the the local docker image repository 
    # Push the image into the Azure Container Registry
    push {

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
        $jsonString = az container create --resource-group $myResourceGroup --name $containerInstanceName --image $newTag --cpu $containerInstanceCpu --memory $containerInstanceMemory  --registry-login-server $acrLoginServer --registry-username $azureLoginName --registry-password $azureContainerRegistryPassword --ports $containerInstancePort --os-type Linux --dns-name-label $dnsLabel --environment-variables generationIndex=$containerInstanceIndex
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
