/*
    Pipeline Syntax: https://jenkins.io/doc/book/pipeline/syntax/
*/
pipeline {
    agent any
    environment {
        PROJECT_NAME = "fMicroService Project"
    }
    parameters {
        booleanParam (name: 'BuildPush_QueueProcessor', defaultValue: false, description: 'Build .NET core project and push container to Azure Registry')
        booleanParam (name: 'BuildPush_RestApi', defaultValue: false, description: 'Build .NET core project and push container to Azure Registry')
        booleanParam (name: 'BuildPush_PersonSimulator', defaultValue: false, description: 'Build .NET core project and push container to Azure Registry')
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
                powershell(".\\build.ps1 -a build -app Donation.QueueProcessor.Console")
            }
        }
        stage('BuildPush_RestApi') {
            when { anyOf { expression { return params.BuildPush_RestApi } } }
            steps {
                powershell(".\\build.ps1 -a build -app Donation.RestApi.Entrance")
            }
        }
        stage('BuildPush_PersonSimulator') {
            when { anyOf { expression { return params.BuildPush_PersonSimulator } } }
            steps {
                powershell(".\\build.ps1 -a build -app Donation.PersonSimulator.Console")
            }
        }
        stage('Package') {
            when {
                anyOf {
                    //branch 'master'
                    //branch 'develop'
                    expression { return params.DeployAndRunToAzure }
                }
            }
            steps {
                echo "DeployAndRunToAzure project:${env.PROJECT_NAME}, DeployAndRunToAzure:${params.DeployAndRunToAzure}"
            }
        }
    }
}