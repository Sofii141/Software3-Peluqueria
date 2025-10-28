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
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiAuthUrl = `${environment.apiUrl}/api/account`;

  // BehaviorSubject para el token y el rol
  private currentUserToken = new BehaviorSubject<string | null>(this.getTokenFromStorage());
  private currentUserRole = new BehaviorSubject<string | null>(this.getRoleFromToken(this.getTokenFromStorage()));

  // Observables públicos
  public currentUserToken$: Observable<string | null> = this.currentUserToken.asObservable();
  public currentUserRole$: Observable<string | null> = this.currentUserRole.asObservable();

  // Método de Login 
  login(loginRequest: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/login`, loginRequest).pipe(
      tap(response => {
        // Al recibir respuesta, guardamos el token y actualizamos los BehaviorSubjects
        localStorage.setItem('authToken', response.token);
        this.currentUserToken.next(response.token);
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
    localStorage.removeItem('authToken');
    this.currentUserToken.next(null);
    this.currentUserRole.next(null);
    this.router.navigate(['/']);
  }

  public getToken(): string | null {
    return this.currentUserToken.getValue();
  }

  public getRole(): string | null {
    return this.currentUserRole.getValue();
  }
  
  public isAdmin(): boolean {
    return this.getRole() === 'Admin'; // El rol viene del backend
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
      // En tu backend, el claim se llama 'role'. Si se llamara de otra forma, ajústalo aquí.
      return decodedToken.role;
    } catch (error) {
      console.error("Error decodificando el token", error);
      return null;
    }
  }

  
}