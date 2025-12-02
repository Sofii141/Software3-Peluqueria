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
    this.categoriaService.getCategorias().subscribe(categorias => {
      this.categorias = categorias;
      const servicioId = this.route.snapshot.paramMap.get('id');
      if (servicioId) {
        this.servicioService.getServicioById(+servicioId).subscribe(servicio => {
          this.servicio = servicio;

          console.log('Servicio cargado:', this.servicio);
          console.log('URL de la imagen a cargar:', this.servicio.imagen);

          if (servicio.categoria) {
            this.servicio.categoria = this.categorias.find(cat => cat.id === servicio.categoria!.id) || null;
          }
        });
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validar tipo de archivo
      const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg'];
      if (!allowedTypes.includes(file.type)) {
        Swal.fire('Formato inválido', 'Solo se permiten imágenes JPG, JPEG o PNG.', 'warning');
        return;
      }

      // Validar tamaño (máx 5MB)
      if (file.size > 5 * 1024 * 1024) {
        Swal.fire('Archivo muy grande', 'La imagen no debe superar los 5MB.', 'warning');
        return;
      }

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

    // Validación de duración
    if (this.servicio.duracionMinutos < 45) {
      Swal.fire('Duración inválida', 'La duración mínima es de 45 minutos.', 'warning');
      return;
    }
    if (this.servicio.duracionMinutos > 480) {
      Swal.fire('Duración inválida', 'La duración máxima es de 480 minutos (8 horas).', 'warning');
      return;
    }

    // Validación de precio
    if (this.servicio.precio <= 0) {
      Swal.fire('Precio inválido', 'El precio debe ser mayor a cero.', 'warning');
      return;
    }

    // Verificar si tiene reservas futuras
    this.servicioService.verificarReservasFuturas(this.servicio.id).subscribe({
      next: (tieneReservas) => {
        if (tieneReservas) {
          Swal.fire({
            title: 'Servicio con Reservas Activas',
            text: 'Este servicio tiene citas futuras programadas. Los cambios pueden afectar a los clientes.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Continuar de todas formas',
            cancelButtonText: 'Cancelar'
          }).then((result) => {
            if (result.isConfirmed) {
              this.ejecutarActualizacion();
            }
          });
        } else {
          this.ejecutarActualizacion();
        }
      },
      error: () => {
        // Si falla la verificación, preguntamos al usuario
        Swal.fire({
          title: 'No se pudo verificar las reservas',
          text: '¿Desea continuar con la actualización?',
          icon: 'question',
          showCancelButton: true,
          confirmButtonText: 'Sí, continuar',
          cancelButtonText: 'Cancelar'
        }).then((result) => {
          if (result.isConfirmed) {
            this.ejecutarActualizacion();
          }
        });
      }
    });
  }

  private ejecutarActualizacion(): void {
    const formData = new FormData();
    formData.append('Nombre', this.servicio.nombre);
    formData.append('Descripcion', this.servicio.descripcion);
    formData.append('DuracionMinutos', this.servicio.duracionMinutos.toString());
    formData.append('Precio', this.servicio.precio.toString());
    formData.append('Disponible', this.servicio.disponible.toString());
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
