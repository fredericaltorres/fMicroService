az aks list -o table # Get the list of clusters
az aks get-credentials --resource-group fkubernetes9 --name fkubernetes9  --overwrite-existing # Switch to 
kubectl config use-context fkubernetes9 # Switch to cluster

# Define the Azure Container Registry as a docker secret
$password = "izBEjxfFrepl+uW5uI3YWKOdO73bk0Pm"
kubectl create secret docker-registry fredcontainerregistry --docker-server fredcontainerregistry.azurecr.io --docker-email fredericaltorres@gmail.com --docker-username=FredContainerRegistry --docker-password $password
kubectl get secret

# Blue / Green Deployment
https://www.ianlewis.org/en/bluegreen-deployments-kubernetes
https://github.com/ivans3/kubernetes-kops-blue-green-deploy

#####################################################################
# Deployment 
# Deployment file Kubernetes\Deployment.Create.v1.0.2.yaml
# Deployment name fdotnetcorewebapp-deployment
kubectl create -f Kubernetes\Deployment.Create.v1.0.2.yaml --record
cls
$json = kubectl get pod -o json
$jsonContent = $json | ConvertFrom-Json
foreach($item in $jsonContent.items) {
	write-host ""
	$podName = $item.metadata.name
	write-host "Pod Name:$podName" -ForegroundColor Cyan
	kubectl logs $podName
}

kubectl get deployment fdotnetcorewebapp-deployment-1.0.2
kubectl describe deployment fdotnetcorewebapp-deployment-1.0.2
kubectl get ReplicaSet # Get Replica Set
kubectl describe ReplicaSet # Get Replica Set
kubectl delete deployment fdotnetcorewebapp-deployment-1.0.2

#####################################################################
# Service
# Service name fdotnetcorewebapp-service
kubectl create -f Kubernetes\Service.v1.0.2.yaml

while($true) {
	$json = kubectl get svc fdotnetcorewebapp-service -o json
	$jsonContent = $json | ConvertFrom-Json
	$serviceName = $jsonContent.metadata.name
	if($jsonContent.status.loadBalancer.ingress -eq $null) {
		write-host "Public IP not ready yet for service $serviceName"
		Start-Sleep -s 2
	}
	else {
		$servicePublicIp = $jsonContent.status.loadBalancer.ingress[0].ip
		write-host "Public IP:$servicePublicIp ready for service $serviceName"
		break
	}
}

External Ip : 104.43.247.128
http://104.43.247.128:80

kubectl get svc fdotnetcorewebapp-service
kubectl describe svc fdotnetcorewebapp-service  
kubectl apply -f helloKubernetes\Service.yaml
kubectl delete svc fdotnetcorewebapp-service

#####################################################################
# Deployment 
# Deployment file Kubernetes\Deployment.Create.v1.0.3.yaml
# Deployment name fdotnetcorewebapp-deployment-1.0.3
kubectl create -f Kubernetes\Deployment.Create.v1.0.3.yaml --record

kubectl get deployment fdotnetcorewebapp-deployment-1.0.3
kubectl describe deployment fdotnetcorewebapp-deployment-1.0.3
kubectl get ReplicaSet # Get Replica Set
kubectl describe ReplicaSet # Get Replica Set
kubectl delete deployment fdotnetcorewebapp-deployment # If done we could delete the deployment


#####################################################################
# Service
# Service name fdotnetcorewebapp-service
kubectl apply -f Kubernetes\Service.v1.0.3.yaml # update service to point to the v 1.0.3 pod

kubectl get svc fdotnetcorewebapp-service
kubectl describe svc fdotnetcorewebapp-service  
kubectl apply -f helloKubernetes\Service.yaml
kubectl delete svc fdotnetcorewebapp-service