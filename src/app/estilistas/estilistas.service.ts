import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Estilista } from './estilista.model';

@Injectable({
  providedIn: 'root'
})
export class EstilistasService {

  private apiUsuarios = 'http://localhost:8080/api/usuarios';
  private apiAuth = 'http://localhost:8080/api/auth';
  private apiReservas = 'http://localhost:8081/reservas';

  constructor(private http: HttpClient) {}

  /** Listar estilistas */
  listar(): Observable<Estilista[]> {
    return this.http.get<Estilista[]>(`${this.apiUsuarios}/estilistas`);
  }

  /** Crear estilista */
  crear(estilista: Partial<Estilista>): Observable<any> {
    return this.http.post(`${this.apiAuth}/registro`, {
      nombre: estilista.nombre,
      email: estilista.email,
      telefono: estilista.telefono,
      password: estilista.password,
      rol: "ESTILISTA"
    });
  }

  /** Actualizar estilista */
  actualizar(id: number, estilista: Partial<Estilista>): Observable<any> {
    return this.http.put(`${this.apiUsuarios}/${id}`, estilista);
  }

  /** Inactivar estilista (solo cambia estado) */
  inactivar(id: number): Observable<any> {
    return this.http.put(`${this.apiUsuarios}/${id}`, { estado: false });
  }

  /** Consultar citas pendientes */
  citasPendientes(id: number): Observable<number> {
    return this.http.get<number>(`${this.apiReservas}/estilista/${id}/pendientes`);
  }
}  