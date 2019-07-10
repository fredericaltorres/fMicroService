[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
	[Alias('a')]
	[ValidateSet('BuildPush', 'BuildPushInstantiate', 'deleteInstance')]
    [string]$action = 'BuildPush'
)
cls

write-error "NOT WORKING, NPM is not installed in container"

switch($action) {

	BuildPush { 
        
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ../deployContainerToAzureContainerRegistry.ps1 -action build -containerImage "Donation.WebDashboard" -clearScreen $false
        ../deployContainerToAzureContainerRegistry.ps1 -action push  -containerImage "Donation.WebDashboard" -clearScreen $false

        Write-Host "Container published in Azure Container Registry" -ForegroundColor Yellow
    }

    BuildPushInstantiate { 
        
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ../deployContainerToAzureContainerRegistry.ps1 -action build -clearScreen $false
        ../deployContainerToAzureContainerRegistry.ps1 -action push -clearScreen $false
        ../deployContainerToAzureContainerRegistry.ps1 -action instantiate -clearScreen $false        

        Write-Host "Container instance should be running in Azure, start by opening the resource group in the Azure portal" -ForegroundColor Yellow
    }
 
    deleteInstance {

        ../deployContainerToAzureContainerRegistry.ps1 -action deleteInstance -clearScreen $false
    }
}
