import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import Swal from 'sweetalert2';
import {Estilista} from "../modelos/estilista.model";

@Injectable({
  providedIn: 'root'
})
export class EstilistasService {
  private apiUrl = `${environment.apiUrl}/api/estilistas`;

  constructor(private http: HttpClient) {}

  // ========== MAPEO DE IMAGEN ==========
  private mapEstilista(estilista: Estilista): Estilista {
    if (estilista.imagen && !estilista.imagen.toLowerCase().startsWith('http')) {
      estilista.imagen = `${environment.apiUrl}/images/estilistas/${estilista.imagen}`;
    }
    return estilista;
  }

  // ========== OBTENER TODOS LOS ESTILISTAS ==========
  getEstilistas(): Observable<Estilista[]> {
    return this.http.get<Estilista[]>(this.apiUrl).pipe(
      map(estilistas => estilistas.map(e => this.mapEstilista(e))),
      catchError(this.handleError)
    );
  }

  // ========== OBTENER ESTILISTA POR ID ==========
  getEstilistaById(id: number): Observable<Estilista> {
    return this.http.get<Estilista>(`${this.apiUrl}/${id}`).pipe(
      map(estilista => this.mapEstilista(estilista)),
      catchError(this.handleError)
    );
  }

  // ========== CREAR ESTILISTA (NUEVO) ==========
  crearEstilista(formData: FormData): Observable<Estilista> {
    return this.http.post<Estilista>(this.apiUrl, formData).pipe(
      map(estilista => this.mapEstilista(estilista)),
      catchError(this.handleError)
    );
  }

  // ========== ACTUALIZAR ESTILISTA ==========
  actualizarEstilista(id: number, formData: FormData): Observable<Estilista> {
    return this.http.put<Estilista>(`${this.apiUrl}/${id}`, formData).pipe(
      map(estilista => this.mapEstilista(estilista)),
      catchError(this.handleError)
    );
  }

  // ========== INACTIVAR ESTILISTA ==========
  inactivarEstilista(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // ========== VERIFICAR RESERVAS FUTURAS ==========
  verificarReservasFuturas(estilistaId: number): Observable<boolean> {
    return this.http.get<boolean>(`${environment.apiUrl}/api/validaciones/estilista/${estilistaId}`).pipe(
      catchError(() => {
        // Si falla, asumimos que SÍ tiene reservas (seguridad)
        return throwError(() => new Error('No se pudo verificar las reservas'));
      })
    );
  }

  // ========== MANEJO DE ERRORES ==========
  private handleError(error: HttpErrorResponse) {
    let mensajeError = 'Ocurrió un error inesperado.';

    if (error.status === 0) {
      mensajeError = 'No se pudo conectar con el servidor.';
    } else if (error.status === 400) {
      // Extraer mensaje del backend
      const backendError = error.error;
      if (backendError?.mensaje) {
        mensajeError = backendError.mensaje;
      } else if (backendError?.title) {
        mensajeError = backendError.title;
      } else {
        mensajeError = 'Datos inválidos. Verifique los campos.';
      }
    } else if (error.status === 401) {
      mensajeError = 'No estás autorizado para realizar esta acción.';
    } else if (error.status === 404) {
      mensajeError = 'El estilista solicitado no existe.';
    } else if (error.status === 409) {
      mensajeError = error.error?.mensaje || 'El correo o usuario ya está registrado.';
    } else if (error.status === 500) {
      mensajeError = 'Error interno del servidor. Contacte al administrador.';
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
