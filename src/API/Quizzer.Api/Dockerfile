#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 50000
EXPOSE 50001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/API/Quizzer.Api/Quizzer.Api.csproj", "src/API/Quizzer.Api/"]
COPY ["src/Modules/Identity/Modules.Identity/Modules.Identity.csproj", "src/Modules/Identity/Modules.Identity/"]
COPY ["src/Shared/SharedKernel.Core/SharedKernel.Core.csproj", "src/Shared/SharedKernel.Core/"]
COPY ["src/Shared/SharedKernel.Infrastructure/SharedKernel.Infrastructure.csproj", "src/Shared/SharedKernel.Infrastructure/"]
COPY ["src/Modules/Question/Modules.Question.Endpoints/Modules.Question.Endpoints.csproj", "src/Modules/Question/Modules.Question.Endpoints/"]
COPY ["src/Modules/Question/Modules.Question.Application/Modules.Question.Application.csproj", "src/Modules/Question/Modules.Question.Application/"]
COPY ["src/Shared/common/common.csproj", "src/Shared/common/"]
COPY ["src/Modules/Question/Modules.Question.Core/Modules.Question.Core.csproj", "src/Modules/Question/Modules.Question.Core/"]
COPY ["src/Modules/Question/Modules.Question.Infrastructure/Modules.Question.Infrastructure.csproj", "src/Modules/Question/Modules.Question.Infrastructure/"]
RUN dotnet restore "./src/API/Quizzer.Api/Quizzer.Api.csproj"
COPY . .
WORKDIR "/src/src/API/Quizzer.Api"
RUN dotnet build "./Quizzer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Quizzer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quizzer.Api.dll"]