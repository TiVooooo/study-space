#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StudySpace.API/StudySpace.API.csproj", "StudySpace.API/"]
COPY ["StudySpace.Common/StudySpace.Common.csproj", "StudySpace.Common/"]
COPY ["StudySpace.Data/StudySpace.Data.csproj", "StudySpace.Data/"]
COPY ["StudySpace.DTOs/StudySpace.DTOs.csproj", "StudySpace.DTOs/"]
COPY ["StudySpace.Service/StudySpace.Service.csproj", "StudySpace.Service/"]
RUN dotnet restore "./StudySpace.API/StudySpace.API.csproj"
COPY . .
WORKDIR "/src/StudySpace.API"
RUN dotnet build "./StudySpace.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StudySpace.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudySpace.API.dll"]
