[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [Alias('a')]
    [ValidateSet('build', 'push','buildAndPush','buildPushAndDeploy','deploy','deleteDeployment','getLogs', 'initData', 'info')]
    [string]$action = "initData",

    [Parameter(Mandatory=$false)]
    [ValidateSet('all','Donation.QueueProcessor.Console','Donation.RestApi.Entrance','Donation.PersonSimulator.Console')]
    [string]$app = "all"
)

<#
write-host "Clean System"
.\DeploymentUtilityAll.ps1 -a deleteDeployment -app all
.\DeploymentUtilityAll.ps1 -a initData
write-host "done"

write-host "Restart System"
.\DeploymentUtilityAll.ps1 -a deploy -app Donation.RestApi.Entrance
.\DeploymentUtilityAll.ps1 -a deploy -app Donation.PersonSimulator.Console
.\DeploymentUtilityAll.ps1 -a deploy -app Donation.QueueProcessor.Console
write-host "done - Running"


#>

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
cls
$scriptTitle = "Donation Automation Deployment Utility"
if($app -eq "all") {
    $appFolders = @("Donation.RestApi.Entrance", "Donation.QueueProcessor.Console", "Donation.PersonSimulator.Console")
}
else {
    $appFolders = @($app)
}

Write-Host "$scriptTitle -- ALL" -ForegroundColor Yellow
Write-Host "Action: $action" -ForegroundColor DarkYellow

switch($action) {
    initData {
        Write-host "Delete donation azure tables" -ForegroundColor DarkYellow
        Set-Location "Donation.Monitor.Console"
        dotnet run -deleteTable true
        Set-Location ..
    }
    info {
        $appFolders | ForEach-Object -Process { executeCommandInFolder $_ $action }
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

Write-Host "`r`n$scriptTitle -- ALL" -ForegroundColor Yellow
