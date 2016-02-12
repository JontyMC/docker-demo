#Docker Demo with .NET Core

* Fast paced with a lot of command line typing - so ask questions if you don't understand something

##Hello World
* Ensure images are cleared except for microsoft/aspnet:1.0.0-rc1-final
* ```docker rmi `docker images -q````
* ```docker pull microsoft/aspnet:1.0.0-rc1-final```

1. Navigate to api:```cd api```
1. Open VSCode:```code .```
1. The simplest [.NET Core HTTP API, using OWIN](api/Startup.cs)
1. [Dockerfile](api/Dockerfile) - each command is a cached layer
1. Close VSCode
1. List images (layers):```docker images -a```
1. Build image:```docker build -t demo/hello .```
1. List images:```docker images -a```
1. Run container:```docker run -t -d -P --name hello demo/hello```
1. Show containers (point out dynamically assigned port):```docker ps -a```
1. Show in browser:```http://localhost:xxxx/```
1. Show logs:```docker logs hello```
1. Show info, eg ip:```docker inspect hello```
1. Show running containers:```docker ps -a```
1. Stop:```docker stop hello```
1. Remove: ```docker rm hello```
1. Show running containers:```docker ps -a```
1. Run image with specific port:```docker run -t -d -p 5004:5004 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

##Registry (Docker hub)

* Now we have a Docker image, what are we going to do with it?
* Need some way to share images
* Docker has concept of registries, a bit like nuget repositories, but for docker images
* The equivalent of the public nuget feed is docker hub::```https://hub.docker.com/```
* Have previously pushed the "hello" container to dockerhub:```docker push jontymc/hello```
* Show, inc build settings:```https://hub.docker.com/r/jontymc/hello/```

1. Show running containers:```docker ps -a```
1. Pull built image from registry:```docker pull jontymc/hello```
1. Run image with specific port:```docker run -t -d -p 5005:5004 --name hello2 jontymc/hello```
1. Show in browser:```http://localhost:5005/```
1. Show other in browser:```http://localhost:5004/```
1. Stop and remove in one line: ```docker rm --force `docker ps -qa````

* Normal workflow would be for CI to push images to a registry
* Docker changes dev/ops contract from nuget package to docker image

##Volumes

* How do we make a change and rebuild the container?

1. Open VSCode:```code .```
1. Change "Hello world" to "Hello Huddle"
1. Close VSCode
1. Rebuild image:```docker build -t demo/hello .```
1. Run image:```docker run -t -d -p 5004:5004 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

* Having to rebuild the image each time is clunky, let's use volume

1. Remove:```docker rm --force `docker ps -qa````
1. Show running containers:```docker ps -a```
1. Run image from demo 1 with a volume:```docker run -t -d -p 5004:5004 --name hello -v `pwd`:/app demo/hello```
  * This mounts the host directory inside the container
1. Show running containers:```docker ps -a```
1. Open VSCode:```code .```
1. Change "Hello Huddle" to "Hello world"
1. Close VSCode
1. Restart container:```docker restart hello```
1. Show in browser:```http://localhost:5004/```

* We could run application with DNX-watch, which will restart application when any files change
* It is possible to run the entire dev environment inside a docker container
* Eg, here is spotify running from a container:
  * ```xhost local:root```
  * ```docker run -it -v /tmp/.X11-unix:/tmp/.X11-unix -e DISPLAY=unix$DISPLAY --device /dev/snd --name spotify jess/spotify```

##Containerized Redis

1. Remove all containers: ```docker rm --force `docker ps -qa````
1. Run redis:```docker run --name redis -d -p 6379:6379 redis```
1. Show running containers:```docker ps -a```
1. Run cli:```docker run -it --link redis:redis --rm redis sh -c 'exec redis-cli -h redis'```
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit
1. Show running with -rm flag removes container when stopped:```docker ps -a```
1. Navigate to api_redis:```cd ../api_redis```
1. Open VSCode:```code .```
1. Show added reference to stack exchange in [project.json](api_redis/project.json)
1. Show [redis code in api](api_redis/startup.cs)
1. Close VSCode
1. Build image:```docker build -t demo/hello .```
1. Run image:```docker run -t -d -p 5004:5004 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

##Managing Multiple Containers with Compose

* Our application now consists of two containers, annoying to have to run both
* Docker Compose lets us specify which containers to run in a manifest

1. Navigate to docker-demo root:```cd ..```
1. Open VSCode:```code .```
1. Show [docker-compose.yml](docker-compose.yml)

* Containers run in order specified
* We are building hello api here, but we could also choose to pull from registry like redis. We could even choose a specific version.
* You could set up a test environment with any combination by simply changing the manifest

1. Close VSCode
1. Remove all containers:```docker rm --force `docker ps -qa````
1. Show running containers:```docker ps -a```
1. Run compose:```docker-compose up -d```
1. Run compose as daemon:```docker-compose up```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

* No data, because redis has been reinstalled

1. Run cli:```docker run -it --net=dockerdemo_default --link redis:redis --rm redis sh -c 'exec redis-cli -h redis'```
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit
1. Show in browser:```http://localhost:5004/```

##Persistence with Data Volumes

1. Remove all containers:```docker rm --force `docker ps -qa````
1. Create data volume:```docker create -v /redis_data --name redis_data redis /bin/true```
  * While this container doesn’t run an application, it reuses the redis image so that all containers are using layers in common, saving disk space.
1. Run redis with mapped volume:```docker run --name redis -d -p 6379:6379 --volumes-from redis_data redis redis-server --appendonly yes```
  * ```redis-server --appendonly yes``` is the command to run the container as persistent redis
1. Run cli:```docker run -it --link redis:redis --rm redis sh -c 'exec redis-cli -h redis'```
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit

* We can now use this volume in our compose setup

1. Open VSCode:```code .```
1. Show [docker-compose-data.yml](docker-compose-data.yml)
1. Remove redis:```docker rm --force redis```
1. Run compose:```docker-compose -f docker-compose-data.yml up -d```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```
1. Remove all containers:```docker rm --force `docker ps -qa````

* Data volumes can be used for backup, restore and migration
* Imagine having a build that output known, good test data
* Could create a registry of data images for different scenarios
* Or doing a live-migration ahead of time and hot-swapping the data




## TODO: versioning v1+v2, networks, compose options eg scale

##Not covered

* Docker swarm
* Docker machine
* Docker networking
* Other orchestration tools - eg kubernetes or mesos
* https://www.mindmeister.com/389671722/open-container-ecosystem-formerly-docker-ecosystem