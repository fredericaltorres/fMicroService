FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src

COPY ["Source/Donation.WebDashboard/Donation.WebDashboard.csproj", "Source/Donation.WebDashboard/"]
COPY ["Source/fAzureCore/fAzureHelper/fAzureHelper.csproj", "Source/fAzureCore/fAzureHelper/"]
COPY ["Source/fAzureCore/fDotNetCoreContainerHelper/fDotNetCoreContainerHelper.csproj", "Source/fAzureCore/fDotNetCoreContainerHelper/"]
RUN dotnet restore "Source/Donation.WebDashboard/Donation.WebDashboard.csproj"
COPY . .
WORKDIR "/src/Source/Donation.WebDashboard"
RUN dotnet build "Donation.WebDashboard.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Donation.WebDashboard.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Donation.WebDashboard.dll"]