[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [Alias('a')]
    [ValidateSet('create', 'delete', 'info','select')]
    [string]$action = "info",

    [Parameter(Mandatory=$false)]
    [string]$kubernetesClusterName = "fkubernetes4"
)

if($null -eq (Get-Module Util)) {
    Write-Host "PSScriptRoot: $PSScriptRoot"
    Import-Module "$(if($PSScriptRoot -eq '') {'.'} else {$PSScriptRoot})\Util.psm1" -Force
}

# https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-sizes-specs
#$vmSize = "Standard_D2s_v2" # 2 cpu, 7 Gb Ram
#$vmSize = "Standard_D4s_v3" # 4 cpu, 17 Gb Ram
#$vmSize = "Standard_D1_v2" # 1 cpu, 3.5 Gb Ram
$vmSize = "Standard_D2_v2" # 2 cpu, 7 Gb Ram
$vmSize = "Standard_D4_v3" # 4 cpu, 17 Gb Ram
$vmCount = 3
$kubernetesClusterRegion = "eastus2"

function selectKubernetesCluster ([string]$kubernetesClusterName) {

    write-host "Initialize credential"
    az aks get-credentials --resource-group $kubernetesClusterName --name $kubernetesClusterName

    write-host "Set $kubernetesClusterName as default"
    kubectl config use-context $kubernetesClusterName # Switch to cluster
}

function createKubernetesCluster (
    [string]$kubernetesClusterName,
    [string]$vmSize,
    [int]$vmCount,
    [string]$kubernetesClusterRegion
    ) {

    write-host "Creating Azure Resource Group $kubernetesClusterName"
    # az group create -n $kubernetesClusterName -l $kubernetesClusterRegion

    write-host "Creating Kubernetes Cluster $kubernetesClusterName"
    az aks create --name $kubernetesClusterName --resource-group $kubernetesClusterName `
        --kubernetes-version 1.12.8 --enable-addons monitoring  `
        --generate-ssh-keys  `
        --node-count $vmCount --node-vm-size $vmSize
    # --enable-rbac rbac is on by default

    selectKubernetesCluster $kubernetesClusterName
}

$scriptTitle = "Kubernetes Cluster Management Utility"
Write-Host "$scriptTitle" -ForegroundColor Yellow
Write-Host "Action: $action" -ForegroundColor DarkYellow

switch($action) {
    create {
        createKubernetesCluster $kubernetesClusterName $vmSize $vmCount $kubernetesClusterRegion
    }
    delete {
        az aks delete -n $kubernetesClusterName -g $kubernetesClusterName
        # az group delete -n $kubernetesClusterName
    }
    select {
        selectKubernetesCluster $kubernetesClusterName
    }
    info {
        & az aks list -o table # Get the list of clusters
        & kubectl.exe cluster-info
        & kubectl.exe config view
        & kubectl.exe get nodes # Get the list all nodes or vm
        & kubectl.exe get pods --all-namespaces # List pods running in the cluster
        & kubectl.exe version # Get version of client and server Kubernetes
    }
}

Write-Host "`r`n$scriptTitle" -ForegroundColor Yellow
