#
# https://www.digitalocean.com/community/tutorials/docker-explained-using-dockerfiles-to-automate-building-of-images
#
# Start with an image of the dot net core runtime 2.2, Must be first
#
FROM mcr.microsoft.com/dotnet/core/runtime:2.2 

MAINTAINER Frederic Torres

COPY bin/Release/netcoreapp2.2/publish/ app/

ENTRYPOINT ["dotnet", "app/Donation.QueueProcessor.Console.dll"]

