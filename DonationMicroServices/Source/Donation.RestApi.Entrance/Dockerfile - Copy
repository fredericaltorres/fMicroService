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
ENTRYPOINT ["dotnet", "Donation.RestApi.Entrance.dll"]