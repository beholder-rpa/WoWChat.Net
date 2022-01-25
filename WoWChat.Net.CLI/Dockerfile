#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WoWChat.Net-CLI/WoWChat.Net.CLI.csproj", "WoWChat.Net.CLI/"]
RUN dotnet restore "WoWChat.Net.CLI/WoWChat.Net.CLI.csproj"
COPY . .
WORKDIR "/src/WoWChat.Net.CLI"
RUN dotnet build "WoWChat.Net.CLI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WoWChat.Net.CLI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WoWChat.Net.CLI.dll"]