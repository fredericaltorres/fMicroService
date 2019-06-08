FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Source/Donation.RestApi.Entrance/Donation.RestApi.Entrance.csproj", "Source/Donation.RestApi.Entrance/"]
RUN dotnet restore "Source/Donation.RestApi.Entrance/Donation.RestApi.Entrance.csproj"
COPY . .
WORKDIR "/src/Source/Donation.RestApi.Entrance"
RUN dotnet build "Donation.RestApi.Entrance.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Donation.RestApi.Entrance.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

# ssl - https://mikewilliams.io/net-core-2-1-and-docker-how-to-get-docker-to-recognize-a-local-ssl-certificate-6e637e1e8800

# https://stackoverflow.com/questions/48669548/why-does-aspnet-core-start-on-port-80-from-within-docker/48669703
#ENV ASPNETCORE_URLS=https://+:443
# ENV ASPNETCORE_URLS: http://+:80 # This is the default
#"--server.urls", "http://*:5000"
ENTRYPOINT ["dotnet", "Donation.RestApi.Entrance.dll"]
