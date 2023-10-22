FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
WORKDIR /src
COPY ["SinglePageApplication/", "SinglePageApplication/"]
WORKDIR "SinglePageApplication"
RUN dotnet restore "SinglePageApplication.csproj"
RUN dotnet publish "SinglePageApplication.csproj" -c Release -o /app/publish
FROM nginx:1 AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .

