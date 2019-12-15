
docker network rm mongo-net
docker network create -d bridge mongo-net

docker volume create --name=mono-runtimeprediction-data

docker stop mongodb-runtimeprediction
docker rm mongodb-runtimeprediction

docker run --name mongodb-runtimeprediction --network mongo-net -p 27017:27017 --restart unless-stopped -v mono-runtimeprediction-data:/data/db -d mongo
docker start mongodb-runtimeprediction
docker logs -f mongodb-runtimeprediction

docker stop mongo-express
docker rm mongo-express
docker run --restart unless-stopped --name mongo-express --network mongo-net --link mongodb-runtimeprediction:mongo -p 8091:8081 -d mongo-express
docker start mongo-express
docker logs -f mongo-express