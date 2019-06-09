[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
	[Alias('a')]
    [string]$action = "demo" # demo, deleteDeployments
)

Import-Module ".\Util.psm1" -Force

cls

Write-HostColor "Blue Green Deployment With Kubernetes, Azure CLI, Powershell Demo" Yellow

switch($action) {

    demo {
		./BlueGreenDeployment.Kubernetes.ps1 -a initialDeploymentToProd -cls $false
		pause
		./BlueGreenDeployment.Kubernetes.ps1 -a deployToStaging -cls $false
		pause
		./BlueGreenDeployment.Kubernetes.ps1 -a getInfo -cls $false
		pause
		./BlueGreenDeployment.Kubernetes.ps1 -a switchStagingToProd -cls $false
		pause
		./BlueGreenDeployment.Kubernetes.ps1 -a getInfo -cls $false
		pause
		./BlueGreenDeployment.Kubernetes.ps1 -a revertProdToPrevious -cls $false
		pause
		./BlueGreenDeployment.Kubernetes.ps1 -a getInfo -cls $false
		pause		
    }
	deleteDeployments {
		./BlueGreenDeployment.Kubernetes.ps1 -a deleteDeployments -cls $false
	}
}

Write-Host "End of demo" -ForegroundColor DarkYellow
