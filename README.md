#Docker Demo with .NET Core

##Hello World
1. Navigate to api:```cd api```
1. Open VSCode:```code .```
1. The simplest [.NET Core HTTP API, using OWIN](api/Startup.cs)
1. [Dockerfile](api/Dockerfile) - each command is a cached layer
1. Close VSCode
1. List images (layers):```docker images -a```
1. Build image:```docker build -t demo/hello_world .```
1. List images:```docker images -a```
1. Run container:```docker run -t -d -P --name hello_world demo/hello_world```
1. Show running containers view port:```docker ps -a```
1. Show in browser:```http://localhost:xxxx/```
1. Show running containers:```docker ps -a```
1. Stop:```docker stop hello_world```
1. Remove: ```docker rm hello_world```
1. Show running containers:```docker ps -a```
1. Run image with specific port:```docker run -t -d -p 5004:5004 --name hello_world demo/hello_world```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

##Registry (Docker hub)

* Now we have a Docker image, what are we going to do with it?
* Need some way to share images
* Explain how docker changes dev/ops contract from nuget package - docker image
* CI
* Show, inc build settings:```https://hub.docker.com/r/jontymc/hello_world/```

1. Show running containers:```docker ps -a```
1. Pull built image from registry:```docker pull jontymc/hello_world```
1. Run image with specific port:```docker run -t -d -p 5005:5004 --name hello_world2 jontymc/hello_world```
1. Show in browser:```http://localhost:5005/```
1. Stop and remove in one line: ```docker rm --force `docker ps -qa````

##Volumes

* How do we make a change and rebuild the container?

1. Open VSCode:```code .```
1. Change "Hello world" to "Hello Huddle"
1. Close VSCode
1. Rebuild image:```docker build -t demo/hello_world .```
1. Run image:```docker run -t -d -p 5004:5004 --name hello_world demo/hello_world```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

* Having to rebuild the image each time is clunky, let's use volume

1. Remove:```docker rm --force `docker ps -qa````
1. Show running containers:```docker ps -a```
1. Run image from demo 1 with a volume:```docker run -t -d -p 5004:5004 --name hello_world -v `pwd`:/app demo/hello_world```
  * This mounts the host directory inside the container
1. Show running containers:```docker ps -a```
1. Open VSCode:```code .```
1. Change "Hello Huddle" to "Hello world"
1. Close VSCode
1. Restart container:```docker restart hello_world```
1. Show in browser:```http://localhost:5004/```

* We could run application with DNX-watch, which will restart application when any files change
* It is possible to run the entire dev environment inside a docker container
* Eg, here is spotify running from a container:```docker run -it -v /tmp/.X11-unix:/tmp/.X11-unix -e DISPLAY=unix$DISPLAY --device /dev/snd --name spotify jess/spotify```

##Containerized Redis

1. Remove all containers: ```docker rm --force `docker ps -qa````
1. Run redis:```docker run --name redis -d -p 6379:6379 redis```
1. Show running containers:```docker ps -a```
1. Run cli:```docker run -it --link redis:redis --rm redis sh -c 'exec redis-cli -h "$REDIS_PORT_6379_TCP_ADDR" -p "$REDIS_PORT_6379_TCP_PORT"'```
1. Insert message:```SET message "Hello from Redis"```
1. Get it back:```GET message```
1. Exit
1. Show running with -rm flag removes container when stopped:```docker ps -a```
1. Navigate to api_redis:```cd ../api_redis```
1. Open VSCode:```code .```
1. Show added reference to stack exchange in [project.json](api_redis/project.json)
1. Show [redis code in api](api_redis/startup.cs)
1. Close VSCode
1. Build image:```docker build -t demo/hello_world .```
1. Run image:```docker run -t -d -p 5004:5004 --name hello_world demo/hello_world```
1. Show running containers:```docker ps -a```
1. Show in browser:```http://localhost:5004/```

##Managing Multiple Containers with Compose

1.