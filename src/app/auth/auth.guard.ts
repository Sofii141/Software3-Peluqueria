import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import Swal from 'sweetalert2';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Si no está logueado
  if (!authService.getToken()) {
    Swal.fire({
      icon: 'warning',
      title: 'Sesión requerida',
      text: 'Debes iniciar sesión para acceder a esta sección.',
      timer: 2000,
      showConfirmButton: false
    });
    router.navigate(['/login']);
    return false;
  }

  // Si está logueado pero no es admin
  if (!authService.isAdmin()) {
    Swal.fire({
      icon: 'error',
      title: 'Acceso denegado',
      text: 'No tienes permisos para acceder a esta página.',
      timer: 2000,
      showConfirmButton: false
    });
    router.navigate(['/']);
    return false;
  }

  return true;
};

