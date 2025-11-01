import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Inyectamos el AuthService para poder acceder al token.
  const authService = inject(AuthService);
  // Obtenemos el token actual.
  const authToken = authService.getToken();

  // Si tenemos un token, clonamos la petición y añadimos la cabecera de autorización
  if (authToken) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${authToken}`
      }
    });
    return next(authReq);
  }

  // Si no hay token, la petición sigue su curso sin modificar
  return next(req);
};