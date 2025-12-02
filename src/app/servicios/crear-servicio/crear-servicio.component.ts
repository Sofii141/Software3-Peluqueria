import { Component, OnInit } from '@angular/core';
import { Servicio } from '../modelos/servicio';
import { ServicioService } from '../servicios/servicio.service';
import { Router } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
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
    this.servicio.categoria = null!;
    this.servicio.disponible = true;
    this.servicio.duracionMinutos = 45; // Valor por defecto
    this.categoriaService.getCategorias().subscribe(
      categorias => this.categorias = categorias
    );
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

  public crearServicio(): void {
    // Validación de imagen
    if (!this.selectedFile) {
      Swal.fire('Error de validación', 'Debe seleccionar una imagen para el servicio.', 'error');
      return;
    }

    // Validación de categoría
    if (!this.servicio.categoria || this.servicio.categoria.id === 0) {
      Swal.fire('Error de validación', 'Debe seleccionar una categoría para el servicio.', 'error');
      return;
    }

    // Validación de duración (45-480 minutos)
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

    const formData = new FormData();
    formData.append('Nombre', this.servicio.nombre);
    formData.append('Descripcion', this.servicio.descripcion);
    formData.append('DuracionMinutos', this.servicio.duracionMinutos.toString());
    formData.append('Precio', this.servicio.precio.toString());
    formData.append('Disponible', this.servicio.disponible.toString());
    formData.append('CategoriaId', this.servicio.categoria.id.toString());
    formData.append('Imagen', this.selectedFile, this.selectedFile.name);

    this.servicioService.createWithImage(formData).subscribe({
      next: (response) => {
        this.router.navigate(['/servicios']);
        Swal.fire('Nuevo Servicio', `Servicio ${response.nombre} creado con éxito!`, 'success');
      },
      error: (err) => {
        console.error('Error detallado al crear el servicio:', err);
      }
    });
  }

  compararCategoria(c1: Categoria, c2: Categoria): boolean {
    return c1 && c2 ? c1.id === c2.id : c1 === c2;
  }
}
