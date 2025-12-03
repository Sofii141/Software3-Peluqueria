# ✂️ Backend Monolítico - Sistema de Gestión de Peluquería

Bienvenido al repositorio del **Backend Monolítico** del sistema de peluquería. Este proyecto gestiona el núcleo del negocio: usuarios, autenticación, gestión de estilistas, catálogo de servicios y configuración de agendas.

Está construido siguiendo los principios de **Clean Architecture** (.NET 8) y se comunica con un microservicio de reservas mediante **RabbitMQ** y **HTTP**.

---

## 📋 Tabla de Contenidos
1. [Requisitos Previos](#-requisitos-previos)
2. [Configuración del Entorno](#-configuración-del-entorno)
3. [Instalación de Base de Datos](#-instalación-de-base-de-datos)
4. [Configuración de RabbitMQ](#-configuración-de-rabbitmq)
5. [Ejecución del Proyecto](#-ejecución-del-proyecto)
6. [🧪 Pruebas con Postman (Oficial)](#-pruebas-con-postman-oficial)
7. [Documentación API (Swagger)](#-documentación-api-swagger)
8. [Arquitectura](#-arquitectura)

---

## 🛠 Requisitos Previos

Asegúrate de tener instalado lo siguiente:

*   **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
*   **[SQL Server](https://www.microsoft.com/es-es/sql-server/sql-server-downloads)** (Express o Developer)
*   **[RabbitMQ](https://www.rabbitmq.com/download.html)** (Recomendado usar Docker)
*   **[Postman](https://www.postman.com/downloads/)** (Para ejecutar la colección de pruebas)
*   **Visual Studio 2022** o VS Code.

---

## ⚙️ Configuración del Entorno

1.  **Clonar el repositorio:**
    ```bash
    git clone https://github.com/tu-usuario/tu-repo.git
    cd tu-repo
    ```

2.  **Configurar `appsettings.json`:**
    Ubica el archivo en `Peluqueria.API`. Si no existe, crea uno con este contenido (ajusta la cadena de conexión a tu servidor local):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PeluqueriaDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JWT": {
    "Issuer": "http://localhost:5167",
    "Audience": "http://localhost:5167",
    "SigningKey": "TU_CLAVE_SUPER_SECRETA_DEBE_SER_LARGA_PARA_HMAC_SHA512"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

---

## 🗄 Instalación de Base de Datos

El sistema usa **Code-First**. No crees la BD manualmente.

1.  Abre la terminal en la carpeta del proyecto API.
2.  Ejecuta los comandos de Entity Framework:

```bash
# Restaurar paquetes
dotnet restore

# Aplicar migraciones y ejecutar SEEDS (Datos de prueba automáticos)
dotnet ef database update
```

> **Nota:** Esto creará automáticamente al usuario Admin (`admin`), un Estilista (`laura.e`) y servicios base.

---

## 🐇 Configuración de RabbitMQ

El sistema necesita RabbitMQ para enviar eventos al microservicio. Ejecuta este comando en Docker:

```bash
docker run -d --hostname my-rabbit --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
*   **Dashboard:** http://localhost:15672 (User: `guest`, Pass: `guest`)

---

## ▶️ Ejecución del Proyecto

### Desde Visual Studio
1.  Establece **Peluqueria.API** como proyecto de inicio.
2.  Presiona `F5`.

### Desde Terminal
```bash
cd Peluqueria.API
dotnet run
```
La API iniciará (usualmente) en: `https://localhost:7274`.

---

## 🧪 Pruebas con Postman (Oficial)

En la raíz de este repositorio encontrarás el archivo:
📄 `Monolito-Peluqueria.postman_collection.json`

### Pasos para configurar Postman:

1.  **Importar:** Abre Postman -> Botón "Import" -> Arrastra el archivo `.json`.
2.  **Crear Entorno (Environment):**
    *   Ve a la pestaña "Environments" en la barra lateral izquierda.
    *   Crea uno nuevo llamado `PeluqueriaLocal`.
    *   Agrega las siguientes variables:

| Variable | Initial Value | Current Value | Descripción |
| :--- | :--- | :--- | :--- |
| `baseUrl` | `https://localhost:7274` | `https://localhost:7274` | Puerto donde corre tu API |
| `adminToken` | (dejar vacío) | (dejar vacío) | Aquí pegaremos el token |

3.  **Seleccionar Entorno:** En la esquina superior derecha de Postman, selecciona `PeluqueriaLocal` en el dropdown.

### Flujo de Prueba:

1.  **Login Admin:**
    *   Ve a la carpeta de la colección y ejecuta la petición **"Login de Administrador"**.
    *   Si es exitoso (200 OK), copia el valor de `token` de la respuesta.
2.  **Configurar Token:**
    *   Ve a tu Entorno (`PeluqueriaLocal`) y pega el token en el valor de la variable `adminToken`.
    *   Guarda los cambios (Save).
3.  **Ejecutar Resto de Peticiones:**
    *   Ahora puedes ejecutar peticiones protegidas como **"Crear Servicio"**, **"Crear Nuevo Estilista"** o **"Actualizar Horario"**.
    *   La colección ya está configurada para leer automáticamente el token de la variable `{{adminToken}}`.

> **Nota:** Las peticiones que suben imágenes (Endpoints `POST` o `PUT` con form-data) pueden requerir que vuelvas a seleccionar el archivo de imagen en la pestaña "Body" de Postman, ya que las rutas de archivos locales no se exportan por seguridad.

---

## 📖 Documentación API (Swagger)

Para ver la documentación interactiva de los esquemas y modelos:
👉 **URL:** `https://localhost:7274/swagger/index.html`

Aquí podrás ver qué campos son obligatorios y los códigos de error documentados (400, 404, 409) gracias a los comentarios XML del código.

---

## 🏗 Arquitectura

El proyecto sigue una arquitectura limpia dividida en capas:

*   **Domain:** Entidades (`Estilista`, `Servicio`, `Agenda`) y Excepciones.
*   **Application:** Interfaces, DTOs, Servicios (`AccountService`, `EstilistaAgendaService`) y Validaciones.
*   **Infrastructure:** EF Core, Identity, RabbitMQ Publisher y Cliente HTTP (Fail-Safe).
*   **API:** Controladores y Middlewares.

### Integraciones
*   **RabbitMQ:** Publica eventos (`estilista.creado`, `horario.actualizado`) al Exchange `agenda_exchange`.
*   **Microservicio Reservas:** Se consulta vía HTTP para validar integridad referencial antes de borrar datos.

---

Hecho con ❤️ en .NET 8
