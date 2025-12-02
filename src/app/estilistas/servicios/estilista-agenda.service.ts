import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { HorarioDia } from '../modelos/horario-dia';
import { BloqueoRango, BloqueoRangoResponse } from '../modelos/bloqueo-rango';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class EstilistaAgendaService {
  private baseUrl = `${environment.apiUrl}/api/estilistas/agenda`;

  constructor(private http: HttpClient) {}

  // ========== HORARIO BASE ==========

  getHorarioBase(estilistaId: number): Observable<HorarioDia[]> {
    return this.http.get<HorarioDia[]>(`${this.baseUrl}/${estilistaId}/horario-base`).pipe(
      catchError(this.handleError)
    );
  }

  updateHorarioBase(estilistaId: number, horarios: HorarioDia[]): Observable<any> {
    return this.http.put(`${this.baseUrl}/${estilistaId}/horario-base`, horarios).pipe(
      catchError(this.handleError)
    );
  }

  // ========== DESCANSOS FIJOS ==========

  getDescansosFijos(estilistaId: number): Observable<HorarioDia[]> {
    return this.http.get<HorarioDia[]>(`${this.baseUrl}/${estilistaId}/descanso-fijo`).pipe(
      catchError(this.handleError)
    );
  }

  updateDescansosFijos(estilistaId: number, descansos: HorarioDia[]): Observable<any> {
    return this.http.put(`${this.baseUrl}/${estilistaId}/descanso-fijo`, descansos).pipe(
      catchError(this.handleError)
    );
  }

  deleteDescansoFijo(estilistaId: number, dia: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${estilistaId}/descanso-fijo/${dia}`).pipe(
      catchError(this.handleError)
    );
  }

  // ========== BLOQUEOS (VACACIONES) ==========

  getBloqueosDiasLibres(estilistaId: number): Observable<BloqueoRangoResponse[]> {
    return this.http.get<BloqueoRangoResponse[]>(`${this.baseUrl}/${estilistaId}/bloqueo-dias-libres`).pipe(
      catchError(this.handleError)
    );
  }

  createBloqueoDiasLibres(estilistaId: number, bloqueo: BloqueoRango): Observable<any> {
    return this.http.post(`${this.baseUrl}/${estilistaId}/bloqueo-dias-libres`, bloqueo).pipe(
      catchError(this.handleError)
    );
  }

  updateBloqueoDiasLibres(estilistaId: number, bloqueoId: number, bloqueo: BloqueoRango): Observable<any> {
    return this.http.put(`${this.baseUrl}/${estilistaId}/bloqueo-dias-libres/${bloqueoId}`, bloqueo).pipe(
      catchError(this.handleError)
    );
  }

  deleteBloqueoDiasLibres(estilistaId: number, bloqueoId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${estilistaId}/bloqueo-dias-libres/${bloqueoId}`).pipe(
      catchError(this.handleError)
    );
  }

  // ========== MANEJO DE ERRORES ==========

  private handleError(error: HttpErrorResponse) {
    let mensajeError = 'Ocurrió un error inesperado. Por favor, intente más tarde.';

    if (error.status === 0) {
      mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
    } else if (error.status === 401) {
      mensajeError = 'No estás autorizado para realizar esta acción.';
    } else if (error.status === 400) {
      const backendError = error.error;
      if (backendError?.mensaje) {
        mensajeError = backendError.mensaje;
      }
    } else if (error.status === 404) {
      mensajeError = 'El estilista no existe.';
    } else if (error.status === 409) {
      mensajeError = 'Conflicto con reservas existentes.';
    }

    console.error(`Error ${error.status}:`, error.error);

    Swal.fire({
      icon: 'error',
      title: `Error (${error.status})`,
      text: mensajeError,
      confirmButtonText: 'Cerrar'
    });

    return throwError(() => new Error(mensajeError));
  }
}
