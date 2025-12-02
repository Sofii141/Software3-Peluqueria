export interface BloqueoRango {
  id?: number; // Opcional al crear, obligatorio al editar
  fechaInicio: string; // Formato "YYYY-MM-DD"
  fechaFin: string;    // Formato "YYYY-MM-DD"
  razon: string;
}

export interface BloqueoRangoResponse {
  id: number;
  fechaInicio: string;
  fechaFin: string;
  razon: string;
}
