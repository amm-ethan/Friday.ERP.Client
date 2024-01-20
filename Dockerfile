# cd ..
# docker buildx build -t friday-client:1.0.1 -f Friday.ERP.Client/Dockerfile .
 
FROM nginx:alpine AS base
USER $APP_UID
WORKDIR /var/www/web
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Friday.ERP.Client/Friday.ERP.Client/Friday.ERP.Client.csproj", "EPR/Friday.ERP.Client/Friday.ERP.Client/"]
COPY ["Friday.ERP.Server/Friday.ERP.Shared/Friday.ERP.Shared.csproj", "ERP/Friday.ERP.Server/Friday.ERP.Shared/"]

RUN dotnet restore "EPR/Friday.ERP.Client/Friday.ERP.Client/Friday.ERP.Client.csproj"
COPY . .
WORKDIR "/src/Friday.ERP.Client"
RUN dotnet build "Friday.ERP.Client/Friday.ERP.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Friday.ERP.Client/Friday.ERP.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /var/www/web
COPY --from=publish /app/publish/wwwroot .
COPY Friday.ERP.Client/nginx.conf /etc/nginx/nginx.conf