import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AyudaComponent } from './ayuda/ayuda.component';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { ListarServiciosComponent } from './servicios/listar-servicios/listar-servicios.component';
import { CrearServicioComponent } from './servicios/crear-servicio/crear-servicio.component';
import { ActualizarServicioComponent } from './servicios/actualizar-servicio/actualizar-servicio.component';

import { adminGuard } from './auth/auth.guard';
import { PrivateLayoutComponent } from './layout/private-layout.component';

export const routes: Routes = [
  // 🌐 RUTAS PÚBLICAS
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // 🔐 RUTAS PRIVADAS (con sidebar y layout privado)
  {
    path: '',
    component: PrivateLayoutComponent,
    children: [
      // ---------------- ADMIN ----------------
      { path: 'admin/servicios', component: ListarServiciosComponent },
      {
        path: 'admin/servicios/crear',
        component: CrearServicioComponent,
        canActivate: [adminGuard]
      },
      {
        path: 'admin/servicios/actualizar/:id',
        component: ActualizarServicioComponent,
        canActivate: [adminGuard]
      },
      {
        path: 'admin/estilistas',
        loadComponent: () =>
          import('./estilistas/estilistas-listar/estilistas-listar.component').then(m => m.EstilistasListarComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/estilistas/crear',
        loadComponent: () =>
          import('./estilistas/estilistas-crear/estilistas-crear.component').then(m => m.EstilistasCrearComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/estilistas/editar/:id',
        loadComponent: () =>
          import('./estilistas/estilistas-editar/estilistas-editar.component').then(m => m.EstilistasEditarComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/categorias',
        loadComponent: () =>
          import('./categorias/categorias-listar/categorias-listar.component').then(m => m.CategoriasListarComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/categorias/crear',
        loadComponent: () =>
          import('./categorias/categorias-crear/categorias-crear.component').then(m => m.CategoriasCrearComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/categorias/editar/:id',
        loadComponent: () =>
          import('./categorias/categorias-editar/categorias-editar.component').then(m => m.CategoriasEditarComponent),
        canActivate: [adminGuard]
      },

      // --------------- CLIENTE ---------------
// {
//   path: 'cliente/reservas',
//   loadComponent: () =>
//     import('./cliente/reservas/reservas.component').then(m => m.ReservasComponent)
// },
// {
//   path: 'cliente/reservas/crear',
//   loadComponent: () =>
//     import('./cliente/reservas/reserva-crear.component').then(m => m.ReservaCrearComponent)
// },

      // --------------- ESTILISTA ---------------
// {
//   path: 'estilista/agenda',
//   loadComponent: () =>
//     import('./estilista/agenda/agenda.component').then(m => m.AgendaComponent)
// },

      // ---------------- COMUNES ----------------
      { path: 'ayuda', component: AyudaComponent },

      // fallback interno (privado)
      { path: '**', redirectTo: 'admin/servicios' }
    ]
  },

  // 🌐 fallback global (público)
  { path: '**', redirectTo: '' }
];
