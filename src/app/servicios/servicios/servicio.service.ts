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
  private imageBaseUrl: string = `${environment.apiUrl}/images/`; 

  constructor(private http: HttpClient) { }
  private mapServicio(servicio: Servicio): Servicio {
    if (servicio.imagen && !servicio.imagen.startsWith('http')) {
      servicio.imagen = this.imageBaseUrl + servicio.imagen; 
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

  deleteServicio(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlEndPoint}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  createWithImage(formData: FormData): Observable<Servicio> {
    return this.http.post<Servicio>(this.urlEndPoint, formData).pipe(
      map(servicio => this.mapServicio(servicio)), 
      catchError(this.handleError)
    );
  }

  /**
   * Actualiza un servicio. Puede incluir una nueva imagen o no.
   * @param id El ID del servicio a actualizar.
   * @param formData Los datos del formulario, incluyendo la imagen opcional.
   */
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
    } else if (error.status === 400 || error.status === 404) {
      // Intentamos obtener el mensaje específico del backend
      mensajeError = error.error?.mensaje || error.error?.title || 'Los datos enviados son incorrectos o el recurso no existe.';
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