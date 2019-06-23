# Azure Kubernetes Service

## Overview
The document describes how to start with Azure Kubernetes Service.

### Videos
* [Azure: "Kubernetes the Easy Way" Managed Kubernetes on Azure AKS | E101](https://www.youtube.com/watch?v=MCRJSKzdDjI)
* [Kubectl Cheat Sheet](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)

### Setup
* Install the Azure Kubernetes Service (aks) for the az command line
```
c:\>az aks install-cli
```

### Configuring powershell console for Azure
```powershell
az login
az account list --refresh --output table
az account set -s <YOUR-CHOSEN-SUBSCRIPTION-NAME>
```

### Creation of the Kubernetes cluster
* A cluster can be created from the portal or the command line
* A cluster cost money, becare full to delete it or shutdown the VM (AKA Pods)
[Kubernetes walkthrough](https://docs.microsoft.com/en-us/azure/aks/kubernetes-walkthrough)
```powershell
az group create -n fkubernetes6 -l eastus2 # Create a resource group fkubernetes6

# Create the cluster
# -c 2 - 2 nodes   -k Kubernete version
az aks create help # return all parameters

# Vm Size https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-sizes-specs
#$vmSize = "Standard_D2s_v2" # 2 cpu, 7 Gb Ram
#$vmSize = "Standard_D4s_v3" # 4 cpu, 17 Gb Ram
$vmSize = "Standard_D1_v2" # 1 cpu, 3.5 Gb Ram
$vmCount = 2
az aks create --name fkubernetes6 --resource-group fkubernetes6 --kubernetes-version 1.12.8 --enable-addons monitoring  --generate-ssh-keys --enable-rbac --node-count $vmCount --node-vm-size $vmSize 


# More on how to create a AKS -> https://msdn.microsoft.com/en-us/magazine/mt846465.aspx?f=255&MSPPError=-2147217396

# See https://docs.microsoft.com/en-us/cli/azure/azure-cli-configuration?view=azure-cli-latest
# to remove the yes/no confirmation
az aks delete -n fkubernetes6 -g fkubernetes6 # How to delete a cluster
az group delete -n fkubernetes6 # Delete the resource group - always delete the Kubernetes service 

```
* A resource group named MC_fkubernetes6_fkubernetes6_eastus2 will be created containing all resources (vm, disk, load balancer).

### Learning more about your AKS (Azure Kubernetes Service)
```powershell
az aks list -o table # Get the list of clusters
```

### Security and access to the cluster
* Probably
  - To apply a fine level of security access to the cluster AKS require a Azure AD.
  - The default mode is the owner of the subscription can do every thing.
```powershell
az aks get-credentials --resource-group fkubernetes6 --name fkubernetes6 # Switch to cluster
```

### Kuster info
```powershell
c:\> kubectl cluster-info
c:\> kubectl config view
```

#### Setup kubectl.exe

The Kubernetes command-line tool, kubectl, allows you to run commands against Kubernetes clusters. 
- [Install and setup kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)

```powershell
kubectl get nodes # Get the list all nodes or vm
kubectl get pods --all-namespaces # List pods running in the cluster
kubectl version # Get version of client and server Kubernetes
```

### Kubernetes Web Based Dashboard

* Open dashbord to anyone, see doc below for more security, execute command below, this is new not well documented
* [Access the Kubernetes web dashboard in Azure Kubernetes Service (AKS)](https://docs.microsoft.com/en-us/azure/aks/kubernetes-dashboard)
 
```powershell
# Authorize anybody to be admin on the cluster dashboard
C:\> kubectl create clusterrolebinding kubernetes-dashboard --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard
az aks browse --resource-group fkubernetes6 --name fkubernetes6 # Start web server dashboard and open in browser
az aks browse -n fkubernetes6 -g fkubernetes6  # open dashboard # Start web server dashboard and open in browser
```
### Adding more node (VM) to the cluster
```powershell
# The cluster was created with 2 agents or node or vm, we now set the number to 3
# The default vm configuration is used
az aks scale --resource-group fkubernetes6 -n fkubernetes6 --agent-count 2
```

### Kubernetes Version

```powershell
# List of version of kubernetes available and the upgrade path
az aks get-versions --location eastus2 -o table
kubectl version # Get version of client and server Kubernetes - server 1.12.8 client 1.20.11
```

### Switch to a specific cluster
```powershell
C:\> kubectl config use-context fkubernetes6 # Switch to cluster
C:\> kubectl get services
```

## Kubernetes the POD concept

A Kubernetes pod is a group of containers that are deployed together on the same host. 
(Generally 1). If you frequently deploy single containers, you can generally replace the word "pod" with "container" and accurately understand the concept. [Overview of a Pod](https://coreos.com/kubernetes/docs/latest/pods.html)

How to instanciate a container image from a (Azure Container) Registry into a Kubernetes cluster?

* [How to use a private Azure Container Registry with Kubernetes](https://thorsten-hans.com/how-to-use-private-azure-container-registry-with-kubernetes)

1. Register the Azure Container Registry into the Kubernetes cluster
```powershell
# Define the Azure Container Registry as a docker secret
C:\> kubectl create secret docker-registry fredcontainerregistry --docker-server fredcontainerregistry.azurecr.io --docker-email fredericaltorres@gmail.com --docker-username=FredContainerRegistry --docker-password "$($env:azureContainerRegistryPassword)"
```

1. Create a pod-sample.yaml file

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: fcoreconsoleazurestorage # name to reference the instance
spec:
  containers:
  - name: container-fcoreconsoleazurestorage
    image: fredcontainerregistry.azurecr.io/fcoreconsoleazurestorage:1.0.31
  imagePullSecrets:
  - name: fredcontainerregistry
```

1. Execute the command to instanciate the container image into a pod
```powershell

C:\> kubectl create -f pod-sample.1.yaml # instanciate the container image into a pod
C:\> kubectl create -f pod-sample.2.yaml # instanciate a second instance of the container image into a pod

C:\> kubectl describe pod fcoreconsoleazurestorage1
C:\> kubectl describe pod fcoreconsoleazurestorage2

C:\> kubectl get deployments # get the information about the deployment

C:\> kubectl get pods # Get all the pod
C:\> kubectl get pods --field-selector=status.phase=Running # Get all running pods or container in the namespace

C:\> kubectl logs fcoreconsoleazurestorage1 # Get Log
C:\> kubectl logs fcoreconsoleazurestorage2 # Get Log

C:\> kubectl logs -f fcoreconsoleazurestorage # Stream log
C:\> kubectl exec fcoreconsoleazurestorage -- ls /app/tutu # Run command in pod

C:\> kubectl delete -f pod-sample.1.yaml # Delete running pod or container using the yaml file
C:\> kubectl delete -f pod-sample.2.yaml # Delete running pod or container

C:\> kubectl delete pod fcoreconsoleazurestorage1 # Delete running pod or container using the name
C:\> kubectl delete pod fcoreconsoleazurestorage2 # Delete running pod or container using the name
```

## Kubernetes the Replication Controller concept

before we start with the replication controller, you can test the image
'nigelpoulton/pluralsight-docker-ci' like this
```powershell
docker pull nigelpoulton/pluralsight-docker-ci:latest
docker run -p 8080:8080 nigelpoulton/pluralsight-docker-ci:latest
# http://localhost:8080
```

```yaml
apiVersion: v1
kind: ReplicationController
metadata:
  name: hello-rc
spec:
  replicas: 3
  selector:
    app: hello-world
  template:
    metadata:
      labels:
        app: hello-world
    spec:
      containers:
      - name: hello-pod
        image: nigelpoulton/pluralsight-docker-ci:latest
        ports:
        - containerPort: 8080
```

```powershell
C:\> kubectl create -f .\hello-ReplicationController.yaml
C:\> kubectl get rc -o wide # Get the replication controller instanciated
C:\> kubectl get pods # Get the pods here we have 3 pods
```
#### Updating the state
Increase `replicas:` to 5 and run 
```powershell
C:\> kubectl apply -f .\hello-ReplicationController.yaml
C:\> kubectl get rc -o wide # Get the replication controller instanciated
C:\> kubectl get pods # Get the pods here we have 5 pods
```

### Kubernetes the Service concept

- Service is a REST object in the K8s API
- Abstraction/proxy/Load balancer that allows to access pods from inside and outside cluster

The Service and the pods use the same port number
```
 Service (ip, dns, port)
     /      |     \
    pod1   pod2  pod3
```
- An Endpoint object associated with the Service maintain the list of active pods.
- Services and pods are associated via labels.
- Service Discovery
  * DNS based
  * Environment variables
- Thanks to the association of one Services to PODs via label, we can switch
from one version to another, by changing a label in the services.

Service file configuration: hello-svc.yaml
```yaml
apiVersion: v1
kind: Service
metadata:  
  name: hello-svc
  annotations:
    # service.beta.kubernetes.io/azure-load-balancer-internal: "true"
    # service.beta.kubernetes.io/azure-load-balancer-mode: "aa"
  labels:
    app: hello-world
spec:
  # ClusterIP: Stable internal cluster IP (Only inside the cluster)
  # NodePort: Exposes the app outside of the cluster by adding a cluster wide port on top of the ClusterIP, other
  # LoadBalancer: Integration NodePort with a cloud-based load balancers
  type: LoadBalancer # https://docs.microsoft.com/en-us/azure/aks/concepts-network - https://docs.microsoft.com/en-us/azure/aks/internal-lb
  ports:
  - port: 8080 # Port exposed in the container is mapped to the nodePort
    # nodePort: 30001
    protocol: TCP # can use UDP
  selector:
    app: hello-world # Must match the pod in the replication controller file
                     # Execute `kubectl describe pods` and check Labels field
```

```powershell
kubectl create -f hello-svc.yaml
kubectl apply -f hello-svc.yaml
kubectl get svc hello-svc
kubectl describe svc hello-svc
#NAME        TYPE           CLUSTER-IP   EXTERNAL-IP   PORT(S)          AGE
#hello-svc   LoadBalancer   10.0.57.75   <pending>     8080:30001/TCP   1m
#hello-svc   LoadBalancer   10.0.57.75   40.70.217.83   8080:30001/TCP   2m

kubectl describe svc hello-svc
# LoadBalancer Ingress:     10.240.0.6

kubectl delete svc hello-svc
```
Use as ip the field 'LoadBalancer Ingress'.
http://104.211.49.78:8080

#### Getting endpoint informaton

```powershell
C:\> kubectl get ep
C:\> kubectl describe ep hello-svc
```

## Kubernetes the Deployment concept

- Updates & rollbacks
- Deployment wrap around Replicat Controller AKA Replica Set
- Let's delete the previous Replication Controller
```powershell
C:\> kubectl delete rc hello-rc
```

### Creation

Deployment file configuration: hello-Deployment.Creation.yaml
```yaml
apiVersion: extensions/v1beta1
kind: Deployment
metadata:
  name: hello-deploy
spec:
  replicas: 3
  template:
    metadata:
      labels:
        app: hello-world
    spec:
      containers:
      - name: hello-pod
        image: nigelpoulton/pluralsight-docker-ci:latest
        ports:
        - containerPort: 8080
```

```powershell
kubectl create -f hello-Deployment.Creation.yaml  --record
kubectl get deployment hello-deploy
kubectl describe deployment hello-deploy
kubectl get ReplicaSet # Get Replica Set
kubectl describe ReplicaSet # Get Replica Set
kubectl delete deployment hello-deploy # If done we could delete the deployment
```
 
Deployment file configuration: hello-Deployment.Update.yaml
```yaml
apiVersion: extensions/v1beta1
kind: Deployment
metadata:
  name: hello-deploy
spec:
  replicas: 3
  minReadySeconds: 10
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1
      maxSurge: 1
  template:
    metadata:
      labels:
        app: hello-world
    spec:
      containers:
      - name: hello-pod
        image: nigelpoulton/pluralsight-docker-ci:latest
        ports:
        - containerPort: 8080
```


```powershell
# Change image to `nigelpoulton/pluralsight-docker-ci:edge`
kubectl apply -f hello-Deployment.Update.yaml --record
kubectl rollout status deployment hello-deploy # Wait for the rollout to finish
kubectl get deployment hello-deploy 
kubectl describe deployment hello-deploy 
kubectl rollout history deployment hello-deploy # Get deployment history
kubectl get ReplicaSet # Get Replica Set - Now we have 2 replica for 2 deployments 
```

Now undo the previous update

```powershell
kubectl rollout history deployment hello-deploy # Get deployment history
kubectl rollout undo deployment hello-deploy --to-revision=1 # Undo deployment 2 by restoring to deployment 1
kubectl rollout status deployment hello-deploy # Wait for the rollout to finish
kubectl get deploy hello-deploy
```

# https://devopscube.com/kubernetes-deployment-tutorial/
kubectl create -f pod-deployment.yaml

## Kubernetes.io tutorials
- [Interactive Tutorial - Deploying an App](https://kubernetes.io/docs/tutorials/kubernetes-basics/deploy-app/deploy-interactive/)
```bash

kubectl run kubernetes-bootcamp --image=gcr.io/google-samples/kubernetes-bootcamp:v1 --port=8080
# kubectl run --generator=deployment/apps.v1 is DEPRECATED and will be removed in a future version. Use 
# kubectl run --generator=run-pod/v1 or kubectl create instead.
kubectl proxy
curl http://localhost:8001/version


kubectl proxy # rer
```