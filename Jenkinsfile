/*
    Pipeline Syntax: https://jenkins.io/doc/book/pipeline/syntax/
*/
pipeline {
    agent any
    environment {        
        PROJECT_NAME = "fMicroService Project"
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
            steps {
                echo "Packaging project:${env.PROJECT_NAME}"
            }
        }        
    }
}