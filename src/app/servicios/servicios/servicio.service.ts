import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Servicio } from '../modelos/servicio';
import { catchError, map, Observable, throwError } from 'rxjs';
import Swal from 'sweetalert2';
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class ServicioService {
  private urlEndPoint: string = `${environment.apiUrl}/api/servicios`;

  constructor(private http: HttpClient) { }

  private mapServicio(servicio: Servicio): Servicio {
    if (servicio.imagen && !servicio.imagen.toLowerCase().startsWith('http')) {
      servicio.imagen = `${environment.apiUrl}/images/${servicio.imagen}`;
    }
    return servicio;
  }

  getServicios(): Observable<Servicio[]> {
    return this.http.get<Servicio[]>(this.urlEndPoint).pipe(
      map(servicios => servicios.map(servicio => this.mapServicio(servicio)))
    );
  }

  getServicioById(id: number): Observable<Servicio> {
    return this.http.get<Servicio>(`${this.urlEndPoint}/${id}`).pipe(
      map(servicio => this.mapServicio(servicio))
    );
  }

  getServiciosPorCategoria(categoriaId: number): Observable<Servicio[]> {
    if (!categoriaId || categoriaId === 0) {
      return this.getServicios();
    }
    return this.http.get<Servicio[]>(`${this.urlEndPoint}/categoria/${categoriaId}`).pipe(
      map(servicios => servicios.map(servicio => this.mapServicio(servicio)))
    );
  }

  // ← NUEVO: Verificar si tiene reservas futuras
  verificarReservasFuturas(servicioId: number): Observable<boolean> {
    return this.http.get<boolean>(`${environment.apiUrl}/api/validaciones/servicio/${servicioId}`).pipe(
      catchError(() => {
        // Si falla la petición, asumimos que SÍ tiene reservas (seguridad)
        return throwError(() => new Error('No se pudo verificar las reservas'));
      })
    );
  }

  deleteServicio(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlEndPoint}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  reactivarServicio(id: number): Observable<Servicio> {
    // Para reactivar, hacemos un PUT con Disponible = true
    const formData = new FormData();
    formData.append('Disponible', 'true');

    return this.http.put<Servicio>(`${this.urlEndPoint}/${id}`, formData).pipe(
      map(servicio => this.mapServicio(servicio)),
      catchError(this.handleError)
    );
  }

  createWithImage(formData: FormData): Observable<Servicio> {
    return this.http.post<Servicio>(this.urlEndPoint, formData).pipe(
      map(servicio => this.mapServicio(servicio)),
      catchError(this.handleError)
    );
  }

  updateWithImage(id: number, formData: FormData): Observable<Servicio> {
    return this.http.put<Servicio>(`${this.urlEndPoint}/${id}`, formData).pipe(
      map(servicio => this.mapServicio(servicio)),
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    let mensajeError = 'Ocurrió un error inesperado. Por favor, intente más tarde.';

    if (error.status === 0) {
      mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
    } else if (error.status === 401) {
      mensajeError = 'No estás autorizado para realizar esta acción.';
    } else if (error.status === 400) {
      // Extraer mensaje del backend
      const backendError = error.error;
      if (backendError?.mensaje) {
        mensajeError = backendError.mensaje;
      } else if (backendError?.title) {
        mensajeError = backendError.title;
      }
    } else if (error.status === 404) {
      mensajeError = 'El servicio solicitado no existe.';
    } else if (error.status === 409) {
      mensajeError = 'Ya existe un servicio con ese nombre.';
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
