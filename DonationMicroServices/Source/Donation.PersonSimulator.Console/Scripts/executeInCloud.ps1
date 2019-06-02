[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
	[Alias('a')]
    [string]$action = "BuildPushInstantiate", # BuildPushInstantiate, BuildPushInstantiateAll, deleteInstance, deleteInstanceAll, getLog, getLogAll
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

$donationGenerationFile = @(0,1,2,3) #,4,5,6,7,8,9

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

		$donationGenerationFile | ForEach-Object -Process { 
			startInstanceOfContainer $_
		}

        Write-Host "Container instances should be running in Azure" -ForegroundColor Yellow
    }
 
    deleteInstance {
		deleteInstanceContainer $generationIndex
    }

	deleteInstanceAll {
		$donationGenerationFile | ForEach-Object -Process { 
			deleteInstanceContainer  $_
		}
    }

	getLog {
        getLogFromContainer $generationIndex
    }
	getLogAll {
		
		$donationGenerationFile | ForEach-Object -Process { 
			getLogFromContainer  $_
		}
    }
}
