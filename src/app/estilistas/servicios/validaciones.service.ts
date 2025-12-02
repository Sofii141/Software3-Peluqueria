import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ValidacionesService {
  private baseUrl = `${environment.apiUrl}/api/validaciones`;

  constructor(private http: HttpClient) {}

  // Verificar si un día específico tiene citas
  tieneCitasEnDia(estilistaId: number, dia: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/estilista/${estilistaId}/dia/${dia}`);
  }

  // Verificar si un rango de fechas tiene citas
  tieneCitasEnRango(estilistaId: number, inicio: string, fin: string): Observable<boolean> {
    return this.http.get<boolean>(
      `${this.baseUrl}/estilista/${estilistaId}/rango?inicio=${inicio}&fin=${fin}`
    );
  }

  // Verificar si un horario de descanso tiene citas
  tieneCitasEnDescanso(estilistaId: number, dia: number, inicio: string, fin: string): Observable<boolean> {
    return this.http.get<boolean>(
      `${this.baseUrl}/estilista/${estilistaId}/descanso/${dia}?inicio=${inicio}&fin=${fin}`
    );
  }
}
