import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Categoria } from "../modelos/categoria";
import { Observable } from "rxjs";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class CategoriaService {
  // Usamos la URL del environment
  private urlEndPoint: string = `${environment.apiUrl}/api/categorias`;

  constructor(private http: HttpClient) { }

  getCategorias(): Observable<Categoria[]> {
    console.log("Listando categorias desde el servicio");
    return this.http.get<Categoria[]>(this.urlEndPoint);
  }
}