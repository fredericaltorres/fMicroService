[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
    [ValidateSet('build', 'push','buildAndPush','buildPushAndDeploy','deploy','deleteDeployment','getLogs')]
    [string]$action = "deploy"
)

if($null -eq (Get-Module Util)) {
    Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\..\Util.psm1" -Force
}

$appName = GetAppNameFromProject
$dockerFilName = ".\Source\$appName\Dockerfile"
$containerImageName = $appName
$appVersion = GetProjectVersion
$scriptTitle = "Donation Automation Deployment Utility -- $appName  $($appVersion)"
$traceKubernetesCommand = $true
$deployService = $true

Write-HostColor "$scriptTitle" Yellow
Write-Host "$action appName:$($appName) - $($appVersion), containerImageName:$containerImageName" -ForegroundColor DarkYellow

function buildContainer() {

    Write-Host "First go up 2 folders before building container" -ForegroundColor DarkGray
    pushd
    cd ..\..
    docker build -t "$containerImageName"-f "$dockerFilName" .
    popd
}

function pushContainerImageToRegistry() {

    ..\deployContainerToAzureContainerRegistry.ps1 -a push -containerImage "$containerImageName" -cls $false
}

function deploy() {

    ..\Deployment.Kubernetes.ps1 -a deployToProd -appName $appName -appVersion $appVersion -cls $false -traceKubernetesCommand $traceKubernetesCommand -deployService $deployService
}

switch($action) {
    build {
        buildContainer
    }
    push {
        pushContainerImageToRegistry
    }
    buildAndPush {
        buildContainer
        pushContainerImageToRegistry
    }
    deploy { # Deploy rest api service on 2 pod and loadBalancer
        deploy
    }
    buildPushAndDeploy {
        buildContainer
        pushContainerImageToRegistry
        deploy
    }
    deleteDeployment { # Delete deployment of rest api service and loadBalancer
        ..\Deployment.Kubernetes.ps1 -a deleteDeployments -appName $appName -appVersion $appVersion -cls $false -traceKubernetesCommand $traceKubernetesCommand -deployService $true
    }
    getLogs { 
        ..\Deployment.Kubernetes.ps1 -a getLogs -appName $appName -appVersion $appVersion -cls $false -traceKubernetesCommand $traceKubernetesCommand -deployService $true
    }
}
Write-HostColor "$scriptTitle -- done" Yellow


