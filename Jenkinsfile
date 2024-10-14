pipeline {
    agent any

    stages {
        stage('Packaging') {
            steps {
                sh 'docker build --pull --rm -f Dockerfile -t study-space:latest .'
            }
        }

        stage('Push to DockerHub') {
            steps {
                withDockerRegistry(credentialsId: 'dockerhub', url: 'https://index.docker.io/v1/') {
                    sh 'docker tag study-space:latest thangbinhbeo/study-space:latest'
                    sh 'docker push thangbinhbeo/study-space:latest'
                }
            }
        }

        stage('Deploy FE to DEV') {
            steps {
                echo 'Deploying and cleaning'
                
                sh 'if [ $(docker ps -q -f name=study-space) ]; then docker container stop study-space; fi'
                sh 'echo y | docker system prune'
                sh 'docker container run -d --name study-space -p 8080:8080 -p 8081:8081 thangbinhbeo/study-space'
            }
        }
    }

    post {
        always {
            cleanWs()
        }
    }
}
