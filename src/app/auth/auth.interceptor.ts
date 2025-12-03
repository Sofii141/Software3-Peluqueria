import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { catchError, throwError } from 'rxjs';
import Swal from 'sweetalert2';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.getToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        Swal.fire({
          icon: 'error',
          title: 'Sesión expirada',
          text: 'Tu sesión ha expirado o el token no es válido. Por favor inicia sesión nuevamente.',
          timer: 2500,
          showConfirmButton: false,
        });

        authService.logout();
        router.navigate(['/login']);
      }

      if (error.status === 403) {
        Swal.fire({
          icon: 'error',
          title: 'Acceso denegado',
          text: 'No tienes permisos para realizar esta acción.',
          timer: 2500,
          showConfirmButton: false,
        });
      }

      return throwError(() => error);
    })
  );
};
