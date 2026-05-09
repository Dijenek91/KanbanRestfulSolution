# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
#USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Copying the project files first to leverage Docker cache for restoring nuget packages
# Since the dockerfile is in KanbanRestService folder, this method needs to be used in solution root (not project root): docker build -f KanbanRestService/Dockerfile -t kanbanrestservice:dev . 
COPY ["KanbanRestService/KanbanRestService.csproj", "KanbanRestService/"]
COPY ["KanbanInfrastructure/KanbanInfrastructure.csproj", "KanbanInfrastructure/"]
COPY ["KanbanModel/KanbanModel.csproj", "KanbanModel/"]
COPY ["MigrationPostgresSql/MigrationPostgresSql.csproj", "MigrationPostgresSql/"]
COPY ["MigrationSqlServer/MigrationSqlServer.csproj", "MigrationSqlServer/"]

RUN dotnet restore "KanbanRestService/KanbanRestService.csproj" #runs download of nuget packages (dependencies) for all projects in the solution, but using the service project as entry point for restore to leverage caching of nuget packages in case of changes in other projects
COPY . .
WORKDIR "/src/."
RUN dotnet build "KanbanRestService/KanbanRestService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "KanbanRestService/KanbanRestService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KanbanRestService.dll"]