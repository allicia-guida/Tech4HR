# Imagem da Tech4Hr.API para publicar em um servidor (Render, Azure ou outro).
# O contexto de build é a pasta Tech4Hr.API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Tech4Hr.API.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
# O HTTPS é terminado pelo servidor na frente do container: confia nos cabeçalhos X-Forwarded-*.
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "Tech4Hr.API.dll"]
