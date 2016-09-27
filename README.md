#Docker Demo with .NET Core

* I'm going to show some slides to introduce Docker concepts
* But then I'm going to do some demos to show you how easy it is to work with
* And hopefully demonstrate the benefits it brings to the development process
* There's a lot to get through, but stop me if I'm going to fast.
* ```xhost local:root```
* ```cd dev/docker_demo```
* First of all, I'm gonna run the slides using Chrome in a Docker container!:```docker run -it -v /tmp/.X11-unix:/tmp/.X11-unix -e DISPLAY=unix$DISPLAY -v `pwd`/slides:/slides --rm --name chrome jess/chrome --user-data-dir=/data --app=file:///slides/index.html --no-first-run --start-maximized```

##What is Docker?

* High level API and tooling to manage containers

* Containers are like virtual machines, but they share a kernel and re-use parts of their images which enables them to be much faster and streamlined
  * Like virtual machines, they provide isolation of processes and you can run multiple containers on a single host

* They can be shared and run anywhere
    * You no longer have to worry about what software, or particular versions of software, is installed when deploying, because they are packaged with your application

* They work on every machine

##Dependency Hell

* From local to test to live, we have to manage a lot of dependencies
* The biggest problem is locally, where we have everything installed on one box
* Managing these dependencies costs us a lot of time and money
* We've all lost countless hours setting up our machines or trying to work out why our code doesn't work on another environment or someone elses machine
docker 
* With Docker we no longer deploy app onto an environment, we deploy the app with the environment 
* The container has everything it needs to run packaged with it and it's guaranteed to be the same for everybody
* And it runs in the same way on local, test and live
* This fundamentally changes the contract between development and operations
* We would no longer deploy nuget packages onto an environment we hope has all the right things installed
* Instead we deploy the container and all its dependencies as one package
* This gives us much more confidence that what worked locally will work live
* There are other benefits to the isolation that containers bring, for example:
    * We can upgrade a particular dependency without having to retest everything, as it is isolated to the app that depends on it
    * We can control the resources that a container uses, CPU/memory/disk IO/etc
    * We can restrict the scope of what that application can do on the host

##VMs vs Containers

* Container share the same kernel, so are much faster than VMs to start and stop
* They also share code where possible so are much smaller
* Because they share a kernel, you can't run windows containers on linux and vice versa

* With VMS, you have to create an entire new image for any modification
* Container images are just files and modifications to them are stored as diffs, similar to git
* In this way Docker is a bit like git for environments

##How are containers built?

* Every container has a Dockerfile, which contains the instructions of how to build the container
* Using the Docker engine, we can create a container image from the Dockerfile
* This can then be pushed to a central registry, similar to nuget
* 3rd parties can then pull and run the same image
* Only the diffs between images are pulled, so this is quite efficient and quick
* Now I'm going to show you how this all works

#Demo Time

##Hello World
* Ensure images are cleared except for those needed
* ```docker rmi demo/hello```
* ```docker pull microsoft/aspnet:1.0.0-rc1-final```
* ```docker pull jontymc/hello```
* ```docker pull jontymc/vscode_aspnet```
* ```docker pull jess/chrome```
* ```docker pull jess/spotify```
* ```docker pull redis```

1. Explain [.NET Core HTTP API, using OWIN](api/Startup.cs)
  * Simplest possible .NET Core API
1. Explain [Dockerfile](api/Dockerfile)
  * Inherits from dotnet core linux
  * Each command is a cached layer
  * DNU restore restores all packages in the project.json file
  * So, we copy project.json first so we don't have to re-pull packages every time we rebuild the code layer
1. List images (layers):```docker images -a```
1. Build image:```docker build -t demo/hello .```
1. List images:```docker images -a```
1. Run container:```docker run -d -P --name hello demo/hello```
1. Show containers (point out dynamically assigned port):```docker ps -a```
1. Show in browser:```http://localhost:xxxx/```
1. Show logs:```docker logs hello```
1. Show info, eg ip:```docker inspect hello```
1. Show stats, ```docker stats```
1. Open another console window
1. Run ```docker events```
1. In original window ```docker stop hello```
1. ```docker start hello```
1. Show running containers:```docker ps -a```
1. Close other window
1. Stop:```docker stop hello```
1. Remove: ```docker rm hello```
1. Show running containers:```docker ps -a```
1. Run image with specific port:```docker run -d -p 5020:5020 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5020/```

##Make Changes to a Container

* How do we make a change and rebuild the container?

1. Change "Hello from ASP.NET Core!" in api code
1. Rebuild image:```docker build -t demo/hello .```
1. Run image:```docker run -d -p 5020:5020 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5020/```

* Having to rebuild the image each time is clunky
* Instead we can mount a host directory inside the container

1. Remove:```drmf```
1. Show running containers:```docker ps -a```
1. Run image from demo 1 with a host mount:```docker run -d -p 5020:5020 --name hello -v `pwd`:/app demo/hello```
1. Show running containers:```docker ps -a```
1. Change api code again
1. Restart container:```docker restart hello```
1. Show in browser:```http://localhost:5020/```

##Containerized Redis

1. Remove all containers:```drmf```
1. Run redis:```docker run --name redis -d -p 6379:6379 redis```
1. Show logs:```docker logs redis```
1. Show running containers:```docker ps -a```
1. Run cli:```docker run -it --link redis:redis --rm redis sh -c 'exec redis-cli -h redis'```
  * "link" flag means map the "redis" container to the hostname "redis" inside the container
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit
1. Show running with -rm flag removes container when stopped:```docker ps -a```
  * This is example of running a disposable container
  * Docker containers are not only for daemonized applications but also for adhoc applications and scripts
  * Docker removes the dependency nightmare for running anything  
