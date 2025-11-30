namespace Peluqueria.Application.Exceptions
{
    public class CodigoError
    {
        public string Codigo { get; private set; }
        public string LlaveMensaje { get; private set; }

        private CodigoError(string codigo, string llaveMensaje)
        {
            Codigo = codigo;
            LlaveMensaje = llaveMensaje;
        }

        // --- CÓDIGOS DE SISTEMA (GC) ---

        // GC-0001: Error no controlado (500)
        public static readonly CodigoError ERROR_GENERICO = new CodigoError("GC-0001", "Ha ocurrido un error inesperado en el sistema.");

        // GC-0002: (Reservado para validaciones de DTO automática, aunque usamos G-ERROR-002 normalmente)

        // GC-0003: Recurso no encontrado (Generic 404)
        public static readonly CodigoError ENTIDAD_NO_ENCONTRADA = new CodigoError("GC-0003", "El recurso solicitado no existe.");

        // GC-0004: Formato inválido (Generic)
        public static readonly CodigoError FORMATO_INVALIDO = new CodigoError("GC-0004", "El formato de los datos es incorrecto.");


        // --- CÓDIGOS DE NEGOCIO (G-ERROR) ---

        // G-ERROR-001: Disponibilidad
        public static readonly CodigoError HORARIO_NO_DISPONIBLE = new CodigoError("G-ERROR-001", "El horario ya no está disponible. Seleccione otro.");

        // G-ERROR-002: Validación de Entrada (Campos vacíos, tipos de datos, regex)
        public static readonly CodigoError CAMPOS_OBLIGATORIOS = new CodigoError("G-ERROR-002", "Debe completar los campos obligatorios o corregir el formato.");

        // G-ERROR-003: Lógica de Precios
        public static readonly CodigoError PRECIO_INVALIDO = new CodigoError("G-ERROR-003", "El precio debe ser mayor a cero.");

        // G-ERROR-004: Integridad Referencial (Servicios con Citas)
        public static readonly CodigoError SERVICIO_BLOQUEADO_POR_CITAS = new CodigoError("G-ERROR-004", "No se puede realizar la acción: el servicio tiene citas futuras programadas.");

        // G-ERROR-005: Duplicidad (Usuarios/Emails)
        public static readonly CodigoError ENTIDAD_YA_EXISTE = new CodigoError("G-ERROR-005", "El correo o usuario ya se encuentra registrado.");

        // G-ERROR-006: Regla Estilista
        public static readonly CodigoError ESTILISTA_SIN_SERVICIOS = new CodigoError("G-ERROR-006", "Un estilista debe tener al menos un servicio asociado.");

        // G-ERROR-007: Duplicidad (Categorías)
        public static readonly CodigoError CATEGORIA_YA_EXISTE = new CodigoError("G-ERROR-007", "La categoría ya existe.");

        // G-ERROR-008: Integridad Referencial (Categorías con Servicios)
        public static readonly CodigoError CATEGORIA_CON_SERVICIOS = new CodigoError("G-ERROR-008", "No se puede eliminar la categoría porque tiene servicios asociados.");

        // G-ERROR-009: Bloqueo Genérico (Citas)
        public static readonly CodigoError OPERACION_BLOQUEADA_POR_CITAS = new CodigoError("G-ERROR-009", "La acción no se puede realizar porque existen citas futuras asociadas.");

        // G-ERROR-010: Duplicidad (Nombre Servicio)
        public static readonly CodigoError SERVICIO_NOMBRE_DUPLICADO = new CodigoError("G-ERROR-010", "Ya existe un servicio con ese nombre.");

        // G-ERROR-011: No Encontrado (Servicio Específico)
        public static readonly CodigoError SERVICIO_NO_ENCONTRADO = new CodigoError("G-ERROR-011", "El servicio solicitado no existe.");

        // G-ERROR-012: No Encontrado (Categoría Específica)
        public static readonly CodigoError CATEGORIA_NO_ENCONTRADA = new CodigoError("G-ERROR-012", "La categoría solicitada no existe.");

        // G-ERROR-013: Archivos (Imágenes)
        public static readonly CodigoError IMAGEN_INVALIDA = new CodigoError("G-ERROR-013", "El archivo de imagen no es válido o excede el tamaño permitido.");

        // G-ERROR-014: Reglas Lógicas de Negocio (Ej: Duración > 480 min)
        public static readonly CodigoError REGLA_NEGOCIO_VIOLADA = new CodigoError("G-ERROR-014", "Se ha violado una regla de negocio operativa.");

        // G-ERROR-015: Autenticación (Login) - ANTES ERA ERROR-HU07-02
        public static readonly CodigoError CREDENCIALES_INVALIDAS = new CodigoError("G-ERROR-015", "Credenciales no válidas. Verifique usuario y contraseña.");

        // G-ERROR-016: Seguridad (Password Débil, Cuenta Bloqueada, etc.)
        public static readonly CodigoError SEGURIDAD_CUENTA = new CodigoError("G-ERROR-016", "Error de seguridad en la cuenta.");
    }
}