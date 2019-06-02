# fMicroService
Experiments building back end with microservices with .NET Core, Docker, Kubernetes on Azure

## AzureServiceBus
This folder contains experimentation with the Azure Service Bus Pub / Sub feature
- Using a pub and a sub console that can be deployed as container
- The sub console can be instantiated multiple time. 
    * The sub console creates dynamically its subscription name based on the name of the machine or POD (In Progress)

- My goal is to demonstrate the ability to scale using Kubernetes and provioning multiple sub console (Not even started)
