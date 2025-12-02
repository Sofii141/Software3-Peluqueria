import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Reserva, ReservaDetalle, EstadoReserva } from '../modelos/reserva.model';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class AgendaEstilistaService {
  private apiUrl = `${environment.apiUrl}/api/reservas`;

  constructor(private http: HttpClient) {}

  // ========== OBTENER RESERVAS POR FECHA ==========
  getReservasPorFecha(estilistaId: number, fecha: string): Observable<ReservaDetalle[]> {
    return this.http.get<Reserva[]>(`${this.apiUrl}/estilista/${estilistaId}/fecha/${fecha}`).pipe(
      map(reservas => reservas.map(r => this.calcularEstadoReserva(r))),
      catchError(this.handleError)
    );
  }

  // ========== OBTENER DETALLE DE UNA RESERVA ==========
  getReservaDetalle(reservaId: number): Observable<ReservaDetalle> {
    return this.http.get<Reserva>(`${this.apiUrl}/${reservaId}`).pipe(
      map(r => this.calcularEstadoReserva(r)),
      catchError(this.handleError)
    );
  }

  // ========== CHECK-IN (Marcar llegada del cliente) ==========
  marcarCheckIn(reservaId: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${reservaId}/check-in`, {}).pipe(
      catchError(this.handleError)
    );
  }

  // ========== INICIAR SERVICIO ==========
  iniciarServicio(reservaId: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${reservaId}/iniciar`, {}).pipe(
      catchError(this.handleError)
    );
  }

  // ========== FINALIZAR SERVICIO ==========
  finalizarServicio(reservaId: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${reservaId}/finalizar`, {}).pipe(
      catchError(this.handleError)
    );
  }

  // ========== MARCAR NO-SHOW ==========
  marcarNoShow(reservaId: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${reservaId}/no-show`, {}).pipe(
      catchError(this.handleError)
    );
  }

  // ========== LÓGICA DE NEGOCIO: CALCULAR PERMISOS ==========
  private calcularEstadoReserva(reserva: Reserva): ReservaDetalle {
    const ahora = new Date();
    const fechaHoraReserva = new Date(`${reserva.fecha}T${reserva.horaInicio}`);
    const minutosParaInicio = Math.floor((fechaHoraReserva.getTime() - ahora.getTime()) / 60000);

    return {
      ...reserva,
      minutosParaInicio,
      puedeIniciar: reserva.estado === EstadoReserva.CONFIRMADA && minutosParaInicio <= 0,
      puedeMarcarNoShow: reserva.estado === EstadoReserva.CONFIRMADA && minutosParaInicio < -10,
      puedeFinalizar: reserva.estado === EstadoReserva.EN_CURSO
    };
  }

  // ========== MANEJO DE ERRORES ==========
  private handleError(error: HttpErrorResponse) {
    let mensajeError = 'Ocurrió un error inesperado.';

    if (error.status === 0) {
      mensajeError = 'No se pudo conectar con el servidor.';
    } else if (error.status === 400) {
      mensajeError = error.error?.mensaje || 'Solicitud inválida.';
    } else if (error.status === 404) {
      mensajeError = 'Reserva no encontrada.';
    } else if (error.status === 409) {
      mensajeError = error.error?.mensaje || 'Conflicto con el estado de la reserva.';
    }

    Swal.fire({
      icon: 'error',
      title: `Error (${error.status})`,
      text: mensajeError,
      confirmButtonText: 'Cerrar'
    });

    return throwError(() => new Error(mensajeError));
  }
}
