import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Categoria } from "../modelos/categoria";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class CategoriaService {

  private apiUrl = `${environment.apiUrl}/api/categorias`;

  constructor(private http: HttpClient) {}

  /** Obtener todas las categorías */
  getCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(this.apiUrl);
  }

  /** Obtener una sola categoría por ID */
  getCategoria(id: number): Observable<Categoria> {
    return this.http.get<Categoria>(`${this.apiUrl}/${id}`);
  }

  /** Crear una nueva categoría */
  crearCategoria(categoria: Partial<Categoria>): Observable<Categoria> {
    return this.http.post<Categoria>(this.apiUrl, categoria);
  }

  /** Actualizar una categoría existente */
  actualizarCategoria(id: number, categoria: Partial<Categoria>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, categoria);
  }

  /** Inactivar una categoría */
  inactivarCategoria(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/inactivar`, {});
  }
}
