FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["cryptothune.Bot/cryptothune.Bot.csproj", "cryptothune.Bot/"]
RUN dotnet restore "cryptothune.Bot/cryptothune.Bot.csproj"
COPY . .
WORKDIR "/src/cryptothune.Bot"
RUN dotnet build "cryptothune.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cryptothune.Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "cryptothune.Bot.dll"]
