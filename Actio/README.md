
# About RabbitMQ

```bash
# Run RabbitMQ as a container locally
C:\>docker run -p 5672:5762 rabbitmq
```

## Starting all services locally

```bash
# listening on: https://localhost:5001
# listening on: http://localhost:5000
cd "C:\DVT\microservices\fMicroService\Actio\src\Actio.Api"
dotnet run

cd "C:\DVT\microservices\fMicroService\Actio\src\Actio.Services.Activities"
# Now listening on: http://[::]:5050
dotnet run --urls "http://*:5050"
```

## Testing URL
http://localhost:5000/activities					  
post
Header
	Content-Type: application/json
Body
	{ "category": "bar-category", "name": "foo-name"}

Url:
curl -X GET https://localhost:5001/activities
curl -H "Content-Type: application/json" -X POST -d '{"category":"bar-category","name":"foo-name"}' https://localhost:5001/activities
curl --header "Content-Type: application/json" --request POST -d '{}' https://localhost:50001/activities?name=popo

https://localhost:44354/api/values
curl -X GET https://localhost:44354/activities
curl -H "Content-Type: application/json; charset=utf-8" -X POST -d '{"Category":"bar-category","Name":"foo-name"}' https://localhost:44354/activities
curl --header "Content-Type: application/json" --request POST --data '{"Category":"bar-category","Name":"foo-name"}' https://localhost:44354/activities
curl --header "Content-Type: application/json" --request POST --data '"Category":"bar-category","Name":"foo-name"' https://localhost:44354/activities

curl --header "Content-Type: application/json" --request POST -d '{"Category":"bar-category"}' https://localhost:44354/activities?name=popo


curl --header "Content-Type: application/json" --request POST --data '{}' https://localhost:44354/api/values?value=tutu
curl --header "Content-Type: application/json" --request POST --data '{"Name":"fred", "Category":"Cat"}' https://localhost:44354/api/values
curl --header "Content-Type: application/json" --request POST --data '{"Name":"foo","Category":"Cat"}' https://localhost:44354/api/values




