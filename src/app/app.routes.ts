import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ListarServiciosComponent } from './servicios/listar-servicios/listar-servicios.component';
import { CrearServicioComponent } from './servicios/crear-servicio/crear-servicio.component';
import { ActualizarServicioComponent } from './servicios/actualizar-servicio/actualizar-servicio.component';
import { OfertasComponent } from './ofertas/ofertas.component';
import { CuponesComponent } from './cupones/cupones.component';
import { AyudaComponent } from './ayuda/ayuda.component';
import { adminGuard } from './auth/auth.guard';

import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';

// Importa el LAYOUT GENERAL
import { SidebarComponent } from './layout/sidebar.component';

export const routes: Routes = [
  // -------------------------
  // RUTAS PÚBLICAS (SIN SIDEBAR)
  // -------------------------
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // -------------------------
  // RUTAS PRIVADAS CON SIDEBAR
  // (todas dentro del LayoutComponent)
  // -------------------------
  {
    path: '',
    component: SidebarComponent,  // ← TU LAYOUT QUE INCLUYE EL SIDEBAR

    children: [
      // ---- SERVICIOS ----
      { path: 'servicios', component: ListarServiciosComponent },
      {
        path: 'servicios/crear',
        component: CrearServicioComponent,
        canActivate: [adminGuard]
      },
      {
        path: 'servicios/actualizar/:id',
        component: ActualizarServicioComponent,
        canActivate: [adminGuard]
      },

      // ---- OFERTAS / CUPONES / AYUDA ----
      { path: 'ofertas', component: OfertasComponent },
      { path: 'cupones', component: CuponesComponent },
      { path: 'ayuda', component: AyudaComponent },

      // ------------ ADMIN ESTILISTAS ------------
      {
        path: 'admin/estilistas',
        loadComponent: () =>
          import('./estilistas/estilistas-listar/estilistas-listar.component')
            .then(m => m.EstilistasListarComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/estilistas/crear',
        loadComponent: () =>
          import('./estilistas/estilistas-crear/estilistas-crear.component')
            .then(m => m.EstilistasCrearComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/estilistas/editar/:id',
        loadComponent: () =>
          import('./estilistas/estilistas-editar/estilistas-editar.component')
            .then(m => m.EstilistasEditarComponent),
        canActivate: [adminGuard]
      },

      // ------------ ADMIN CATEGORIAS ------------
      {
        path: 'admin/categorias',
        loadComponent: () =>
          import('./categorias/categorias-listar/categorias-listar.component')
            .then(m => m.CategoriasListarComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/categorias/crear',
        loadComponent: () =>
          import('./categorias/categorias-crear/categorias-crear.component')
            .then(m => m.CategoriasCrearComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'admin/categorias/editar/:id',
        loadComponent: () =>
          import('./categorias/categorias-editar/categorias-editar.component')
            .then(m => m.CategoriasEditarComponent),
        canActivate: [adminGuard]
      },

      // Fallback dentro del layout
      { path: '**', redirectTo: '' }
    ]
  },

  // Fallback global
  { path: '**', redirectTo: '' }
];
