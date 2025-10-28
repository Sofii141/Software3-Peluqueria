import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { CommonModule } from '@angular/common';
import { Servicio } from '../modelos/servicio';
import { ServicioService } from '../servicios/servicio.service';
import { Categoria } from '../../categorias/modelos/categoria';
import { CategoriaService } from '../../categorias/servicios/categoria.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-actualizar-servicio',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './actualizar-servicio.component.html',
  styleUrls: ['./actualizar-servicio.component.css']
})
export class ActualizarServicioComponent implements OnInit {

  public servicio: Servicio = new Servicio();
  public categorias: Categoria[] = [];
  public titulo: string = 'Actualizar Servicio';

  public selectedFile: File | null = null;
  public imagePreview: string | ArrayBuffer | null = null;

  constructor(
    private categoriaService: CategoriaService,
    private servicioService: ServicioService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

    ngOnInit(): void {
    // Es mejor cargar las categorías primero
    this.categoriaService.getCategorias().subscribe(categorias => {
      this.categorias = categorias;
      
      // Una vez que tenemos las categorías, cargamos el servicio
      const servicioId = this.route.snapshot.paramMap.get('id');
      if (servicioId) {
        this.servicioService.getServicioById(+servicioId).subscribe(servicio => {
          this.servicio = servicio;
          
          // Este 'if' es nuestra garantía de que 'servicio.categoria' no es null.
          if (servicio.categoria) {

            // --- ÚNICO CAMBIO AQUÍ ---
            // Añadimos '!' para asegurarle a TypeScript que 'servicio.categoria' no es null.
            this.servicio.categoria = this.categorias.find(cat => cat.id === servicio.categoria!.id) || null;
          }
        });
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result;
      };
      reader.readAsDataURL(file);
    }
  }

  public actualizarServicio(): void {
    if (!this.servicio.categoria) {
        Swal.fire('Error', 'Debe seleccionar una categoría para el servicio.', 'error');
        return;
    }

    const formData = new FormData();
    formData.append('Nombre', this.servicio.nombre);
    formData.append('Descripcion', this.servicio.descripcion);
    formData.append('Precio', this.servicio.precio.toString());
    formData.append('Disponible', this.servicio.disponible.toString());
    
    // --- ÚNICO CAMBIO AQUÍ ---
    // Añadimos '!' para decirle a TypeScript que confiamos en que 'categoria' no es null.
    formData.append('CategoriaId', this.servicio.categoria!.id.toString());
    
    if (this.selectedFile) {
      formData.append('Imagen', this.selectedFile, this.selectedFile.name);
    }

    this.servicioService.updateWithImage(this.servicio.id, formData).subscribe({
      next: (response: Servicio) => {
        this.router.navigate(['/servicios']);
        Swal.fire('Servicio Actualizado', `El servicio ${response.nombre} se actualizó con éxito!`, 'success');
      },
      error: (err: HttpErrorResponse) => { 
        console.error('Error al actualizar el servicio:', err);
      }
    });
  }

  compararCategoria(c1: Categoria, c2: Categoria): boolean {
    return c1 && c2 ? c1.id === c2.id : c1 === c2;
  }
}