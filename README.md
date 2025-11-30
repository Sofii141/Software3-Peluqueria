# Software3-Barberia
Proyecto barberia utilizando de backend ASP.NET CORE y Angular

## Backend 

Add-Migration InitialCreate -Project Peluqueria.Infrastructure -StartupProject Peluqueria.API    

Update-Database -Project Peluqueria.Infrastructure -StartupProject Peluqueria.API

## Microservicio papitas actual

dotnet ef migrations add InitialMigration --context ReservacionesDbContext

dotnet ef database update --context ReservacionesDbContext

## En el CMD

dotnet dev-certs https --trust

## Admin credenciales 

{
  "username": "admin",
  "password": "password123"
} 

## Onion 

El principio de la Arquitectura Cebolla establece que las capas internas no pueden depender de las capas externas. La construcción de la URL absoluta es una preocupación de Presentación/UI (cómo el recurso se mostrará al cliente), no una preocupación del Dominio o la Aplicación.

<img width="450" height="450" alt="image" src="https://github.com/user-attachments/assets/cdb1b73b-0c6e-43cf-9c52-283bc6e297e5" />






