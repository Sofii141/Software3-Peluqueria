import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Categoria } from "../modelos/categoria";
import { catchError, Observable, throwError } from "rxjs";
import { environment } from "../../../environments/environment";
import Swal from "sweetalert2";

@Injectable({
  providedIn: 'root'
})
export class CategoriaService {
  private urlEndPoint: string = `${environment.apiUrl}/api/categorias`;

  constructor(private http: HttpClient) { }

  // Obtener todas las categorías
  getCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(this.urlEndPoint).pipe(
      catchError(this.handleError)
    );
  }

  // Obtener una categoría por ID
  getCategoriaById(id: number): Observable<Categoria> {
    return this.http.get<Categoria>(`${this.urlEndPoint}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // Crear una nueva categoría
  crearCategoria(categoria: { nombre: string }): Observable<Categoria> {
    return this.http.post<Categoria>(this.urlEndPoint, categoria).pipe(
      catchError(this.handleError)
    );
  }

  // Actualizar una categoría existente
  actualizarCategoria(id: number, categoria: { nombre: string; estaActiva: boolean }): Observable<Categoria> {
    return this.http.put<Categoria>(`${this.urlEndPoint}/${id}`, categoria).pipe(
      catchError(this.handleError)
    );
  }

  // Inactivar una categoría (baja lógica)
  inactivarCategoria(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlEndPoint}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // Verificar si tiene servicios con reservas futuras
  verificarReservasFuturas(categoriaId: number): Observable<boolean> {
    return this.http.get<boolean>(`${environment.apiUrl}/api/validaciones/categoria/${categoriaId}`).pipe(
      catchError(() => {
        // Si falla, asumimos que SÍ tiene reservas (seguridad)
        return throwError(() => new Error('No se pudo verificar las reservas'));
      })
    );
  }

  // Manejo centralizado de errores
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
      mensajeError = 'La categoría solicitada no existe.';
    } else if (error.status === 409) {
      mensajeError = 'Ya existe una categoría con ese nombre.';
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
