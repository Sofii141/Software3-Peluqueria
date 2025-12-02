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

  /** LOGIN */
  login(loginRequest: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/login`, loginRequest).pipe(
      tap(response => {
        const token = response?.token;

        if (!token) {
          console.error('❌ No se recibió token en la respuesta del login.');
          return;
        }

        const role = this.getRoleFromToken(token);
        if (!role) {
          console.error('❌ No se pudo extraer el rol del token.');
          return;
        }

        localStorage.setItem('authToken', token);
        this.currentUserToken.next(token);
        this.currentUserRole.next(role);

        console.log(`✅ Login exitoso. Rol: ${role}`);

        // Redirección según rol
        switch (role) {
          case 'Admin':
            this.router.navigate(['/admin/estilistas']);
            break;
          case 'Cliente':
            this.router.navigate(['/cliente/reservas']);
            break;
          case 'Estilista':
            this.router.navigate(['/estilista/agenda']);
            break;
          default:
            this.router.navigate(['/servicios']);
        }
      })
    );
  }

  /** REGISTRO */
  register(registerRequest: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/register`, registerRequest);
  }

  /** CERRAR SESIÓN */
  logout(): void {
    localStorage.removeItem('authToken');
    this.currentUserToken.next(null);
    this.currentUserRole.next(null);
    this.router.navigate(['/']);
  }

  /** GETTERS */
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

  /** TOKEN STORAGE */
  private getTokenFromStorage(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('authToken');
    }
    return null;
  }

  /** DECODIFICAR TOKEN */
  private getRoleFromToken(token: string | null): string | null {
    if (!token) return null;

    try {
      const decodedToken: any = jwtDecode(token);
      return decodedToken?.role || null;
    } catch (error) {
      console.error('❌ Error decodificando el token:', error);
      return null;
    }
  }
}
