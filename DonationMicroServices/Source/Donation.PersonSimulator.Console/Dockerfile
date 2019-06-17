FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Source/Donation.PersonSimulator.Console/Donation.PersonSimulator.Console.csproj", "Source/Donation.PersonSimulator.Console/"]
COPY ["Source/Donation.Model.Lib/Donation.Model.Lib.csproj", "Source/Donation.Model.Lib/"]
COPY ["Source/fAzureCore/fDotNetCoreContainerHelper/fDotNetCoreContainerHelper.csproj", "Source/fAzureCore/fDotNetCoreContainerHelper/"]
COPY ["Source/Donation.Queue.Lib/Donation.Queue.Lib.csproj", "Source/Donation.Queue.Lib/"]
COPY ["Source/fAzureCore/fAzureHelper/fAzureHelper.csproj", "Source/fAzureCore/fAzureHelper/"]
RUN dotnet restore "Source/Donation.PersonSimulator.Console/Donation.PersonSimulator.Console.csproj"
COPY . .
WORKDIR "/src/Source/Donation.PersonSimulator.Console"
RUN dotnet build "Donation.PersonSimulator.Console.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Donation.PersonSimulator.Console.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Donation.PersonSimulator.Console.dll"]