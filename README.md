# 🧩 .NET Saga Orchestration Example with RabbitMQ & MassTransit

This project demonstrates how to implement **Saga Orchestration Pattern** in a **.NET Microservices** architecture using **RabbitMQ** and **MassTransit**.

## 🚀 Technologies Used
- .NET 8 Web API
- RabbitMQ
- MassTransit
- Entity Framework Core
- MongoDB / SQL Server

## 🧠 Architecture
The system simulates an order workflow with:
- Order Service
- Payment Service
- Stock Service
- Coordinator (Saga Orchestrator)

Each service communicates asynchronously through RabbitMQ using **MassTransit**.  
Saga Orchestration ensures **distributed transaction consistency** between microservices.

## ⚙️ How to Run
```bash
docker-compose up --build
