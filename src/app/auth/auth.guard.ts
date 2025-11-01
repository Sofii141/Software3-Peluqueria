import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import Swal from 'sweetalert2';

// 'route' y 'state' son información sobre la ruta a la que se intenta acceder.
export const adminGuard: CanActivateFn = (route, state) => {
  // Inyectamos el servicio de autenticación y el router.
  const authService = inject(AuthService);
  const router = inject(Router);

  // Le preguntamos al servicio si el usuario actual es un administrador.
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
  // Devolvemos 'false'. El acceso a la ruta está bloqueado.
  return false;
};