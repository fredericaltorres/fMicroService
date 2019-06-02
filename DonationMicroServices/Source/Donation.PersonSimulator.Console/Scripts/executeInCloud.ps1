[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
	[Alias('a')]
    [string]$action = "BuildPushInstantiate", # BuildPushInstantiate, deleteInstance
	[Parameter(Mandatory=$false)]
    [int]$generationIndex = 0
)
cls

function deleteInstanceContainer([int]$generationIndex) {
	
	./Scripts/deployContainerToAzureContainerRegistry.ps1 -action deleteInstance -clearScreen $false -generationIndex $generationIndex
}

function getLogFromContainer([int]$generationIndex) {

	./Scripts/deployContainerToAzureContainerRegistry.ps1 -action getLog -clearScreen $false -generationIndex $generationIndex
}

function startInstanceOfContainer([int]$genIndex) {

	#./Scripts/deployContainerToAzureContainerRegistry.ps1 -action instantiate -clearScreen $false -generationIndex $generationIndex
	invoke-expression "cmd /c start powershell -Command { ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action instantiate -generationIndex $genIndex }"
}

switch($action) {

    BuildPushInstantiate { 
        
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action build -clearScreen $false
        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action push -clearScreen $false
		
		startInstanceOfContainer $generationIndex

        Write-Host "Container instance should be running in Azure" -ForegroundColor Yellow
    }

	BuildPushInstantiateAll {
        
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action build -clearScreen $false
        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action push -clearScreen $false
		
		startInstanceOfContainer 0
		startInstanceOfContainer 1
		startInstanceOfContainer 2       

        Write-Host "Container instances should be running in Azure" -ForegroundColor Yellow
    }
 
    deleteInstance {
		deleteInstanceContainer $generationIndex
    }

	deleteInstanceAll {
		deleteInstanceContainer 0
		deleteInstanceContainer 1
		deleteInstanceContainer 2
    }

	getLog {
        getLogFromContainer $generationIndex
    }
	getLogAll {
        getLogFromContainer 0
		getLogFromContainer 1
		getLogFromContainer 2
    }
}
