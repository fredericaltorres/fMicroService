/*
    Pipeline Syntax: https://jenkins.io/doc/book/pipeline/syntax/
*/
pipeline {
    agent any
    environment {        
        PROJECT_NAME = "fMicroService Project"
    }
    parameters {
        booleanParam (name: 'FORCE_PACKAGE', defaultValue: false, description: 'Package build')
    }
    stages {
        stage('Init') {
            steps {
                echo "Initialization project:${env.PROJECT_NAME}"
                // cd "DonationMicroServices/Source"
            }
        }
        stage('Build') {
            steps {
                echo "Building project:${env.PROJECT_NAME}"
            }
        }
        stage('Package') {
            when {
                anyOf {
                    //branch 'master'
                    branch 'develop'
                    //expression { return params.FORCE_PUBLISH }
                }
            }            
            steps {
                echo "Packaging project:${env.PROJECT_NAME}"
            }
        }        
    }
}