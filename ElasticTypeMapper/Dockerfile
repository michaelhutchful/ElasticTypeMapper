FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ElasticTypeMapper.csproj ElasticTypeMapper/
RUN dotnet restore ElasticTypeMapper/ElasticTypeMapper.csproj
COPY . .
WORKDIR /src/ElasticTypeMapper
RUN dotnet build ElasticTypeMapper.csproj -c Release -f netcoreapp2.1 -r ubuntu.16.04-x64 -o /app

FROM build AS publish
RUN dotnet publish -c Release -f netcoreapp2.1 -r ubuntu.16.04-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet","ElasticTypeMapper.dll"]
