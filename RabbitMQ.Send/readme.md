# RabbitMQ tutorial
### Introduction
- Prerequisites  
  `docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management`
## Tutorial - "Hello World!"
## Tutorial - Work Queues
### Round-robin dispatching
- Run 2 workers
  ```shell
  # shell 1
  cd WorkQueues/Worker
  dotnet run
  # => Press [enter] to exit
  ```
  ```shell
  # shell 2
  cd WorkQueues/Worker
  dotnet run
  # => Press [enter] to exit
  ```
- Run NewTask
  ```shell
  # shell 3
  cd WorkQueues/NewTask
  dotnet run "First message."
  dotnet run "Second message.."
  dotnet run "Third message..."
  dotnet run "Fourth message...."
  dotnet run "Fifth message....."
  ```
- Result
  ```shell
  # shell 1
  # => Press [enter] to exit.
  # => [x] Received First message.
  # => [x] Done
  # => [x] Received Third message...
  # => [x] Done
  # => [x] Received Fifth message.....
  # => [x] Done
  ```
  ```shell
  # shell 2
  # => Press [enter] to exit.
  # => [x] Received Second message..
  # => [x] Done
  # => [x] Received Fourth message....
  # => [x] Done
  ```