import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Estilista } from '../modelos/estilista.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EstilistasService {
  private urlEndPoint = `${environment.apiUrl}/api/estilistas`;

  constructor(private http: HttpClient) {}

  // Método para listar todos los estilistas
  listar(): Observable<Estilista[]> {
    return this.http.get<Estilista[]>(this.urlEndPoint);
  }

  // ⭐ AÑADIR ESTE MÉTODO ⭐
  getEstilistaById(id: number): Observable<Estilista> {
    return this.http.get<Estilista>(`${this.urlEndPoint}/${id}`);
  }

  // Método para verificar citas pendientes
  citasPendientes(id: number): Observable<number> {
    return this.http.get<number>(`${this.urlEndPoint}/${id}/citas-pendientes`);
  }

  // Método para inactivar estilista
  inactivar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlEndPoint}/${id}`);
  }
}
