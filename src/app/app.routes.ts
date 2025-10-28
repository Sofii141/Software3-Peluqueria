import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ListarServiciosComponent } from './servicios/listar-servicios/listar-servicios.component';
import { CrearServicioComponent } from './servicios/crear-servicio/crear-servicio.component';
import { ActualizarServicioComponent } from './servicios/actualizar-servicio/actualizar-servicio.component';
import { OfertasComponent } from './ofertas/ofertas.component';
import { CuponesComponent } from './cupones/cupones.component';
import { AyudaComponent } from './ayuda/ayuda.component';
import { adminGuard } from './auth/auth.guard';
// 1. IMPORTAR LOS NUEVOS COMPONENTES
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },

    // 2. AÑADIR RUTAS DE AUTENTICACIÓN
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    
    // Rutas de la funcionalidad principal (Servicios de la peluquería)
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

    // Rutas "Borrador"
    { path: 'ofertas', component: OfertasComponent },
    { path: 'cupones', component: CuponesComponent },
    { path: 'ayuda', component: AyudaComponent },
    
    // Redirige cualquier otra ruta a la página de inicio
    { path: '**', redirectTo: '', pathMatch: 'full' }
];