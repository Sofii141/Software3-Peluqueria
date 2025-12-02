export interface HorarioDia {
  diaSemana: number; // 0=Domingo, 1=Lunes, ..., 6=Sábado
  horaInicio: string; // Formato "HH:mm" (ej: "09:00")
  horaFin: string;    // Formato "HH:mm" (ej: "18:00")
  esLaborable: boolean;
}

export interface HorarioDiaDisplay extends HorarioDia {
  nombreDia: string; // Para mostrar en la UI
}
