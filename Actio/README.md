
# About RabbitMQ

```bash
# Run RabbitMQ as a container locally
C:\>docker run -p 5672:5762 rabbitmq
```

## Starting all services locally

```bash
cd "C:\DVT\microservices\fMicroService\Actio\src\Actio.Services.Activities"
# Now listening on: http://[::]:5050
dotnet run --urls "http://*:5050"

# listening on: https://localhost:5001
# listening on: https://localhost:5001/activities
# listening on: http://localhost:5000
cd "C:\DVT\microservices\fMicroService\Actio\src\Actio.Api"
dotnet run
```

## Testing URL
curl -X GET https://localhost:5001
curl -H "Content-Type: application/json; charset=utf-8" -X POST -d "{""Category"":""bar-category"",""Name"":""foo-name""}" https://localhost:5001/activities

curl -X GET https://localhost:44354/activities
curl -H "Content-Type: application/json; charset=utf-8" -X POST -d "{""Category"":""bar-category"",""Name"":""foo-name""}" https://localhost:44354/activities


## Udemy Course
- https://piotrgankiewicz.com/contact/
- https://piotrgankiewicz.com/2016/10/31/net-core-rabbitmq-rawrabbit/