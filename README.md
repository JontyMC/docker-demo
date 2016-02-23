#Docker Demo with .NET Core

* Fast paced with a lot of command line typing - so ask questions if you don't understand something
* Docker: no longer deploy app onto an environment, deploy app with the environment


##Hello World
* Ensure images are cleared except for microsoft/aspnet:1.0.0-rc1-final
* ```docker rmi `docker images -q````
* ```docker pull microsoft/aspnet:1.0.0-rc1-final```

1. Explain [.NET Core HTTP API, using OWIN](api/Startup.cs)
  * Simplest possible .NET Core API
1. Explain [Dockerfile](api/Dockerfile)
  * Inherits from asp.net mono image
  * Each command is a cached layer
  * DNU restore restores all packages in the project.json file
  * So, we copy project.json first so we don't have to re-pull packages every time we rebuild the code layer
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

##Make Changes to a Container

* How do we make a change and rebuild the container?

1. Change "Hello world" to "Hello Huddle" in api code
1. Rebuild image:```docker build -t demo/hello .```
1. Run image:```docker run -t -d -p 5004:5004 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

* Having to rebuild the image each time is clunky
* Instead we can mount a host directory inside the container

1. Remove:```docker rm --force `docker ps -qa````
1. Show running containers:```docker ps -a```
1. Run image from demo 1 with a host mount:```docker run -t -d -p 5004:5004 --name hello -v `pwd`:/app demo/hello```
1. Show running containers:```docker ps -a```
1. Change "Hello Huddle" back to "Hello world" in api code
1. Restart container:```docker restart hello```
1. Show in browser:```http://localhost:5004/```

* We could run application with DNX-watch, which will restart application when any files change
* It is possible to run the entire dev environment inside a docker container
* Eg, here is spotify running from a container:
  * ```xhost local:root```
  * ```docker run -it -v /tmp/.X11-unix:/tmp/.X11-unix -e DISPLAY=unix$DISPLAY --device /dev/snd --name spotify jess/spotify```

##Containerized Redis

1. Remove all containers:```docker rm --force `docker ps -qa````
1. Run redis:```docker run --name redis -d -p 6379:6379 redis```
1. Show logs:```docker logs redis```
1. Show running containers:```docker ps -a```
1. Run cli:```docker run -it --link redis:redis --rm redis sh -c 'exec redis-cli -h redis'```
  * "link" flag means map the "redis" container to the hostname "redis" inside the container
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit
1. Show running with -rm flag removes container when stopped:```docker ps -a```
1. Show added reference to stack exchange in [project.json](api_redis/project.json)
1. Show [redis code in api](api_redis/startup.cs)
1. Build image:```docker build -t demo/hello .```
1. Run image:```docker run -t -d -p 5004:5004 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

##Managing Multiple Containers with Compose

* Our application now consists of two containers, annoying to have to run both
* Docker Compose lets us specify which containers to run in a manifest

1. Show [docker-compose.yml](docker-compose.yml)
  * Containers run in order specified
  * We are building hello api here, but we could also choose to pull from registry like redis. We could even choose a specific version.
1. Remove all containers:```docker rm --force `docker ps -qa````
1. Show running containers:```docker ps -a```
1. Run compose as daemon:```docker-compose up -d```
1. Show running containers:```docker ps -a```
1. Show logs:```docker-compose logs```
1. Show in browser:```http://localhost:5004/```
  * No data, because redis has been reinstalled
1. Run cli:```docker run -it --net=dockerdemo_default --link redis:redis --rm redis sh -c 'exec redis-cli -h redis'```
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit
1. Show in browser:```http://localhost:5004/```
1. Remove composed application:```docker-compose down```

##Persistence with Data Volumes

* Volumes allow data to be shared between containers
* Volumes persist event if the container has been deleted

1. List existing volumes:```docker volumes ls```
1. Remove volumes:```docker volume rm `docker volume ls -qf dangling=true````
1. List volumes:```docker volumes ls```
1. Show running containers:```docker ps -a```
1. Create volume:```docker volume create --name dockerdemo_redis_data```
1. Run redis with mapped volume:```docker run --name redis -d -v dockerdemo_redis_data:/data redis redis-server --appendonly yes```
  * ```redis-server --appendonly yes``` is the command to run the container as persistent redis
1. Run cli:```docker run -it --link redis:redis --rm redis sh -c 'exec redis-cli -h redis'```
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit
1. Remove redis:```docker rm --force redis```
1. But we still have volume:```docker volumes ls```

* We can now use this volume in our compose setup

1. Show [docker-compose-data.yml](docker-compose-data.yml)
1. Run compose:```docker-compose -f docker-compose-data.yml up -d```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```
1. Remove composed application:```docker-compose -f docker-compose-data.yml down```

* We didn't actually need to create the volume, as it would be automatically created through compose up
* Data volumes can be used for backup, restore and migration
* Imagine having a build that output known, good test data
* Could create a registry of data images for different scenarios
* Or doing a live-migration ahead of time and hot-swapping the data

##Multiple Environments On Single Host

* Docker has virtualised networking that allows us to create isolated environments with multiple containers

1. Show networks:```docker network ls```
1. Remove all non-default networks:```docker network ls | awk '{print $1, $2}' | grep -v 'none\|host\|bridge' | awk '{print $1}' | xargs docker network rm```
1. Show [docker-compose-multi.yml](docker-compose-multi.yml)
  * Removed container name, so name will be autogenerated based on the network name
  * Added container to put data into redis automatically
  * Hello API port is being randomly assigned
1. Run compose with project name environment variable:```COMPOSE_PROJECT_NAME=env1 docker-compose -f docker-compose-multi.yml up -d```
1. Show running containers:```docker ps -a```
1. Show networks:```docker network ls```
1. Show volumes:```docker volume ls```
1. Run another environment:```COMPOSE_PROJECT_NAME=env2 docker-compose -f docker-compose-multi.yml up -d```
1. Show running containers:```docker ps -a```
1. Show networks:```docker network ls```
1. Remove containers:```docker rm --force `docker ps -qa````
1. Lets run 5 environments in one line:```for i in {1..5}; do COMPOSE_PROJECT_NAME=env$i docker-compose -f docker-compose-multi.yml up -d; done```
1. Show running containers:```docker ps -a```

##Running Tests

1. Remove containers:```docker rm --force `docker ps -qa````
1. Run compose:```COMPOSE_PROJECT_NAME=test docker-compose -f docker-compose-multi.yml up -d```
1. Run Phantom tests:```docker run --net test_default --link test_hello_1:hello -v `pwd`:/mnt/test cmfatih/phantomjs /usr/bin/phantomjs /mnt/test/test.js```
1. Remove composed application:```docker-compose -f docker-compose-data.yml -p test down```


## TODO: versioning v1+v2, compose options eg scale, extend compose files

##Not covered

* Docker swarm
* Docker machine
* Docker networking
* Other orchestration tools - eg kubernetes or mesos
* https://www.mindmeister.com/389671722/open-container-ecosystem-formerly-docker-ecosystem