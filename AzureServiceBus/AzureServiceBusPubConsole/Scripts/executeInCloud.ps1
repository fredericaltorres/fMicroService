[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$action = "BuildPushInstantiate" # BuildPushInstantiate, deleteInstance
)
cls

switch($action) {

    BuildPushInstantiate { 
        
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action build -clearScreen $false
        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action push -clearScreen $false
        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action instantiate -clearScreen $false        

        Write-Host "Container instance should be running in Azure, start by opening the resource group in the Azure portal" -ForegroundColor Yellow
    }
 
    deleteInstance {

        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action deleteInstance -clearScreen $false
    }
}
