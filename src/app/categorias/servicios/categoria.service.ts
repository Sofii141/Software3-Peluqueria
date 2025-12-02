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

  constructor(private http: HttpClient) { }

  getCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(this.apiUrl);
  }

  getCategoria(id: number): Observable<Categoria> {
    return this.http.get<Categoria>(`${this.apiUrl}/${id}`);
  }

  crearCategoria(categoria: Partial<Categoria>): Observable<any> {
    return this.http.post(this.apiUrl, categoria);
  }

  actualizarCategoria(id: number, categoria: Partial<Categoria>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, categoria);
  }

  inactivarCategoria(id: number): Observable<any> {

    return this.http.put(`${this.apiUrl}/${id}/inactivar`, {});
  }
}
