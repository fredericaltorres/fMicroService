[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
    [ValidateSet('build', 'push','buildAndPush','buildPushAndDeploy','deploy','deleteDeployment','getLogs', 'initData')]
    [string]$action = "build"
)

if($null -eq (Get-Module Util)) {
    Write-Host "PSScriptRoot: $PSScriptRoot"
    Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\Util.psm1" -Force
}


function executeCommandInFolder($folder, $theAction) {
    cd "$folder"
    Write-Host ""
    .\DeploymentUtility.ps1 -a $theAction
    cd ..
}

$scriptTitle = "Donation Automation Deployment Utility"
$appFolders = @("Donation.RestApi.Entrance", "Donation.QueueProcessor.Console", "Donation.PersonSimulator.Console")

Write-Host "$scriptTitle -- ALL" -ForegroundColor Yellow
Write-Host "Action: $action" -ForegroundColor DarkYellow

switch($action) {
    initData {
        Write-host "Delete donation azure tables" -ForegroundColor DarkYellow
        Set-Location "Donation.Monitor.Console"
        dotnet run -deleteTable true
        Set-Location ..
    }
    build {
        $appFolders | ForEach-Object -Process { executeCommandInFolder $_ $action }
    }
    push {
        $appFolders | ForEach-Object -Process { executeCommandInFolder $_ $action }
    }
    buildAndPush {
        $appFolders | ForEach-Object -Process { executeCommandInFolder $_ $action }
    }
    deploy { # Deploy rest api service on 2 pod and loadBalancer
        $appFolders | ForEach-Object -Process { executeCommandInFolder $_ $action }
    }
    buildPushAndDeploy {
        $appFolders | ForEach-Object -Process { executeCommandInFolder $_ $action }
    }
    deleteDeployment { # Delete deployment of rest api service and loadBalancer
        $appFolders | ForEach-Object -Process { executeCommandInFolder $_ $action }
    }    
}

Write-HostColor "$scriptTitle -- done" Yellow
