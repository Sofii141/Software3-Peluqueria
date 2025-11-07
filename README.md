# Proyecto Peluqueria - Kaisoju - Backend API

Instrucciones para la configuración y ejecución del backend del proyecto en un entorno local.

## 1. Prerrequisitos

Asegúrate de tener instalado el siguiente software:

*   **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** o una versión compatible.
*   **[SQL Server Express](https://www.microsoft.com/es-es/sql-server/sql-server-downloads)** o cualquier otra edición de SQL Server.
*   **[Postman](https://www.postman.com/downloads/)** para probar los endpoints de la API.

## 2. Configuración de la Base de Datos

La base de datos se creará y se llenará con datos iniciales (seeding) utilizando Entity Framework Core.

1.  **Revisar la Cadena de Conexión**:
    *   Abre el archivo `appsettings.json`.
    *   Verifica que la cadena de conexión en `ConnectionStrings.DefaultConnection` apunte a tu instancia de SQL Server. La configuración por defecto es:
        ```json
        "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PeluqueriaDB1;Trusted_Connection=True;TrustServerCertificate=True;"
        ```
    *   Si tu servidor SQL tiene un nombre diferente a `localhost\SQLEXPRESS`, ajústalo.

2.  **Aplicar Migraciones**:
    *   Abre una terminal o consola en la raíz del proyecto de backend (donde se encuentra el archivo `.csproj`).
    *   Ejecuta el siguiente comando. Esto creará la base de datos `PeluqueriaDB1`, aplicará las tablas y registrará los datos iniciales (roles, usuario admin, categorías y servicios).
        ```bash
        dotnet ef database update
        ```

## 3. Ejecución del Backend

Una vez configurada la base de datos, puedes iniciar el servidor de la API.

1.  **Desde la terminal**:
    *   En la misma terminal, en la raíz del proyecto backend, ejecuta:
        ```bash
        dotnet run
        ```
2.  **Verificar que está en ejecución**:
    *   El servidor se iniciará y estará escuchando en la URL configurada en `Properties/launchSettings.json`. Por defecto, debería ser `https://localhost:7274` y `http://localhost:5034`.

## 4. Probar la API con Postman

Se proporciona una colección de Postman (`CRUD_Postman_Collection.json`) en esta misma carpeta para probar todos los endpoints.

1.  **Importar la Colección**:
    *   Abre Postman.
    *   Haz clic en `Import` y selecciona el archivo `CRUD_Postman_Collection.json`.

2.  **Probar Endpoints Públicos**:
    *   Puedes probar directamente las peticiones `GET` que no requieren autenticación, como:
        *   `GET /api/categorias`
        *   `GET /api/servicios`

3.  **Probar Endpoints Protegidos (Admin)**:
    *   Las operaciones de Crear (POST), Actualizar (PUT) y Eliminar (DELETE) requieren un token de administrador.
    *   **Paso A: Obtener el Token**:
        *   En Postman, busca la petición `Login Admin` en la colección.
        *   Envía la petición. El usuario administrador ya fue creado en la base de datos con estas credenciales:
            *   **Username**: `admin`
            *   **Password**: `password123`
        *   En la respuesta, copia el valor del `token`.
    *   **Paso B: Usar el Token**:
        *   Selecciona una petición protegida (ej. `DELETE Servicio`).
        *   Ve a la pestaña **Authorization**.
        *   Selecciona `Type`: **Bearer Token**.
        *   Pega el token que copiaste en el campo de la derecha.
        *   Ahora puedes enviar la petición y tendrás acceso.
