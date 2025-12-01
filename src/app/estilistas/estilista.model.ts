export interface Estilista {
  id: number;
  nombre: string;
  nombreUsuario?: string;
  telefono: string;
  email: string;
  estado: boolean;
  servicios: string[];    
  imagenUrl?: string;
  password?: string;
}
