namespace peluqueria.reservaciones.Infraestructura.Mensajes
{
    public class RabbitMqOptions
    {
        // Se mapea desde la sección "RabbitMQSettings"
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}