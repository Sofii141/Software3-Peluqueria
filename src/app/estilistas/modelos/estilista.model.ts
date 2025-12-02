export interface Estilista {
  id: number;
  nombreCompleto: string;
  email: string;
  telefono: string;
  estaActivo: boolean;
  imagen: string;
  serviciosIds: number[];
}

export interface CreateEstilistaRequest {
  username: string;
  email: string;
  password: string;
  nombreCompleto: string;
  telefono: string;
  serviciosIds: number[];
  imagen?: File;
}

export interface UpdateEstilistaRequest {
  nombreCompleto: string;
  telefono: string;
  password?: string;
  username?: string;
  email?: string;
  serviciosIds: number[];
  imagen?: File;
}
