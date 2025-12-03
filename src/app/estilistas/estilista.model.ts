export interface Estilista {
  id: number;
  nombreCompleto: string;
  username: string;
  telefono: string;
  email: string;
  estado: boolean;
  servicios: number[];        // IDs de servicios (para backend)
  imagenUrl?: string;    
  password?: string; // ← agregar esto     
}