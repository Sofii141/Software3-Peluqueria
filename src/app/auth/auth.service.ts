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

  private currentUserToken = new BehaviorSubject<string | null>(this.getTokenFromStorage());
  private currentUserRole = new BehaviorSubject<string | null>(this.getRoleFromToken(this.getTokenFromStorage()));

  public currentUserToken$: Observable<string | null> = this.currentUserToken.asObservable();
  public currentUserRole$: Observable<string | null> = this.currentUserRole.asObservable();

  // LOGIN: Guarda el token, obtiene rol, y redirige automáticamente
  login(loginRequest: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/login`, loginRequest).pipe(
      tap(response => {
        const token = response.token;
        const role = this.getRoleFromToken(token);

        localStorage.setItem('authToken', token);
        this.currentUserToken.next(token);
        this.currentUserRole.next(role);

        //  REDIRECCIÓN SOLO A RUTAS PRIVADAS
        if (role === 'Admin') {
          this.router.navigate(['/admin/estilistas']);
        } else if (role === 'Cliente') {
          this.router.navigate(['/cliente/reservas']);
        } else if (role === 'Estilista') {
          this.router.navigate(['/estilista/agenda']);
        } else {
          this.router.navigate(['/servicios']); // fallback privado
        }
      })
    );
  }

  // REGISTER: Solo devuelve la respuesta, no guarda token ni redirige
  register(registerRequest: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/register`, registerRequest);
  }

  // Cerrar sesión
  logout(): void {
    localStorage.removeItem('authToken');
    this.currentUserToken.next(null);
    this.currentUserRole.next(null);
    this.router.navigate(['/']);
  }

  // Helpers
  public getToken(): string | null {
    return this.currentUserToken.getValue();
  }

  public getRole(): string | null {
    return this.currentUserRole.getValue();
  }

  public isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  public isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private getTokenFromStorage(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('authToken');
    }
    return null;
  }

  private getRoleFromToken(token: string | null): string | null {
    if (!token) return null;

    try {
      const decodedToken: { role: string } = jwtDecode(token);
      return decodedToken.role;
    } catch (error) {
      console.error("Error decodificando el token", error);
      return null;
    }
  }
}
