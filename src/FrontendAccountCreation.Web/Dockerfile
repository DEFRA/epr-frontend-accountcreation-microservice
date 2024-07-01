FROM defradigital/dotnetcore:dotnet8.0 AS base
USER root
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

RUN apk update && apk --no-cache add icu-libs icu-data-full libintl
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

FROM defradigital/dotnetcore-development:dotnet8.0 AS build
USER root
WORKDIR /src
RUN apk update && apk --no-cache add nodejs npm
COPY ["FrontendAccountCreation.Web/FrontendAccountCreation.Web.csproj", "FrontendAccountCreation.Web/"]
COPY ["FrontendAccountCreation.Core/FrontendAccountCreation.Core.csproj", "FrontendAccountCreation.Core/"]
RUN dotnet restore "FrontendAccountCreation.Web/FrontendAccountCreation.Web.csproj"

COPY --chown=dotnet:dotnet FrontendAccountCreation.Web/. ./FrontendAccountCreation.Web/.
COPY --chown=dotnet:dotnet FrontendAccountCreation.Core/. ./FrontendAccountCreation.Core/.

WORKDIR "/src/FrontendAccountCreation.Web"
RUN dotnet build "FrontendAccountCreation.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FrontendAccountCreation.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER dotnet
ENTRYPOINT ["dotnet", "FrontendAccountCreation.Web.dll"]
