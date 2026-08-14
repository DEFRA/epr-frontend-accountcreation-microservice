FROM defradigital/dotnetcore:dotnet10.0 AS base
USER root
ENV ASPNETCORE_URLS=http://*:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
EXPOSE 8080

RUN apk add --no-cache icu-libs icu-data-full libintl tzdata

FROM defradigital/dotnetcore-development:dotnet10.0 AS build
USER root
WORKDIR /src
RUN apk add --no-cache nodejs npm
COPY ["FrontendAccountCreation.Web/FrontendAccountCreation.Web.csproj", "FrontendAccountCreation.Web/"]
COPY ["FrontendAccountCreation.Core/FrontendAccountCreation.Core.csproj", "FrontendAccountCreation.Core/"]
RUN dotnet restore "FrontendAccountCreation.Web/FrontendAccountCreation.Web.csproj"

COPY FrontendAccountCreation.Web/. ./FrontendAccountCreation.Web/.
COPY FrontendAccountCreation.Core/. ./FrontendAccountCreation.Core/.

WORKDIR "/src/FrontendAccountCreation.Web"

FROM build AS publish
RUN dotnet publish "FrontendAccountCreation.Web.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER dotnet
ENTRYPOINT ["dotnet", "FrontendAccountCreation.Web.dll"]
