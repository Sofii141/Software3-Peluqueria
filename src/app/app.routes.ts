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

import { CategoriasListarComponent } from './categorias/categorias-listar/categorias-listar.component';
import { CategoriasCrearComponent } from './categorias/categorias-crear/categorias-crear.component';
import { CategoriasEditarComponent } from './categorias/categorias-editar/categorias-editar.component';

import { ConfigurarHorariosComponent } from './estilistas/configurar-horarios/configurar-horarios.component';

import { AgendaCalendarioComponent } from './agenda-estilista/agenda-calendario/agenda-calendario.component';
import { ReservaDetalleComponent } from './agenda-estilista/reserva-detalle/reserva-detalle.component';
import { estilistaGuard } from './auth/estilista.guard';
import {ReservarCitaComponent} from "./clientes/reservar-cita/reservar-cita.component";
import {clienteGuard} from "./auth/cliente.guard";

export const routes: Routes = [
    { path: '', component: HomeComponent },

    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },

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

    { path: 'ofertas', component: OfertasComponent },
    { path: 'cupones', component: CuponesComponent },
    { path: 'ayuda', component: AyudaComponent },

    {
      path: 'admin/categorias',
      component: CategoriasListarComponent,
      canActivate: [adminGuard]
    },
    {
      path: 'admin/categorias/crear',
      component: CategoriasCrearComponent,
      canActivate: [adminGuard]
    },
    {
      path: 'admin/categorias/editar/:id',
      component: CategoriasEditarComponent,
      canActivate: [adminGuard]
    },

    {
      path: 'admin/estilistas/horarios/:id',
      component: ConfigurarHorariosComponent,
      canActivate: [adminGuard]
    },

  // ========== RUTAS DE ESTILISTA ==========
  {
    path: 'estilista',
    canActivate: [estilistaGuard],
    children: [
      {
        path: 'agenda',
        component: AgendaCalendarioComponent
      },
      {
        path: 'agenda/detalle/:id',
        component: ReservaDetalleComponent
      }
    ]
  },

  {
    path: 'cliente/reservar',
    component: ReservarCitaComponent,
    canActivate: [clienteGuard]  // o el guard que uses para clientes
  },
    // Redirige cualquier otra ruta a la página de inicio
    { path: '**', redirectTo: '', pathMatch: 'full' }
];
