#
# https://www.digitalocean.com/community/tutorials/docker-explained-using-dockerfiles-to-automate-building-of-images
#
# Start with an image of the dot net core runtime 2.2, Must be first
#
FROM mcr.microsoft.com/dotnet/core/runtime:2.2 

MAINTAINER Frederic Torres

#RUN echo The command RUN is executed at run time

# Set working directory for CMD command
#WORKDIR ~/
#CMD ls

# Set environment variable
#ENV Fred true

# Run a bash command as part of the image building...
# It is a Linux bash command
#RUN echo Add local file to image

# Add local file to image in the default directory where all the .exe and .dll will be layout
#ADD ./data.txt app/

# Create sub directory in app folder and copy data.txt
#RUN mkdir app/tutu
#ADD ./data.txt app/tutu

COPY bin/Release/netcoreapp2.2/publish/ app/

ENTRYPOINT ["dotnet", "app/AzureServiceBusSubConsole.dll"]
#ENTRYPOINT ["/bin/bash", ""]

# Run the image locally
# docker run azureservicebussubconsole
#

