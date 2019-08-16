/*
    Pipeline Syntax: https://jenkins.io/doc/book/pipeline/syntax/
*/
pipeline {
    agent any
    environment {        
        PROJECT_NAME = "fMicroService Project"
    }    
    stages {
        stage('Test') {
            steps {
                echo "Executing stage Test, project:${env.PROJECT_NAME}"
                cd "DonationMicroServices/Source"
            }
        }
    }
}