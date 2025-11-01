import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest } from './auth.interfaces'; 

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Inyeccion de dependencias
  private http = inject(HttpClient); // Peticiones HTTP
  private router = inject(Router); // Navegar 
  private apiAuthUrl = `${environment.apiUrl}/api/account`; // URL completa

  // BehaviorSubject para el token y el rol que hay en localStorage
  private currentUserToken = new BehaviorSubject<string | null>(this.getTokenFromStorage());
  private currentUserRole = new BehaviorSubject<string | null>(this.getRoleFromToken(this.getTokenFromStorage()));

  // Observables públicos
  public currentUserToken$: Observable<string | null> = this.currentUserToken.asObservable();
  public currentUserRole$: Observable<string | null> = this.currentUserRole.asObservable();

  // Método de Login 
  login(loginRequest: LoginRequest): Observable<LoginResponse> {
    // Peticion POST
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/login`, loginRequest).pipe(
      tap(response => {
        // Al recibir respuesta, guardamos el token y actualizamos los BehaviorSubjects
        // Sesion persistente 
        localStorage.setItem('authToken', response.token);
        // Se notifica a toda la app que hay nuevo token
        this.currentUserToken.next(response.token);
        // Decodificamos el token y le decimos a la app sobre nuevo rol
        this.currentUserRole.next(this.getRoleFromToken(response.token));
      })
    );
  }

  // Método de Registro
  register(registerRequest: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/register`, registerRequest).pipe(
      tap(response => {
        localStorage.setItem('authToken', response.token);
        this.currentUserToken.next(response.token);
        this.currentUserRole.next(this.getRoleFromToken(response.token));
      })
    );
  }

  logout(): void {
    // Eliminarmos token del almacenamiento
    localStorage.removeItem('authToken');
    // Notificamos a la app que ya no hay token
    this.currentUserToken.next(null);
    // Notificamos que no hay rol
    this.currentUserRole.next(null);
    this.router.navigate(['/']);
  }

  // Devuelve el valor actual del token
  public getToken(): string | null {
    return this.currentUserToken.getValue();
  }

  // Devuelve el valor actual del rol
  public getRole(): string | null {
    return this.currentUserRole.getValue();
  }
  
  // metoodo usado por guard para ver si es admin
  public isAdmin(): boolean {
    return this.getRole() === 'Admin'; 
  }

  private getTokenFromStorage(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('authToken');
    }
    return null;
  }

  // Decodifica el token para obtener el rol
  private getRoleFromToken(token: string | null): string | null {
    if (!token) {
      return null;
    }
    try {
      const decodedToken: { role: string } = jwtDecode(token);
      return decodedToken.role;
    } catch (error) {
      console.error("Error decodificando el token", error);
      return null;
    }
  }

  
}