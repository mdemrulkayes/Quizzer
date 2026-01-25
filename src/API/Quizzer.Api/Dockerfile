# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/API/Quizzer.Api/Quizzer.Api.csproj", "src/API/Quizzer.Api/"]
COPY ["src/Modules/Identity/Modules.Identity/Modules.Identity.csproj", "src/Modules/Identity/Modules.Identity/"]
COPY ["src/Shared/Shared.Core/Shared.Core.csproj", "src/Shared/Shared.Core/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]
COPY ["src/Modules/Quiz/Modules.Quiz.Endpoints/Modules.Quiz.Endpoints.csproj", "src/Modules/Quiz/Modules.Quiz.Endpoints/"]
COPY ["src/Modules/Quiz/Modules.Quiz.Application/Modules.Quiz.Application.csproj", "src/Modules/Quiz/Modules.Quiz.Application/"]
COPY ["src/Shared/Shared.Application/Shared.Application.csproj", "src/Shared/Shared.Application/"]
COPY ["src/Modules/Quiz/Modules.Quiz.Core/Modules.Quiz.Core.csproj", "src/Modules/Quiz/Modules.Quiz.Core/"]
COPY ["src/Modules/Quiz/Modules.Quiz.Infrastructure/Modules.Quiz.Infrastructure.csproj", "src/Modules/Quiz/Modules.Quiz.Infrastructure/"]
RUN dotnet restore "./src/API/Quizzer.Api/Quizzer.Api.csproj"
COPY . .
WORKDIR "/src/src/API/Quizzer.Api"
RUN dotnet build "./Quizzer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Quizzer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quizzer.Api.dll"]