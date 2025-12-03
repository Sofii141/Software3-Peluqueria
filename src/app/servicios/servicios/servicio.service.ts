import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {environment} from "../../../environments/environment";
import {Servicio, ServicioForm} from "../modelos/servicio";


@Injectable({
  providedIn: 'root'
})
export class ServiciosService {

  private apiUrl = `${environment.apiUrl}/api/servicios`;

  constructor(private http: HttpClient) {}

  /** Listar todos los servicios */
  getServicios(): Observable<Servicio[]> {
    return this.http.get<Servicio[]>(this.apiUrl);
  }

  /** Obtener un servicio por id */
  getServicio(id: number): Observable<Servicio> {
    return this.http.get<Servicio>(`${this.apiUrl}/${id}`);
  }

  /** Crear servicio (usa multipart/form-data) */
  crearServicio(data: ServicioForm, imagen: File | null): Observable<Servicio> {
    const formData = new FormData();

    // Los nombres deben coincidir con el backend (.NET / Postman)
    formData.append('Nombre', data.nombre);
    formData.append('Descripcion', data.descripcion);
    formData.append('Precio', data.precio.toString());
    formData.append('Disponible', String(data.disponible));
    formData.append('CategoriaId', data.categoriaId.toString());

    // Si tienes DuracionMinutos en el backend, puedes añadirlo aquí:
    // formData.append('DuracionMinutos', data.duracionMinutos.toString());

    if (imagen) {
      formData.append('Imagen', imagen);
    }

    return this.http.post<Servicio>(this.apiUrl, formData);
  }

  /** Actualizar servicio (también multipart/form-data) */
  actualizarServicio(id: number, data: ServicioForm, imagen: File | null): Observable<Servicio> {
    const formData = new FormData();

    formData.append('Nombre', data.nombre);
    formData.append('Descripcion', data.descripcion);
    formData.append('Precio', data.precio.toString());
    formData.append('Disponible', String(data.disponible));
    formData.append('CategoriaId', data.categoriaId.toString());

    if (imagen) {
      formData.append('Imagen', imagen);
    }

    return this.http.put<Servicio>(`${this.apiUrl}/${id}`, formData);
  }

  /** Eliminar servicio */
  eliminarServicio(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /** Servicios por categoría (por si los necesitas luego) */
  getServiciosPorCategoria(categoriaId: number): Observable<Servicio[]> {
    return this.http.get<Servicio[]>(`${this.apiUrl}/categoria/${categoriaId}`);
  }
}
