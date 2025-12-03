export interface Servicio {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  imagen: string | null;
  fechaCreacion: string;
  disponible: boolean;
  categoria: {
    id: number;
    nombre: string;
  };
}

export interface ServicioForm {
  nombre: string;
  descripcion: string;
  precio: number;
  disponible: boolean;
  categoriaId: number;
}
