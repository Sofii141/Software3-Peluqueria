/**
 * Datos que se ENVÍAN al backend para iniciar sesión.
 * Coincide con el DTO LoginDto en .NET.
 */
export interface LoginRequest {
  username: string;
  password: string;
}

/**
 * Datos que se ENVÍAN al backend para registrar un nuevo usuario.
 * Coincide con el DTO RegisterDto en .NET.
 */
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  nombreCompleto: string;
  telefono: string;
}

/**
 * Datos que RECIBIMOS del backend tras login o registro exitoso.
 * Coincide con el DTO NewUserDto en .NET.
 */
export interface LoginResponse {
  userName: string; 
  email: string;
  token: string;
}
