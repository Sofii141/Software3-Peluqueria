
/*
 @autor: Juan David Moran
 @descripcion: Clase de configuración para las opciones de conexión a RabbitMQ.
 */

namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}