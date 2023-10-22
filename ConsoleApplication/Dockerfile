FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

COPY ../localhost.crt /usr/local/share/ca-certificates/localhost.crt
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConsoleApplication/ConsoleApplication.csproj", "ConsoleApplication/"]
RUN dotnet restore "ConsoleApplication/ConsoleApplication.csproj"
COPY . .
WORKDIR "/src/ConsoleApplication"
RUN dotnet build "ConsoleApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsoleApplication.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleApplication.dll"]
