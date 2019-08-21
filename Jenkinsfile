/*
    Pipeline Syntax: https://jenkins.io/doc/book/pipeline/syntax/
*/
pipeline {
    agent any
    environment {
        PROJECT_NAME = "fMicroService Project"
        AZURE_CRED_ID="jenkinsAzureCli2"
        RES_GROUP="yourWebAppAzureResourceGroupName" // do not need that
        WEB_APP="yourWebAppName" // do not need that
    }
    parameters {
        booleanParam (name: 'BuildPush_QueueProcessor', defaultValue: false, description: 'Build .NET core project and push container to Azure Registry')
        booleanParam (name: 'BuildPush_RestApi', defaultValue: false, description: 'Build .NET core project and push container to Azure Registry')
        booleanParam (name: 'BuildPush_PersonSimulator', defaultValue: false, description: 'Build .NET core project and push container to Azure Registry')
        booleanParam (name: 'DeployAndRunInCurrentAzureKubernetesCluster', defaultValue: false, description: 'Deploy And Run In Current Azure Kubernetes Cluster')

        booleanParam (name: 'CleanCurrentAzureKubernetesCluster', defaultValue: false, description: 'Clean Current Azure Kubernetes Cluster')

        booleanParam (name: 'TestMode', defaultValue: false, description: 'Test Mode')

        
// 'all','Donation.QueueProcessor.Console','Donation.RestApi.Entrance','Donation.PersonSimulator.Console'
    }
    stages {
        stage('Init') {
            steps {
                echo "Initialization project:${env.PROJECT_NAME}, WORKSPACE:${env.WORKSPACE}"
            }
        }
        stage('BuildPush_QueueProcessor') {
            when { anyOf { expression { return params.BuildPush_QueueProcessor } } }
            steps {
                powershell(".\\build.ps1 -a buildAndPush -app Donation.QueueProcessor.Console")
            }
        }
        stage('BuildPush_RestApi') {
            when { anyOf { expression { return params.BuildPush_RestApi } } }
            steps {
                powershell(".\\build.ps1 -a buildAndPush -app Donation.RestApi.Entrance")
            }
        }
        stage('DeployAndRunInCurrentAzureKubernetesCluster') {
            when { anyOf { expression { return params.DeployAndRunInCurrentAzureKubernetesCluster } } }
            steps {
                // 'all','Donation.QueueProcessor.Console','Donation.RestApi.Entrance','Donation.PersonSimulator.Console'
                powershell(".\\build.ps1 -a deploy -app Donation.QueueProcessor.Console")
                powershell(".\\build.ps1 -a deploy -app Donation.RestApi.Entrance")
                powershell(".\\build.ps1 -a deploy -app Donation.PersonSimulator.Console")
            }
        }
        stage('CleanCurrentAzureKubernetesCluster') {
            when { anyOf { expression { return params.CleanCurrentAzureKubernetesCluster } } }
            steps {
                // 'all','Donation.QueueProcessor.Console','Donation.RestApi.Entrance','Donation.PersonSimulator.Console'
                powershell(".\\build.ps1 -a deleteDeployment -app Donation.QueueProcessor.Console")
                powershell(".\\build.ps1 -a deleteDeployment -app Donation.RestApi.Entrance")
                powershell(".\\build.ps1 -a deleteDeployment -app Donation.PersonSimulator.Console")
                powershell(".\\build.ps1 -a initData -app all")
            }
        }
        stage('TestMode') {
            when { anyOf { expression { return params.TestMode } } }
            steps {
                echo " YESSSSSSSSSSSSSSSSS "
            }
        }

        // stage('Package') {
        //     when {
        //         anyOf {
        //             //branch 'master'
        //             //branch 'develop'
        //             expression { return params.DeployAndRunToAzure }
        //         }
        //     }
        //     steps {
        //         echo "DeployAndRunToAzure project:${env.PROJECT_NAME}, DeployAndRunToAzure:${params.DeployAndRunToAzure}"
        //     }
        // }
    }
}