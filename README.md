# Awesome .NET Library - NuGet Package  

[![NuGet](https://img.shields.io/nuget/v/YourPackageName.svg)](https://www.nuget.org/packages/YourPackageName/)  
[![Build](https://github.com/your-repo/actions/workflows/build.yml/badge.svg)](https://github.com/your-repo/actions)  

## Overview  

This repository contains a collection of reusable .NET libraries. The goal is to provide modular, extensible packages to simplify development.  

## Features  

✔ **TN.EventBus**: Core event bus interface.  
✔ **TN.EventBus.RabbitMQ**: Implementation using RabbitMQ.  
✔ **TN.Example**: Sample usage of the event bus.  
✔ **TN.Azure.EventHub**: Implementation using Azure eventhub.  
✔ **TN.Azure.EventGrid**: Implementation using Azure event grid.  
✔ **TN.Azure.ServiceBus**: Implementation using Azure servicebus.  
✔ **TN.Azure.Storage**: Implementation using Azure storage.  
✔ **TN.Azure.KeyVault**: Implementation using Azure keyvault.  
✔ **TN.Azure.Cosmos**: Implementation using Azure cosmos database.  
✔ **TN.Excel**: Implementation importer/exporter excel.  
✔ **TN.Slack**: Implementation function send slack.  
✔ **TN.Memcache**: Implementation function send slack.  
✔ **TN.Redis**: Implementation function send slack.  

## Installation  

### NuGet Package Manager  
```powershell
Install-Package YourPackageName
```

### .NET CLI  
```sh
dotnet add package YourPackageName
```

## Usage  

### 1. Setup Event Bus  
```csharp
var eventBus = new RabbitMqEventBus(connectionString);
eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
```

### 2. Publish an Event  
```csharp
var orderCreatedEvent = new OrderCreatedEvent(orderId);
eventBus.Publish(orderCreatedEvent);
```

## Configuration  
Update `appsettings.json` if needed:  
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

## Roadmap  
- [ ] Add Kafka Event Bus  
- [ ] Add Azure Service Bus Integration  
- [ ] Improve Documentation  

## Contributing  
Contributions are welcome! Follow these steps:  
1. Fork the repository  
2. Create a new branch  
3. Make changes & commit  
4. Submit a pull request  
