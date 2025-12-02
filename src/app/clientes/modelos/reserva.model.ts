export interface Servicio {
  id: number;
  nombre: string;
  duracionMinutos: number;
  precio: number;
}

export interface Estilista {
  id: number;
  nombreCompleto: string;
  serviciosIds: number[]; // Servicios que brinda el estilista
}

export interface Disponibilidad {
  fecha: string; // formato YYYY-MM-DD
  horasDisponibles: string[]; // ej: ["09:00", "09:30", "10:00"]
}

export interface ReservaRequest {
  clienteId: number;
  servicioId: number;
  estilistaId: number;
  fecha: string;   // YYYY-MM-DD
  horaInicio: string; // HH:mm
  notas?: string;
}
