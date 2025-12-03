namespace Peluqueria.Application.Exceptions
{
    /// <summary>
    /// Clase tipo "Smart Enum" que centraliza todos los códigos de error del dominio.
    /// </summary>
    /// <remarks>
    /// Evita el uso de "cadenas mágicas" (magic strings) en el código. 
    /// Cada error tiene un código único (ej: GC-001) y un mensaje base por defecto.
    /// Estos códigos son devueltos al cliente (Frontend) para que pueda mostrar mensajes localizados o tomar decisiones lógicas.
    /// </remarks>
    public class CodigoError
    {
        /// <summary>
        /// Código alfanumérico único (ej: "G-ERROR-001").
        /// </summary>
        public string Codigo { get; private set; }

        /// <summary>
        /// Mensaje descriptivo por defecto asociado al error.
        /// </summary>
        public string LlaveMensaje { get; private set; }

        private CodigoError(string codigo, string llaveMensaje)
        {
            Codigo = codigo;
            LlaveMensaje = llaveMensaje;
        }

        // --- CÓDIGOS DE SISTEMA (GC) ---

        /// <summary>
        /// Error no controlado (Bug o fallo de infraestructura). HTTP 500.
        /// </summary>
        public static readonly CodigoError ERROR_GENERICO = new CodigoError("GC-0001", "Ha ocurrido un error inesperado en el sistema.");

        /// <summary>
        /// Recurso no encontrado genérico. HTTP 404.
        /// </summary>
        public static readonly CodigoError ENTIDAD_NO_ENCONTRADA = new CodigoError("GC-0003", "El recurso solicitado no existe.");

        /// <summary>
        /// Error de formato en la entrada de datos. HTTP 400.
        /// </summary>
        public static readonly CodigoError FORMATO_INVALIDO = new CodigoError("GC-0004", "El formato de los datos es incorrecto.");


        // --- CÓDIGOS DE NEGOCIO (G-ERROR) ---

        /// <summary>
        /// El slot de tiempo ya fue ocupado por otra persona.
        /// </summary>
        public static readonly CodigoError HORARIO_NO_DISPONIBLE = new CodigoError("G-ERROR-001", "El horario ya no está disponible. Seleccione otro.");

        /// <summary>
        /// Faltan datos requeridos en el Request.
        /// </summary>
        public static readonly CodigoError CAMPOS_OBLIGATORIOS = new CodigoError("G-ERROR-002", "Debe completar los campos obligatorios o corregir el formato.");

        /// <summary>
        /// Validación lógica de precios.
        /// </summary>
        public static readonly CodigoError PRECIO_INVALIDO = new CodigoError("G-ERROR-003", "El precio debe ser mayor a cero.");

        /// <summary>
        /// Intento de borrar/modificar un servicio que ya tiene citas agendadas.
        /// </summary>
        public static readonly CodigoError SERVICIO_BLOQUEADO_POR_CITAS = new CodigoError("G-ERROR-004", "No se puede realizar la acción: el servicio tiene citas futuras programadas.");

        /// <summary>
        /// Conflicto de unicidad (Email o Username duplicado).
        /// </summary>
        public static readonly CodigoError ENTIDAD_YA_EXISTE = new CodigoError("G-ERROR-005", "El correo o usuario ya se encuentra registrado.");

        /// <summary>
        /// Regla: Un estilista no puede quedar sin habilidades.
        /// </summary>
        public static readonly CodigoError ESTILISTA_SIN_SERVICIOS = new CodigoError("G-ERROR-006", "Un estilista debe tener al menos un servicio asociado.");

        /// <summary>
        /// Conflicto de unicidad (Nombre de Categoría).
        /// </summary>
        public static readonly CodigoError CATEGORIA_YA_EXISTE = new CodigoError("G-ERROR-007", "La categoría ya existe.");

        /// <summary>
        /// Integridad referencial: Categoría -> Servicios -> Citas.
        /// </summary>
        public static readonly CodigoError CATEGORIA_CON_SERVICIOS = new CodigoError("G-ERROR-008", "No se puede eliminar la categoría porque tiene servicios asociados.");

        /// <summary>
        /// Bloqueo general por existencia de reservas futuras (ej. Vacaciones, Inactivar Estilista).
        /// </summary>
        public static readonly CodigoError OPERACION_BLOQUEADA_POR_CITAS = new CodigoError("G-ERROR-009", "La acción no se puede realizar porque existen citas futuras asociadas.");

        /// <summary>
        /// Conflicto de unicidad (Nombre de Servicio).
        /// </summary>
        public static readonly CodigoError SERVICIO_NOMBRE_DUPLICADO = new CodigoError("G-ERROR-010", "Ya existe un servicio con ese nombre.");

        /// <summary>
        /// Error específico: Servicio no hallado por ID.
        /// </summary>
        public static readonly CodigoError SERVICIO_NO_ENCONTRADO = new CodigoError("G-ERROR-011", "El servicio solicitado no existe.");

        /// <summary>
        /// Error específico: Categoría no hallada por ID.
        /// </summary>
        public static readonly CodigoError CATEGORIA_NO_ENCONTRADA = new CodigoError("G-ERROR-012", "La categoría solicitada no existe.");

        /// <summary>
        /// Error en la subida de archivos (formato o tamaño).
        /// </summary>
        public static readonly CodigoError IMAGEN_INVALIDA = new CodigoError("G-ERROR-013", "El archivo de imagen no es válido o excede el tamaño permitido.");

        /// <summary>
        /// Violación genérica de una regla operativa (ej. Duración > 480 min).
        /// </summary>
        public static readonly CodigoError REGLA_NEGOCIO_VIOLADA = new CodigoError("G-ERROR-014", "Se ha violado una regla de negocio operativa.");

        /// <summary>
        /// Fallo en Login (Usuario o Password incorrectos).
        /// </summary>
        public static readonly CodigoError CREDENCIALES_INVALIDAS = new CodigoError("G-ERROR-015", "Credenciales no válidas. Verifique usuario y contraseña.");

        /// <summary>
        /// Problemas de seguridad (Cuenta bloqueada, Password débil).
        /// </summary>
        public static readonly CodigoError SEGURIDAD_CUENTA = new CodigoError("G-ERROR-016", "Error de seguridad en la cuenta.");
    }
}