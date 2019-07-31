# fMicroService
In this repo I am experimenting building microservices with 
- .NET Core
- Docker
- Kubernetes on Azure

My goal is to build a case study that I can use to evaluate the scalability possibilities of the technologies mentioned above.

## On line donation back end case study
I am going to build a back end able to received and processed donations.
The donation should be created by hundreds of users entering donation amounts
and credit card information on a web site and press send.

### The Person Simulator
I will not build the front end, but rather create a .NET Core console application
that can be instantiated up to 10 times in Docker containers instance in an Azure Kubernetes cluster.
Each instance will read a specific local donation[X].json file containing 50 000
donations and execute an HTTP post to a specific end point for each donations.
10 HTTP post are executed in parallel. 
For every 500 donations sent, the application send to an Azure Service Bus channel (Publisher/Subscribers) some performance information.

* [Source Code](https://github.com/fredericaltorres/fMicroService/tree/master/DonationMicroServices/Source/Donation.QueueProcessor.Console)

### The Rest Api
A .NET Core REST API will implement the HTTP post to received the donations.
Multiple instances of the API process will be executed in a Docker containers
behind a load balancer provisioned using am Azure Kubernetes cluster.
When a donation is received, it is 
- Validated
- Push to an Azure Queue
- For every 500 donations received, the endpoint send to an Azure Service Bus channel (Publisher/Subscribers) some performance information.

* [Source Code](https://github.com/fredericaltorres/fMicroService/tree/master/DonationMicroServices/Source/Donation.RestApi.Entrance)

### The Queue Processor
A .NET Core console application that can be instantiated multiple times as Docker containers instances in an Azure Kubernetes cluster will
- Pop messages from the Azure Queue
- Validate the data
- Store the data in an Azure Table
- Compute an aggregate of the amount received per country, store the data into another Azure Table.
- For every 500 donations processed, the application send to an Azure Service Bus channel (Publisher/Subscribers) the aggregated information and other performance information.

* [Source Code](https://github.com/fredericaltorres/fMicroService/tree/master/DonationMicroServices/Source/Donation.QueueProcessor.Console)

### The Web Dashboard
A ASP.NET Core Web Application implementing
- An internal endpoint named SystemActivitiesController will
    * Received the information sent by the the different processes to the Azure Service Bus channel
    * Store and aggregate the data in static dictionaries
    * Communicate the information the Dashboard browser side 

    * [SystemActivitiesController source code](https://github.com/fredericaltorres/fMicroService/blob/master/DonationMicroServices/Source/Donation.WebDashboard/Controllers/SystemActivitiesController.cs)

- A Web Dashboard written as a Single Page Application (SPA) in React that display the performance informations sent by the different processes and the donation amount per country in charts and tables in `pseudo real time`.

    * [Web Dashboard React Code](https://github.com/fredericaltorres/fMicroService/blob/master/DonationMicroServices/Source/Donation.WebDashboard/ClientApp/src/components/Home.js)

* [Source Code](https://github.com/fredericaltorres/fMicroService/tree/master/DonationMicroServices/Source/Donation.WebDashboard)


## Build and Deployment
The build and deployment processes consisting of
- Compiling the .NET Core projects
- Creating the docker images locally
- Pushing the docker images to an Azure Container Registry
- Deploying the different container image to an Azure Kubernetes cluster
are automated using PowerShell script and the Kubernetes command line tool KubeCtl.exe,
running on an Azure VM.

* [See powershell script in folder](https://github.com/fredericaltorres/fMicroService/tree/master/DonationMicroServices/Source)

### Kubernetes YAML files
A simple template engine written in PowerShell will execute the YAML template file located in folder

* [Kubernetes.Templates](https://github.com/fredericaltorres/fMicroService/tree/master/DonationMicroServices/Source/Kubernetes.Templates)