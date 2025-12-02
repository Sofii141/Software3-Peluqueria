import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Servicio, Estilista, Disponibilidad, ReservaRequest } from '../modelos/reserva.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReservaService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  obtenerServicios(): Observable<Servicio[]> {
    return this.http.get<Servicio[]>(`${this.apiUrl}/servicios`);
  }

  obtenerEstilistasPorServicio(servicioId: number): Observable<Estilista[]> {
    return this.http.get<Estilista[]>(`${this.apiUrl}/estilistas/servicio/${servicioId}`);
  }

  obtenerDisponibilidad(estilistaId: number, fecha: string): Observable<Disponibilidad> {
    return this.http.get<Disponibilidad>(`${this.apiUrl}/estilistas/${estilistaId}/disponibilidad/${fecha}`);
  }

  crearReserva(reserva: ReservaRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/reservas`, reserva);
  }
}
