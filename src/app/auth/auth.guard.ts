import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import Swal from 'sweetalert2';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdmin()) {
    return true;
  }

  Swal.fire({
    icon: 'error',
    title: 'Acceso Denegado',
    text: 'No tienes permisos para acceder a esta página.',
    timer: 2000,
    showConfirmButton: false
  });
  
  router.navigate(['/']);
  return false;
};