1. Show added reference to stack exchange in [project.json](api_redis/project.json)
1. Show [redis code in api](api_redis/startup.cs)
1. Change directory:```cd ../api_redis```
1. Build image:```docker build -t demo/hello .```
1. Run image:```docker run -d -p 5020:5020 --link redis:redis --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5020/```

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
1. Show in browser:```http://localhost:5020/```
  * No data, because redis container has been killed and run again
  * Ideally we need a way of persisting data
1. Insert message:```SET message "Hello from Redis"```
1. Remove composed application:```docker-compose down```

##Persistence with Data Volumes

* Volumes allow data to be shared between containers
* Volumes persist event if the container has been deleted

1. List existing volumes:```docker volumes ls```
  * These are volumes created from previous containers
  * Volumes are declared in docker file: https://github.com/docker-library/redis/blob/a38166e6f3430512ba8ce2cb5ebd889ee17b9dc4/3.2/Dockerfile
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
1. Remove redis:```docker rm -f redis```
1. But we still have volume:```docker volumes ls```

* We can now use this volume in our compose setup

1. Show [docker-compose-data.yml](docker-compose-data.yml)
1. Run compose:```docker-compose -f docker-compose-data.yml up -d```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5020/```
1. Remove composed application:```docker-compose down```

* We didn't actually need to create the volume, as it would be automatically created through compose up
* Data volumes can be used for backup, restore and migrations
* Can be used to store test data
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
  * If we dont include env variable, project name is based on parent directory, hence dockerdemo previouslys
1. Show running containers:```docker ps -a```
1. Show networks:```docker network ls```
1. Show volumes:```docker volume ls```
1. Run another environment:```COMPOSE_PROJECT_NAME=env2 docker-compose -f docker-compose-multi.yml up -d```
1. Show running containers:```docker ps -a```
1. Show networks:```docker network ls```
1. Remove containers:```docker rm --force `docker ps -qa````
1. Lets run 5 environments in one line:```for i in {1..5}; do COMPOSE_PROJECT_NAME=env$i docker-compose -f docker-compose-multi.yml up -d; done```
1. Show running containers:```docker ps -a```

* Possible to override docker compose

##Running Tests

1. Remove containers:```docker rm --force `docker ps -qa````
1. Run compose:```COMPOSE_PROJECT_NAME=test docker-compose -f docker-compose-multi.yml up -d```
1. Run Phantom tests:```docker run --rm --net test_default --link test_hello_1:hello -v `pwd`:/mnt/test cmfatih/phantomjs /usr/bin/phantomjs /mnt/test/test.js```
1. Remove composed application:```docker-compose -f docker-compose-data.yml -p test down```

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

* TODO: Teamcity / local registry

##Windows containers

* http://26thcentury.com/2016/01/03/dockerfile-to-create-sql-server-express-windows-container-image/
* Dockerfile for sql server: https://github.com/brogersyh/Dockerfiles-for-windows/blob/master/sqlexpress/dockerfile

1. Remote onto azure machine (Windows Server 2016 TP4 - comes with Docker installed)
1. Search for windows images:```docker search *```
1. Show local images:```docker images```
1. Run sql in container:```docker run --name sql -d -p 1433:1433 -v c:\sql:c:\sql sqlexpress```
1. Run another sql in container:```docker run --name sql -d -p 1434:1433 -v c:\sql2:c:\sql sqlexpress```
1. Run SQL management studio, log on with ip,port and sa thepassword2#

* Can't get .net core container to build :( However, the commands work when run manually...

1. Show startup.cs with sql code
1. Run aspnet container mounted against host:```docker run -it -p 80:80 -v c:\app:c:\app --name hello --rm microsoft/aspnet cmd```

##Docker benefits

* Repeatability
  * We never have to worry about environment being setup correctly or having the right version of a dependency
  * Gives us more confidence that if it works on one machine, it will work on any machine
* Isolation
  * Software running in dockerone container is not going to affect other containers
  * We can upgrade dependencies in one container independently of all the others
  * We can run mutliple environments on a single machine using Dockers network virtualization
* Speed
  * Containers include the minimal runtime requirements of the application, so can be deployed quickly
* Automation
  * Greatly reduces the friction in automating deployments
  * Makes it much easier to build distributed applications - and we will cover this in more detail in the next talk on Service Discovery and Orchestration with Docker
  * Using the Docker API and Compose, it is simple to define and run complicated applications and networks with multiple containers
  * Unified deployment platform
* Portabliliy
  * Can deploy any container onto any machine running Docker
* Version tracking
  * Each image has a version number, so you know exactly what it contains
  * Versions are tracked and available for everyone in central repository


## TODO: compose options eg scale, extend compose files

##Security features

* Docker daemon currently requires root privileges, so only trusted users should have access
* Namespaces mean processes cannot interact with each other
* Each container has its own network stack
* Control groups manage memory, CPU and disk I/O usage
* Docker are working on code to search images in docker hub for vunerabilities

###1.9

* Networking

###1.10

* Granular permissions on system calls with security profiles
* Multiple user namespaces on single host
* Authorization plugins


##Not covered

* Docker swarm
* Docker machine
* Other orchestration tools - eg kubernetes or mesos
* https://www.mindmeister.com/389671722/open-container-ecosystem-formerly-docker-ecosystem