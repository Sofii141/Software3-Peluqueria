# Microservicio Reservaciones



Para la ejecución del microservicio se necesitan las siguientes herramientas:



Editor Visual Studio (Para mayor comodidad)

.NET versión 8.0

SLQ server (para base de datos)



### Instrucciones para ejecución



Antes de ejecutar el microservicio debemos generar las migraciones iniciales y crear la base de datos, para hacer esto debemos ejecutar los siguientes comandos: 



Desde el panel de administrador de paquetes ubicado en el proyecto peluqueria.reservaciones.Infraestructura ejecutamos los siguientes comandos:



Add-Migration InitialReservationsSchema -Project peluqueria.reservaciones.Infraestructura -StartupProject peluqueria.reservaciones.Api



Update-Database -Project peluqueria.reservaciones.Infraestructura -StartupProject peluqueria.reservaciones.Api



Una vez con la base de datos creada ejecutamos el microservicio abriendo el archivo PeluqueriaReservaciones.sln y damos click en play en la interfaz de Visual Studio



### Nota



En este proyecto se utiliza una base de datos SQL server en caso de usar una base de datos diferentes se deberá modificar la cadena de conexión a la base de datos que se encuentra en el archivo appsettings.json dentro del proyecto peluqueria.reservaciones.Api .





