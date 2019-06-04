[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
	[Alias('a')]
	[ValidateSet('buildPushInstantiate', 'buildPushInstantiateAll', 'deleteInstance', 'deleteInstanceAll', 'getLog', 'getLogAll')]
    [string]$action,

	[Parameter(Mandatory=$false)]
    [int]$containerInstanceIndex = 0,

	[Parameter(Mandatory=$false)]
    [string]$containerImage = "donation.queueprocessor.console"
)
cls



function deleteInstanceContainer([int]$containerInstanceIndex) {
	
	../deployContainerToAzureContainerRegistry.ps1 -action deleteInstance -cls $false -containerImage $containerImage -containerInstanceIndex $containerInstanceIndex
}

function getLogFromContainer([int]$containerInstanceIndex) {

	../deployContainerToAzureContainerRegistry.ps1 -action getLog -cls $false -containerImage $containerImage -containerInstanceIndex $containerInstanceIndex
}

function startInstanceOfContainer([int]$genIndex, [bool]$synchronous) {

	if($synchronous) {
		../deployContainerToAzureContainerRegistry.ps1 -a instantiate -containerImage $containerImage -containerInstanceIndex $genIndex
	}
	else {
		invoke-expression "cmd /c start powershell -Command { ../deployContainerToAzureContainerRegistry.ps1 -a instantiate -containerImage $containerImage -containerInstanceIndex $genIndex }"
	}
}

$donationGenerationFile = @(0,1) #,2,3,4,5,6,7,8,9

switch($action) {

    BuildPushInstantiate { 
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ../deployContainerToAzureContainerRegistry.ps1 -action build -cls $false -containerImage $containerImage 
        ../deployContainerToAzureContainerRegistry.ps1 -action push -cls $false -containerImage $containerImage 
		
		startInstanceOfContainer $containerInstanceIndex $true

        Write-Host "Container instance should be running in Azure" -ForegroundColor Yellow
    }

	BuildPushInstantiateAll {
        Write-Host "About to build, publish and execute this .NET Core project as a Azure Container" -ForegroundColor Yellow

        ../deployContainerToAzureContainerRegistry.ps1 -action build -cls $false -containerImage $containerImage 
        ../deployContainerToAzureContainerRegistry.ps1 -action push -cls $false -containerImage $containerImage 

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
