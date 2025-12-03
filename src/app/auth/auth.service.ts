import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest
} from './auth.interfaces';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiAuthUrl = `${environment.apiUrl}/auth`; // ✅ RUTA CORRECTA SEGÚN TU API GATEWAY

  // Estados reactivos
  private currentUserToken = new BehaviorSubject<string | null>(this.getTokenFromStorage());
  private currentUserRole = new BehaviorSubject<string | null>(this.getRoleFromToken(this.getTokenFromStorage()));

  public currentUserToken$ = this.currentUserToken.asObservable();
  public currentUserRole$ = this.currentUserRole.asObservable();

  /** 🔐 LOGIN */
  login(loginRequest: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/login`, loginRequest).pipe(
      tap(response => this.handleLoginSuccess(response))
    );
  }

  /** 📝 REGISTRO */
  register(registerRequest: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiAuthUrl}/register`, registerRequest);
  }

  /** 🚪 LOGOUT */
  logout(): void {
    localStorage.removeItem('jwtToken');
    this.currentUserToken.next(null);
    this.currentUserRole.next(null);
    this.router.navigate(['/']);
  }

  /** ✅ GETTERS */
  getToken(): string | null {
    return this.currentUserToken.getValue();
  }

  getRole(): string | null {
    return this.currentUserRole.getValue();
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  /** 🧠 MANEJO DE TOKEN Y REDIRECCIÓN */
  private handleLoginSuccess(response: LoginResponse): void {
    const token = response.token;

    if (!token) {
      console.error('❌ No se recibió token en la respuesta del login.');
      return;
    }

    const role = this.getRoleFromToken(token);
    if (!role) {
      console.error('❌ No se pudo extraer el rol del token.');
      return;
    }

    // Guardar token y actualizar estado
    localStorage.setItem('jwtToken', token);
    this.currentUserToken.next(token);
    this.currentUserRole.next(role);

    console.log(`✅ Login exitoso. Rol: ${role}`);

    // Redireccionar según rol
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
        break;
    }
  }

  /** 💾 LECTURA DESDE STORAGE */
  private getTokenFromStorage(): string | null {
    return typeof window !== 'undefined'
      ? localStorage.getItem('jwtToken')
      : null;
  }

  /** 🔍 DECODIFICAR ROL DESDE TOKEN */
  private getRoleFromToken(token: string | null): string | null {
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);
      return decoded?.role || decoded?.Role || null;
    } catch (err) {
      console.error('❌ Error decodificando el token:', err);
      return null;
    }
  }
}
