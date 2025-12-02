import { Categoria } from "../../categorias/modelos/categoria";

export class Servicio {
  id!: number;
  nombre!: string;
  descripcion!: string;
  duracionMinutos!: number; // ← AÑADIR ESTA PROPIEDAD
  precio!: number;
  imagen!: string;
  fechaCreacion!: string;
  disponible!: boolean;
  categoria: Categoria | null = null;
}
