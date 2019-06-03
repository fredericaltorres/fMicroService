[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
	[Alias('a')]
	[ValidateSet('buildPushInstantiate', 'buildPushInstantiateAll', 'deleteInstance', 'deleteInstanceAll', 'getLog', 'getLogAll')]
    [string]$action = "BuildPushInstantiate",
	[Parameter(Mandatory=$false)]
    [int]$containerInstanceIndex = 0
)
cls

function deleteInstanceContainer([int]$containerInstanceIndex) {
	
	./Scripts/deployContainerToAzureContainerRegistry.ps1 -action deleteInstance -clearScreen $false -containerInstanceIndex $containerInstanceIndex
}

function getLogFromContainer([int]$containerInstanceIndex) {

	./Scripts/deployContainerToAzureContainerRegistry.ps1 -action getLog -clearScreen $false -containerInstanceIndex $containerInstanceIndex
}

function startInstanceOfContainer([int]$genIndex, [bool]$synchronous) {

	if($synchronous) {
		./Scripts/deployContainerToAzureContainerRegistry.ps1 -a instantiate -containerInstanceIndex $genIndex
	}
	else {
		invoke-expression "cmd /c start powershell -Command { ./Scripts/deployContainerToAzureContainerRegistry.ps1 -a instantiate -containerInstanceIndex $genIndex }"
	}
	
}

$donationGenerationFile = @(0,1) #,2,3,4,5,6,7,8,9

switch($action) {

    BuildPushInstantiate { 
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action build -clearScreen $false
        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action push -clearScreen $false
		
		startInstanceOfContainer $containerInstanceIndex $true

        Write-Host "Container instance should be running in Azure" -ForegroundColor Yellow
    }

	BuildPushInstantiateAll {
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action build -clearScreen $false
        ./Scripts/deployContainerToAzureContainerRegistry.ps1 -action push -clearScreen $false

		$donationGenerationFile | ForEach-Object -Process { 
			startInstanceOfContainer $_ $false
		}

        Write-Host "Container instances should be running in Azure" -ForegroundColor Yellow
    }
 
    deleteInstance {
		deleteInstanceContainer $containerInstanceIndex
    }

	deleteInstanceAll {
		$donationGenerationFile | ForEach-Object -Process { 
			deleteInstanceContainer  $_
		}
    }

	getLog {
        getLogFromContainer $containerInstanceIndex
    }
	getLogAll {
		
		$donationGenerationFile | ForEach-Object -Process { 
			getLogFromContainer  $_
		}
    }
}
