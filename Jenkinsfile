/*
    Pipeline Syntax: https://jenkins.io/doc/book/pipeline/syntax/
*/
pipeline {
    agent any
    environment {
        PROJECT_NAME = "fMicroService Project"
    }
    parameters {
        booleanParam (name: 'DeployAndRunToAzure', defaultValue: false, description: 'Deploy and run to Azure')
    }
    stages {
        stage('Init') {
            steps {
                echo "Initialization project:${env.PROJECT_NAME}, WORKSPACE:${env.WORKSPACE}"
            }
        }
        stage('Build') {
            steps {
                echo "Building project:${env.PROJECT_NAME}"
                powershell(".\\build.ps1")
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