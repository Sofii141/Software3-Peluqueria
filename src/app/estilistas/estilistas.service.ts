import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Estilista } from './estilista.model';
import { environment } from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class EstilistasService {

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** Listar estilistas */
  listar(): Observable<Estilista[]> {
    return this.http.get<Estilista[]>(`${this.baseUrl}/api/usuarios/estilistas`);
  }

  /** Crear estilista */
  crear(estilista: Partial<Estilista>): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/auth/registro`, {
      nombre: estilista.nombre,
      email: estilista.email,
      telefono: estilista.telefono,
      password: estilista.password,
      rol: "ESTILISTA"
    });
  }

  /** Actualizar estilista */
  actualizar(id: number, estilista: Partial<Estilista>): Observable<any> {
    return this.http.put(`${this.baseUrl}/api/usuarios/${id}`, estilista);
  }

  /** Inactivar estilista (solo cambia estado) */
  inactivar(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/api/usuarios/${id}`, { estado: false });
  }

  /** Consultar citas pendientes */
  citasPendientes(id: number): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}/api/reservas/estilista/${id}/pendientes`);
  }
}
