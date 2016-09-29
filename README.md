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
  * Show in docker hub https://hub.docker.com/r/microsoft/dotnet/
  * Each command is a cached layer
  * DNU restore restores all packages in the project.json file
  * So, we copy project.json first so we don't have to re-pull packages every time we rebuild the code layer
1. List images (layers):```docker images -a```
1. Build image:```docker build -t demo/hello .```
1. List images:```docker images -a```
1. Run container:```docker run -d -P --name hello demo/hello```
1. Show containers (point out dynamically assigned port):```docker ps -a```
1. Show curl:```curl localhost:xxxx```
1. Show logs:```docker logs hello```
1. Show info, eg ip:```docker inspect hello```
1. Show stats, ```docker stats```
1. Open another console window
1. Run ```docker events```
1. Show exec a command inside running container: ```docker exec -it hello /bin/bash```
1. In original window ```docker stop hello```
1. ```docker start hello```
1. Show running containers:```docker ps -a```
1. Close other window
1. Stop:```docker stop hello```
1. Remove: ```docker rm hello```
1. Show running containers:```docker ps -a```
1. Run image with specific port:```docker run -d -p 5020:5020 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show:```curl localhost:5020```

##Make Changes to a Container

* How do we make a change and rebuild the container?

1. Change "Hello from ASP.NET Core!" in api code
1. Rebuild image:```docker build -t demo/hello .```
1. Run image:```docker run -d -p 5020:5020 --name hello demo/hello```
1. Show running containers:```docker ps -a```
1. Show in browser:```curl localhost:5020``

* Having to rebuild the image each time is clunky
* Instead we can mount a host directory inside the container

1. Remove:```drmf```
1. Show running containers:```docker ps -a```
1. Run image from demo 1 with a host mount:```docker run -d -p 5020:5020 --name hello -v `pwd`:/app demo/hello```
1. Show running containers:```docker ps -a```
1. Change api code again
1. Restart container:```docker restart hello```
1. Show:```curl localhost:5020```

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
1. Show:```curl localhost:5020```

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
1. Show:```curl localhost:5020```
  * No data, because redis container has been killed and run again
  * Ideally we need a way of persisting data
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
1. Show in browser:```curl localhost:5020```
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

* Possible to extend docker compose using another file, so you could have a production compose and a local or testing override compose that will build one or more of the images

##Running Tests

1. Remove containers:```docker rm --force `docker ps -qa````
1. Run compose:```COMPOSE_PROJECT_NAME=test docker-compose -f docker-compose-multi.yml up -d```
1. Run Phantom tests:```docker run --rm --net test_default --link test_hello_1:hello -v `pwd`:/mnt/test cmfatih/phantomjs /usr/bin/phantomjs /mnt/test/test.js```
1. Remove composed application:```docker-compose -f docker-compose-data.yml -p test down```

##Logging

1. Show elk compose and logstash config
1. Run elk: ```cd elk``` ```docker-compose up -d```
1. Show containers: ```docker ps -a```
1. Show kibana: ```http://localhost:5601```
1. Show logging compose - gelf logging
1. Run logging: ```cd ../logging``` ```docker-compose up -d```
1. Logs: ```docker-compose logs```
1. Show kibana: ```http://localhost:5601```

##Docker orchestration

* (pre-run this: ```for N in 1 2 3 4 5; do docker-machine create --driver virtualbox node$N; done```)
* Now we can easily run full applications with a single command
* We want to provision new boxes to run docker containers on
* Docker has docker-machine which can provision new docker hosts

1. Run ```docker-machine ls```
1. Can see I previously created 5 machines
1. Let's create a new machine locally using the virtualbox driver: ```docker-machine create --driver virtualbox demo1```
  * There are many drivers, eg aws https://docs.docker.com/machine/drivers/aws/
1. Open new terminal
1. Connect to existing: ```docker-machine ssh node1```
1. Show docker running: ```docker info```
1. Exit

* Now we can provision machines, we want to deploy containers too them
* Let's create a cluster of machines using docker swarm

1. Run ```docker-machine ls```
1. Get ip for node1: ```docker-machine ip node1```
1. SSH onto node1: ```docker-machine ssh node1```
1. Initialize swarm: ```docker swarm init --advertise-addr [ip-address]```

* output is the key required to join this cluster

