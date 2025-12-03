import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AyudaComponent } from './ayuda/ayuda.component';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';

// Servicios (admin)
import { ServiciosListarComponent } from './servicios/listar-servicios/servicios-listar.component';
import { ServiciosCrearComponent } from './servicios/crear-servicio/servicios-crear.component';
import { ServiciosEditarComponent } from './servicios/actualizar-servicio/servicios-editar.component';

import { adminGuard } from './auth/auth.guard';
import { PrivateLayoutComponent } from './layout/private-layout.component';

export const routes: Routes = [
  // 🌐 RUTAS PÚBLICAS
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // 🔐 RUTAS PRIVADAS (con sidebar y layout privado)
  {
    path: '',
    component: PrivateLayoutComponent,
    children: [
      // ---------------- ADMIN ----------------
      { path: 'admin/servicios', component: ServiciosListarComponent },

      { 
        path: 'admin/servicios/crear',
        component: ServiciosCrearComponent,
        canActivate: [adminGuard] 
      },

      { 
        path: 'admin/servicios/actualizar/:id',
        component: ServiciosEditarComponent,
        canActivate: [adminGuard] 
      },

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

      // ---------------- COMUNES ----------------
      { path: 'ayuda', component: AyudaComponent },

      // fallback interno (privado)
      { path: '**', redirectTo: 'admin/servicios' }
    ]
  },

  // 🌐 fallback global (público)
  { path: '**', redirectTo: '' }
];
