# Back end Architecture

Request -> HTTP API `gateway` -> Service Bus ->

    add_activity -> `Activities Service` -> activity_added

Identity Service
Activities Service 
    add_activity command consumed by Activities Service which subscribe to message
 
 activity_edit


Projection Architecture

- Each service has its own Mongo database.

Actio.Api - MongoDB `CRQS`
Actio.Commom
    Messages
Actio.Identity - MongoDB
Actio.Services.Activities - MongoDB
Actio.Tests
Actio.Tests.EndToEnd

## Store
- Mongo DB
