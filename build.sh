# build options
# BUILD_REVISION=`git rev-parse --short HEAD`
# BUILD_DIR_BASE=`git rev-parse --git-dir`/..
# BUILD_VERSION?=
# BUILD_IMAGE=0
IMAGE_ACCOUNT=${2:-goldenstarc}
IMAGE_REPO=${3:-virgolLanding}
IMAGE_TAG=${1:-latest}
TAG_REVISION=0

sudo git stash
sudo git pull origin master
# sudo git pull origin Beta

# docker image prune
# sudo sed -i 's/process.env.REACT_APP_VERSION/'$IMAGE_TAG' نسخه/g' ./src/Presentation/Virgol.School/ClientApp/src/components/login/Login.js
sudo docker login
sudo docker build -t $IMAGE_ACCOUNT/$IMAGE_REPO:$IMAGE_TAG -t $IMAGE_ACCOUNT/$IMAGE_REPO:latest --force-rm .

sudo docker push $IMAGE_ACCOUNT/$IMAGE_REPO
sudo docker push $IMAGE_ACCOUNT/$IMAGE_REPO:$IMAGE_TAG

echo -e "\ncd ~/docker/virgol/ && docker-compose pull && docker-compose up -d\n"
