# fMicroService
In this repo I am experimenting building microservices with 
- .NET Core
- Docker
- Kubernetes on Azure

My goal is to build a case study that I can use to evaluate the scalability possibilities of the technologies mentioned above.

## On line donation back end prototype
I am going to build a back end able to received and processed donations.
The donation should be created by hundreds of users entering donation amounts
and credit card information on a web site and press send.

### The Person Simulator
I will not build the front end, but rather create a .NET Core console application
that can be instantiated up to 10 times in Docker containers instance in an Azure Kubernetes cluster.
Each instance will read a specific local donation[X].json file containing 50 000
donations and execute an HTTP post to a specific end point for each donations.
10 HTTP post are executed in parallel. 
Every 500 donations sent, the application send to an Azure Service Bus channel (Publisher/Subscribers) some performance information.

### Rest Api
A .NET Core REST API will implement the HTTP post to received the donations.
Multiple instances of the API process will be executed in a Docker containers
behind a load balancer provisioned using am Azure Kubernetes cluster.
When a donation is received, it is 
- Validated
- Push to a queue
- Every 500 donations received, the endpoint send to an Azure Service Bus channel (Publisher/Subscribers) some performance information.

### Queue Processor
A .NET Core console application that can be instantiated multiple times as Docker containers instances in an Azure Kubernetes cluster will
- Pop messages from the queue
- Validate the data
- Store the data in an Azure Table
- Compute an aggregate of the amount received per country 
- Every 500 donations processed, the application send to an Azure Service Bus channel (Publisher/Subscribers) the aggregated information and other performance information.

### Web Dashboard
A ASP.NET Core Web Application implementing
- An internal endpoint named SystemActivitiesController will
    * Received the information sent by the the different processes to the Azure Service Bus channel
    * Store and aggregate the data in static dictionaries
    * Communicate the information the Dashboard browser side 

    * [SystemActivitiesController source code](https://github.com/fredericaltorres/fMicroService/blob/master/DonationMicroServices/Source/Donation.WebDashboard/Controllers/SystemActivitiesController.cs)

- A Web Dashboard written as a Single Page Application (SPA) in React that display the performance informations sent by the different processes and the donation amount per country in charts and tables in `pseudo real time`.

    * [Web Dashboard React Code](https://github.com/fredericaltorres/fMicroService/blob/master/DonationMicroServices/Source/Donation.WebDashboard/ClientApp/src/components/Home.js)


## Donation MicroServices
This folder contains the source code of a back end and client simulator to
1. Send 500 000 donation transactions
1. Process the transactions

in progress
