[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
	[Alias('a')]
    [string]$action = "", # build, push, instantiate, deleteInstance, getLog

	#[Parameter(Mandatory=$false)]
    #[string]$generationIndex = "5",
	
    [Parameter(Mandatory=$false)]
    [string]$imageTag = "donation.queueprocessor.console",    
    [Parameter(Mandatory=$false)]
    $containerInstanceName = "donation.queueprocessor.console.instance0",

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
    [int]$containerInstanceCpu = 1,
    [Parameter(Mandatory=$false)] 
    [int]$containerInstanceMemory = 1,
    [Parameter(Mandatory=$false)] 
    [int]$containerInstancePort = 8080,

    [Parameter(Mandatory=$false)] 
    [bool]$clearScreen = $true
)

$containerInstanceName = $containerInstanceName.replace(".", "-")

function GetProjectName() {

    # Assume there is only one csproj in the current directory
    $project = gci -path . -rec -Include *.csproj
    return $project.Name
}

function GetProjectVersion() {

    $projectName = GetProjectName
    [xml]$doc = get-content($projectName)
    $version = $doc.Project.PropertyGroup.Version
    return $version
}

function GetContainerInstanceIpFromJsonMetadata($jsonString) {

    $jsonContent = $jsonString | ConvertFrom-Json
    $fqdn = $jsonContent.ipAddress.fqdn
    $ip = $jsonContent.ipAddress.ip
    $port = $jsonContent.ipAddress.ports.port
    $url = "http://$fqdn`:8080"
    return $url
}

function Write-Host-Color([string]$message, $color = "Cyan") {
    Write-Host ""
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

$newTag = "$acrLoginServer/$imageTag`:$(GetProjectVersion)"
$containerInstanceName = $containerInstanceName.toLower()

switch($action) {

    # Build and publish the current container source code in the local docker image repository
    build { 
        
        Write-Host-Color "Building .NET project"
        dotnet publish -c Release

        Write-Host-Color "Build container imageTag:$imageTag"
        docker build -t $imageTag .
        $exp = "docker images --filter=""reference=$imageTag`:latest"""
        Invoke-Expression $exp        
    }

    # Tag the last image built of the container in the the local docker image repository and push into the Azure Container Registry
    push {

        Write-Host-Color "Login to azure registry $acrName"
        az acr login --name $acrName # Log in to container registry

        # Tag image with the loginServer of your container registry. 
        Write-Host-Color  "About to tag container $imageTag with tag:$newTag"
        docker tag $imageTag $newTag 
        docker images

        Write-Host-Color "About to push container $imageTag tagged $newTag to azure registry $acrName"
        docker push $newTag # Push tagged image from docker into the azure registry logged in

        Write-Host-Color "All version in azure registry for container $imageTag"
        az acr repository show-tags --name $acrName --repository $imageTag --output table
    }

    # Using the versioned image in the Azure Container Registry, instanciate an instance of the container under a specific name
    # To find the Azure Container Instance from the portal click on the Resource Group here named 'FredContainerRegistryResourceGroup'
    instantiate {
        
        $azureLoginName = $acrName        
        $dnsLabel = "$($containerInstanceName)"

        Write-Host-Color "About to instantiate instance of container:$containerInstanceName from image:$newTag"
		#Write-Host-Color "Start container with parameter: --environment-variables generationIndex=$generationIndex"
        $jsonString = az container create --resource-group $myResourceGroup --name $containerInstanceName --image $newTag --cpu $containerInstanceCpu --memory $containerInstanceMemory  --registry-login-server $acrLoginServer --registry-username $azureLoginName --registry-password $azureContainerRegistryPassword --ports $containerInstancePort --os-type Linux --dns-name-label $dnsLabel # --environment-variables generationIndex=$generationIndex
		        
        $url = GetContainerInstanceIpFromJsonMetadata $jsonString
        Write-Host-Color "Container Instance URL:$url"
    }

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
