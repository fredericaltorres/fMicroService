# Video Script

0. Clean Up

- Clean up Azure Fred Container Registry
- Initialization cleam the temp folder
- Docker images

```powershell
C:\tools\pdocker.ps1 -a delete-image -filter fredcontainerregistry.azurecr.io/donation.*
C:\tools\pdocker.ps1 -a delete-dangling-image

Remove-item "$($env:TEMP)\*.yaml"
Remove-item "$($env:TEMP)\*.json"
```

1. Let's create a Kubernetes cluster Create.Kubernetes.ps1, which will be
named fkubernetes6
```powershell
    cd "C:\DVT\microservices\fMicroService\DonationMicroServices\Source"
    .\Create.Kubernetes.ps1 -a create
```
* Let's look at the script and cluster configuration
```powershell
    az aks list -o table # Get the list of registered Kubernetets clusters    
    kubectl.exe get nodes # Get the list of VM aka nodes
```
* Let's start Azure Storage Explorer
    - Show queue
    - Show that there is no tables

While the cluster is creating, we can build the container images

2. The build process

```powershell
    cd "C:\dvt\microservices\fMicroService\DonationMicroServices\Source"
    .\DeploymentUtilityMaster.ps1 -a buildAndPush -app all
```

- Let's build the different projects
- Create the Docker container images
- Publish the container images into a Azure Container Registry named FredContainerRegistry

I have 3 C# projects
- Simulator, is .NET core console that can read a JSON file containing 50 000 JSON
donations and send them to an endpoint using an HTTP post. The console app execute 10
HTTP posts in paralele.

- RestApi.Entrance, is .NET core REST API that receive JSON donations
```json
{"Guid":"eacf779c-e42d-47f3-8265-92d3c114feed","FirstName":"Rooney","LastName":"Shall","Email":"rshall0@fema.gov","Gender":"Male","Phone":"271-648-3024","IpAddress":"147.110.186.181","Country":"China","Amount":"$44.38","CC_Number":"6767595943679547","CC_ExpMonth":12,"CC_ExpYear":2016,"CC_SecCode":472},
```
with the following url 'http://HOST/api/Donation'.
The endpoint first validate the data and store the updated JSON into a Azure queue.

- The Queue.Processor is a .NET core console that read donations from the Azure queue,
validate the data, compute in memory amount aggregation per country/amount and then store the data into an Azure Table named 'DonationTable'.

`Notifications`, All applications
- Simulator
- RestApi.Entrance
- The Queue.Processor
after a batch of 500 donations sent, received or processed, a notifycation message is published to an Azure Service Bus (Publish/Subscribe).
* The total number of donation sent, received or processed 
* The number of donation sent, received or processed per second
* The app also notify the details of the country/amount aggregation for the last 500 donations.

All this notification feed data for the real time web dashboard.
The details of the country/amount aggregation is stored in an Azure table named 'DonationAggregate'

3. Let's deploy

- Web Dashboard, the web Dashboard allow me to monitor the system in pseudo real time,
based on Azure Service Bus (Publish/Subscribe). The source code is located in the project
fAzureHelper sub folders SystemActicity and ServiceBus.

```powershell
    cd C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.WebDashboard
     .\Scripts\publishToAzureAppService.ps1 -a publish -Password XXXXXXXX
```

- RestApi.Entrance, The endpoint must be deployed first because we need to know
the end point ip before we can send the data. I going to deploy 3 instances of
container image and one load balancer usin Kubernetes deployment and service concepts.

```powershell
    cd "C:\DVT\microservices\fMicroService\DonationMicroServices\Source"
    .\DeploymentUtilityMaster.ps1 -a deploy -app Donation.RestApi.Entrance
    code "C:\Users\fredericaltorres\AppData\Local\Temp\donation-restapi-entrance--Deployment.{Params}.yaml"
    code "C:\Users\fredericaltorres\AppData\Local\Temp\donation-restapi-entrance--Service.{Params}.yaml"

    # http://52.179.169.37:80/api/info/getinfo
    kubectl get deployment
    kubectl get service
    kubectl get pods
    # Show web dashboard
```

- The Queue.Processor, let's deploy the queue processor, I am goind to deploy
2 instance of container image and using Kubernetes Statefullsets concepts.
The concept of Statefullsets is need to have each container instance machine name or pod name to end with index like 'xxxx-0', 'xxx-1'.

```powershell
    cd "C:\DVT\microservices\fMicroService\DonationMicroServices\Source"
    .\DeploymentUtilityMaster.ps1 -a deploy -app Donation.QueueProcessor.Console
    code "C:\Users\fredericaltorres\AppData\Local\Temp\donation-queueprocessor-console--Deployment.{Params}.yaml"

    kubectl get deployment
    kubectl get service
    kubectl get pods
    # Show web dashboard
```

- The Person.Simulator, let's deploy the Person Simulator, I am goind to deploy
X instances of container image and using Kubernetes Statefullsets concepts.
The concept of Statefullsets is need to have each container instance machine name or pod name to end with index like 'xxxx-0', 'xxx-1'. The index is used the load and send a specific JSON file. Each file contains 50 000 donation and I have up to 10 files.

```powershell
    cd "C:\DVT\microservices\fMicroService\DonationMicroServices\Source"
    .\DeploymentUtilityMaster.ps1 -a deploy -app Donation.PersonSimulator.Console
    code "C:\Users\fredericaltorres\AppData\Local\Temp\donation-personsimulator-console--Deployment.{Params}.yaml"

    kubectl get deployment
    kubectl get service
    kubectl get pods
```

4. Let's analyse the system while receiving donation
- Using the Web Dashboard

5. Let's clean up
```powershell

   .\DeploymentUtilityMaster.ps1 -a deleteDeployment -app all
   .\DeploymentUtilityMaster.ps1 -a initData

    kubectl get deployment
    kubectl get service
    kubectl get pods
    # Show web dashboard
```
