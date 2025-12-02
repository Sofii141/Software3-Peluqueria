export interface Reserva {
  id: number;
  clienteId: number;
  clienteNombre: string;
  clienteTelefono: string;
  clienteEmail: string;
  estilistaId: number;
  estilistaNombre: string;
  servicioId: number;
  servicioNombre: string;
  duracionMinutos: number;
  fecha: string; // "2025-01-15"
  horaInicio: string; // "14:00"
  horaFin: string; // "15:30"
  estado: EstadoReserva;
  precio: number;
  notas?: string;
}

export enum EstadoReserva {
  PENDIENTE = 'PENDIENTE',
  CONFIRMADA = 'CONFIRMADA',
  EN_CURSO = 'EN_CURSO',
  COMPLETADA = 'COMPLETADA',
  CANCELADA = 'CANCELADA',
  NO_SHOW = 'NO_SHOW'
}

export interface ReservaDetalle extends Reserva {
  puedeIniciar: boolean;
  puedeMarcarNoShow: boolean;
  puedeFinalizar: boolean;
  minutosParaInicio: number;
}
