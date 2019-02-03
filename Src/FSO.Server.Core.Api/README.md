# Getting Started

## Prerequisites
- .NET Core 2.2 or above

## Connect to database

You will need to connect to the database similary as you did with the server. Go to ``appsettings.json`` and locate  ``connectionString`` you can enter the same information for your database as you did with game server.

## Running the server

NSO uses .NET Core for it's API server. You can start using the following methods:

- ``dotnet run FSO.Server.Api.dll`` for all supported systems.
- ``screen -S NsoApi -d -m dotnet run FSO.Server.Api.dll`` on Unix systems, if you want to run it through a virtual terminal.

[Docker](https://www.docker.com/) is also possible, but remains untested.