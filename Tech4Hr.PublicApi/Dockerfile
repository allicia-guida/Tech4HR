FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY Tech4Hr.PublicApi.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app .
RUN addgroup -S app && adduser -S app -G app && mkdir -p /data && chown -R app:app /app /data
USER app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ConnectionStrings__PublicApi="Data Source=/data/tech4hr-public.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "Tech4Hr.PublicApi.dll"]
