FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
#WORKDIR /src
#COPY ElasticTypeMapper.csproj ElasticTypeMapper/
#RUN dotnet restore ElasticTypeMapper/ElasticTypeMapper.csproj
#COPY . .
WORKDIR /src/ElasticTypeMapper
COPY . .
RUN dotnet build ElasticTypeMapper.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ElasticTypeMapper.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ElasticTypeMapper.dll"]
