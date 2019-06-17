FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Source/Donation.RestApi.Entrance/Donation.RestApi.Entrance.csproj", "Source/Donation.RestApi.Entrance/"]
COPY ["Source/Donation.Model.Lib/Donation.Model.Lib.csproj", "Source/Donation.Model.Lib/"]
COPY ["Source/fAzureCore/fDotNetCoreContainerHelper/fDotNetCoreContainerHelper.csproj", "Source/fAzureCore/fDotNetCoreContainerHelper/"]
COPY ["Source/Donation.Queue.Lib/Donation.Queue.Lib.csproj", "Source/Donation.Queue.Lib/"]
COPY ["Source/fAzureCore/fAzureHelper/fAzureHelper.csproj", "Source/fAzureCore/fAzureHelper/"]
RUN dotnet restore "Source/Donation.RestApi.Entrance/Donation.RestApi.Entrance.csproj"
COPY . .
WORKDIR "/src/Source/Donation.RestApi.Entrance"
RUN dotnet build "Donation.RestApi.Entrance.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Donation.RestApi.Entrance.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Donation.RestApi.Entrance.dll"]