import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Estilista } from './estilista.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EstilistasService {

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** Listar estilistas */
  listar(): Observable<Estilista[]> {
    return this.http.get<Estilista[]>(`${this.baseUrl}/api/estilistas`);
  }

  /** Obtener un estilista por ID */
  obtener(id: number): Observable<Estilista> {
    return this.http.get<Estilista>(`${this.baseUrl}/api/estilistas/${id}`);
  }

  /** Crear estilista (con imagen y servicios) */
  crear(estilista: any, imagen: File | null): Observable<any> {
    const formData = new FormData();

    formData.append('Username', estilista.username);
    formData.append('NombreCompleto', estilista.nombreCompleto);
    formData.append('Email', estilista.email);
    formData.append('Telefono', estilista.telefono);
    formData.append('Password', estilista.password);

    estilista.servicios.forEach((servicioId: number | string) => {
      formData.append('ServiciosIds', servicioId.toString());
    });

    if (imagen) {
      formData.append('Imagen', imagen);
    }

    return this.http.post(`${this.baseUrl}/api/estilistas`, formData);
  }

  /** Actualizar estilista (con imagen opcional) */
  actualizar(id: number, estilista: any, imagen: File | null): Observable<any> {
    const formData = new FormData();

    formData.append('Username', estilista.username);
    formData.append('NombreCompleto', estilista.nombreCompleto);
    formData.append('Email', estilista.email);
    formData.append('Telefono', estilista.telefono);
    formData.append('Estado', String(estilista.estado)); // importante

    estilista.servicios.forEach((servicioId: number | string) => {
      formData.append('ServiciosIds', servicioId.toString());
    });

    if (imagen) {
      formData.append('Imagen', imagen);
    }

    return this.http.put(`${this.baseUrl}/api/estilistas/${id}`, formData);
  }

  /** Inactivar estilista */
  inactivar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/api/estilistas/${id}`);
  }

  /** Consultar número de citas pendientes */
  citasPendientes(id: number): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}/api/reservas/estilista/${id}/pendientes`);
  }

}
