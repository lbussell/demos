
param([string]$dockerfile=".\Dockerfile")

docker build -t test -f $dockerfile .
docker images test

docker run --rm -p 8080:8080 test