1. Open new terminal
1. ssh onto node2: ```docker-machine ssh node2```
1. join cluster as worker, eg: ```docker swarm join \
    --token SWMTKN-1-4j35tekvgp868z0ew096mykzziyibfoapiw4b4kkh0oij8sdwe-amkvb95iog9od0zk5yffqedj9 \
    192.168.99.101:2377```
1. Exit
1. Do same for other nodes, but quicker:
  * ```for NODE in node3 node4 node5; do```
  * ```docker-machine ssh $NODE ```
  * Then paste join token command, eg: ```docker swarm join \
        --token SWMTKN-1-4j35tekvgp868z0ew096mykzziyibfoapiw4b4kkh0oij8sdwe-amkvb95iog9od0zk5yffqedj9 \
          192.168.99.101:2377```
  * ```done```
1. Lets list the nodes in cluster: ```docker-machine ssh node1 docker node ls```
1. See that node1 is master
1. Promote node 2,3 to master: ```docker-machine ssh node1 docker node promote node2 node3```
1. List the nodes in cluster again: ```docker-machine ssh node1 docker node ls```
  * Managers are responsible for maintaining the cluster
  * If the leader manager goes down, another will be elected the new leader
1. Stop node 1: ```docker-machine stop node1```
1. List from node2: ```docker-machine ssh node2 docker node ls```
1. Start node 1: ```docker-machine start node1```

* Swarm introduces new primitives to docker
* Node: ```A node is an instance of Docker Engine participating in a swarm.```
* Service: ```A service is the definition of the tasks to execute on the worker nodes. It is the central structure of the swarm system and the primary root of user interaction with the swarm.```
* Task: ```A task carries a Docker container and commands to run inside the container. It is the atomic scheduling unit of swarm.```

* Let's create a service:

1. Node1: ```docker-machine ssh node1```
1. Create: ```docker service create --name Web --publish 80:80 --replicas=3 nginx:latest```
  * Swarm will ensure we have 3 instances of the service running across our nodes
1. List our web services: ```docker service ps Web```
  * Shows which nodes are running the service
1. Ports are published to all nodes in the cluster: ```curl localhost:80```
1. Show containers: ```docker ps -a```
1. Kill container: ```docker kill [container ID]```
1. Swarm has reprovisioned an nginx container: ```docker service ps Web```
1. Scale up: ```docker service update Web --replicas 5```
1. Remove: ```docker service rm Web```
1. Can upgrade services, use specific version of nginx: ```docker service create --name Web --publish 80:80 --replicas 3 nginx:1.10.1```
1. Show running: ```docker service ps Web```
1. Update to later verions: ```docker service update Web --image nginx:1.11.3```
1. Can see it updates each task individually: ```docker service ps Web```
1. Remove: ```docker service rm Web```
1. Can create global service: ```docker service create --name Web --mode global --publish 80:80 nginx:latest```
1. Remove: ```docker service rm Web```

* Other things swarm can do:
* If a node fails swarm will reallocated work on another node
* To service a node, you can drain the node
* You can define networks to span the hosts you want

##Private Registry

* (Pre-pull microsoft/dotnet:latest)
* Images are cached per node
* We don't want to pull from docker hub in each node
* Instead we can create a private registry shared between the nodes

1. Create registry: ```docker service create --name registry -p 5000:5000 registry:2```
  * Run this as a service
  * Swarm will insure there is always an instance of it running
1. List services: ```docker service ls```
1. Pull an image from docker hub: ```docker pull alpine```
1. Tag the image for our registry: ```docker tag alpine localhost:5000/alpine```
1. Push to the registry: ```docker push localhost:5000/alpine```
1. List images: ```curl localhost:5000/v2/_catalog```
1. Open new terminal
1. SSH onto node2: ```docker pull localhost:5000/alpine```
1. List images: ```docker images```

* Can provide different storage backend for the registry out of the box, eg can use S3 to stort images
* Supports let's encrypt for TLS
* Alternatively, artifactory can be used as a registry

* Normal workflow would be for CI to push images to a registry
* Docker changes dev/ops contract from nuget package to docker image

* Run teamcity in container: ```docker run -d --name teamcity-server -p 8111:8111 jetbrains/teamcity-server```
* Dockerize play: ```cd ~/test-play``` ```sbt docker:publishLocal```

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
  * Most orchestration and infrastructure software now supports running containers
* Version tracking
  * Each image has a version number, so you know exactly what it contains
  * Versions are tracked and available for everyone in central repository

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