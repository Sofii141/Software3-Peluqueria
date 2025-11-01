import { Component, OnInit } from '@angular/core';
import { Servicio } from '../modelos/servicio';
import { ServicioService } from '../servicios/servicio.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { Categoria } from '../../categorias/modelos/categoria';
import { CategoriaService } from '../../categorias/servicios/categoria.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-crear-servicio',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './crear-servicio.component.html',
  styleUrls: ['./crear-servicio.component.css']
})
export class CrearServicioComponent implements OnInit {
  public servicio: Servicio = new Servicio(); 
  public categorias: Categoria[] = [];
  public titulo: String = 'Registrar Nuevo Servicio';

  public selectedFile!: File;
  public imagePreview: string | ArrayBuffer | null = null;

  constructor(
    private categoriaService: CategoriaService,
    private servicioService: ServicioService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // --- CORREGIDO ---
    this.servicio.categoria = null!; // Inicializamos la categoría a null
    this.servicio.disponible = true;
    this.categoriaService.getCategorias().subscribe(
      categorias => this.categorias = categorias
    );
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


  public crearServicio(): void {
    if (!this.selectedFile) {
      Swal.fire('Error de validación', 'Debe seleccionar una imagen para el servicio.', 'error');
      return;
    }
    // Verificación adicional para la categoría
    if (!this.servicio.categoria || this.servicio.categoria.id === 0) {
      Swal.fire('Error de validación', 'Debe seleccionar una categoría para el servicio.', 'error');
      return;
    }

    const formData = new FormData();

    // Añadimos cada propiedad del servicio como un campo separado.
    // Las claves deben coincidir con las propiedades del DTO del backend.
    formData.append('Nombre', this.servicio.nombre);
    formData.append('Descripcion', this.servicio.descripcion);
    formData.append('Precio', this.servicio.precio.toString());
    formData.append('Disponible', this.servicio.disponible.toString());
    formData.append('CategoriaId', this.servicio.categoria.id.toString());
    
    // La clave para la imagen debe ser 'Imagen' (con 'I' mayúscula) para coincidir con el DTO.
    formData.append('Imagen', this.selectedFile, this.selectedFile.name);

    // Llamamos al servicio con el FormData correctamente construido.
    this.servicioService.createWithImage(formData).subscribe({
      next: (response) => {
        this.router.navigate(['/servicios']);
        Swal.fire('Nuevo Servicio', `Servicio ${response.nombre} creado con éxito!`, 'success');
      },
      error: (err) => {
        // El handleError de tu servicio ya muestra un Swal genérico,
        // pero aquí podemos loguear el error específico si queremos.
        console.error('Error detallado al crear el servicio:', err);
      }
    });
  }

  compararCategoria(c1: Categoria, c2: Categoria): boolean {
    // Se mantiene igual, es correcto
    return c1 && c2 ? c1.id === c2.id : c1 === c2;
  }
}