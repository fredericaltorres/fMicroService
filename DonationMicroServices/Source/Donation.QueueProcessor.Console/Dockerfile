FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Source/Donation.QueueProcessor.Console/Donation.QueueProcessor.Console.csproj", "Source/Donation.QueueProcessor.Console/"]
COPY ["Source/Donation.Model.Lib/Donation.Model.Lib.csproj", "Source/Donation.Model.Lib/"]
COPY ["Source/fAzureCore/fDotNetCoreContainerHelper/fDotNetCoreContainerHelper.csproj", "Source/fAzureCore/fDotNetCoreContainerHelper/"]
COPY ["Source/Donation.Table.Lib/Donation.Table.Lib.csproj", "Source/Donation.Table.Lib/"]
COPY ["Source/fAzureCore/fAzureHelper/fAzureHelper.csproj", "Source/fAzureCore/fAzureHelper/"]
COPY ["Source/fAzureCore/AzureServiceBusSubHelper/AzureServiceBusSubHelper.csproj", "Source/fAzureCore/AzureServiceBusSubHelper/"]
COPY ["Source/Donation.Queue.Lib/Donation.Queue.Lib.csproj", "Source/Donation.Queue.Lib/"]
RUN dotnet restore "Source/Donation.QueueProcessor.Console/Donation.QueueProcessor.Console.csproj"
COPY . .
WORKDIR "/src/Source/Donation.QueueProcessor.Console"
RUN dotnet build "Donation.QueueProcessor.Console.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Donation.QueueProcessor.Console.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Donation.QueueProcessor.Console.dll"]