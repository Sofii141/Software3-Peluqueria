import { Component } from '@angular/core';
import { FeaturedServicesComponent } from '../featured-services/featured-services.component';
import { HeroCarouselComponent } from '../hero-carousel/hero-carousel.component';
import { EstilistasComponent } from '../estilistas/estilistas.component';
import { ProcesoReservaComponent } from '../proceso-reserva/proceso-reserva.component';
import { UbicacionContactoComponent } from '../ubicacion-contacto/ubicacion-contacto.component';
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [HeroCarouselComponent, FeaturedServicesComponent, EstilistasComponent, ProcesoReservaComponent, UbicacionContactoComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css' 
})
export class HomeComponent {
}